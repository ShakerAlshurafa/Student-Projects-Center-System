using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StudentProjectsCenter.Core.Entities.DTO.Users;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Authentication;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<LocalUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<LocalUser> signInManager;
        private readonly IMapper mapper;
        private readonly ITokenServices tokenServices;
        private readonly IEmailService emailService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<AuthRepository> _logger; // Define the logger

        public AuthRepository(ApplicationDbContext dbContext,
                              UserManager<LocalUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              SignInManager<LocalUser> signInManager,
                              IMapper mapper,
                              ITokenServices tokenServices,
                              IEmailService emailService,
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<AuthRepository> logger) // Inject the logger

        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.mapper = mapper;
            this.tokenServices = tokenServices;
            this.emailService = emailService;
            this.httpContextAccessor = httpContextAccessor;
            this._logger = logger;
        }

        public bool IsUniqueUser(string email)
        {
            return dbContext.LocalUsers.FirstOrDefault(u => u.Email == email) == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            // Find user by email
            var user = await userManager.FindByEmailAsync(loginRequestDTO.Email);

            // Check if user exists
            if (user == null)
            {
                _logger.LogWarning($"Login failed for user with email: {loginRequestDTO.Email}");
                return new LoginResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                return new LoginResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Please confirm your email before logging in."
                };
            }

            // Check if password is correct
            var checkPassword = await signInManager.CheckPasswordSignInAsync(user, loginRequestDTO.Password, false);
            if (!checkPassword.Succeeded)
            {
                return new LoginResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            // Check if account is locked
            if (await userManager.IsLockedOutAsync(user))
            {
                return new LoginResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Your account is locked. Please try again later."
                };
            }

            // Retrieve user roles
            var roles = await userManager.GetRolesAsync(user);

            // Optionally log the login attempt for auditing
            _logger.LogInformation($"User with email {user.Email} logged in successfully.");

            // Return success response with user data, token, and roles
            return new LoginResponseDTO
            {
                IsSuccess = true,
                User = mapper.Map<LocalUserDTO>(user),
                Token = await tokenServices.CreateTokenAsync(user),
                Role = roles.ToList(),
            };
        }


        public async Task<ApiResponse> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            var emailExist = await userManager.FindByEmailAsync(registerationRequestDTO.Email);
            if (emailExist != null)
            {
                return new ApiResponse(400, "This email already exists. Please log in if it belongs to you.");
            }

            var user = new LocalUser
            {
                UserName = registerationRequestDTO.FirstName + "_" + registerationRequestDTO.LastName,
                Email = registerationRequestDTO.Email,
                NormalizedEmail = registerationRequestDTO.Email.ToUpper(),
                FirstName = registerationRequestDTO.FirstName,
                MiddleName = registerationRequestDTO.MiddleName,
                LastName = registerationRequestDTO.LastName,
                CompanyName = registerationRequestDTO.CompanyName ?? ""
            };

            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Attempt to create the user
                    var result = await userManager.CreateAsync(user, registerationRequestDTO.Password);
                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description);
                        return new ApiValidationResponse(errors, 400);
                    }

                    // Assign roles to the user
                    var addRolesResult = await userManager.AddToRoleAsync(user, "user");
                    if (!addRolesResult.Succeeded)
                    {
                        var errors = addRolesResult.Errors.Select(e => e.Description);
                        return new ApiValidationResponse(errors, 400);
                    }

                    // Generate confirmation token
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    var request = httpContextAccessor?.HttpContext?.Request;
                    var baseUrl = $"{request?.Scheme}://{request?.Host.Value}";

                    // Create confirmation link
                    var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                    var subject = "Email Confirmation";
                    var message = $@"
                        <p>Please confirm your email by clicking the button below:</p>
                        <br>
                        <a href='{confirmationLink}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #28a745; text-decoration: none; border-radius: 5px;'>Confirm Email</a>
                        <p>If you did not request this, please ignore this email.</p>";


                    // Send confirmation email
                    var emailSent = await emailService.SendEmailAsync(user.Email, subject, message, isHtml: true);

                    if (!emailSent.IsSuccess)
                    {
                        return new ApiResponse(500, $"An error occurred while sending the message: {emailSent.ErrorMessage}");
                    }

                    // If everything is successful, commit the transaction
                    await transaction.CommitAsync();

                    // Return success response
                    return new ApiResponse(201, "User registered successfully",
                            result: new LocalUserDTO { UserName = user.UserName, Email = user.Email });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse(500, ex.Message);
                }
            }
        }

    }
}
