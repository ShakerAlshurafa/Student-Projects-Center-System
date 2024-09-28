using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using StudentProjectsCenterSystem.Services;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<LocalUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<LocalUser> signInManager;
        private readonly IMapper mapper;
        private readonly ITokenServices tokenServices;

        public UserRepository(ApplicationDbContext dbContext, 
                              UserManager<LocalUser> userManager, 
                              RoleManager<IdentityRole> roleManager, 
                              SignInManager<LocalUser> signInManager,
                              IMapper mapper,
                              ITokenServices tokenServices)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.mapper = mapper;
            this.tokenServices = tokenServices;
        }

        public bool IsUniqueUser(string email)
        {
            return dbContext.LocalUsers.FirstOrDefault(u => u.Email == email) == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await userManager.FindByEmailAsync(loginRequestDTO.Email);

            if (user == null)
            {
                return new LoginResponseDTO
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            var checkPassword = await signInManager.CheckPasswordSignInAsync(user, loginRequestDTO.Password, false);
            if (!checkPassword.Succeeded)
            {
                return new LoginResponseDTO()
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            var role = await userManager.GetRolesAsync(user);
            return new LoginResponseDTO()
            {
                User = mapper.Map<LocalUserDTO>(user),
                Token = await tokenServices.CreateTokenAsync(user),
                Role = role.ToList(),
            };
        }

        public async Task<ApiResponse> Register(RegisterationRequestDTO registerationRequestDTO)
        {
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

                    string role = registerationRequestDTO.Role == UserRole.Customer ? "Customer" : "Student";

                    // Assign roles to the user
                    var addRolesResult = await userManager.AddToRoleAsync(user, role);
                    if (!addRolesResult.Succeeded)
                    {
                        var errors = addRolesResult.Errors.Select(e => e.Description);
                        return new ApiValidationResponse(errors, 400);
                    }

                    // If everything is successful, commit the transaction
                    await transaction.CommitAsync();

                    // Return success response
                    return new ApiResponse(201, "User registered successfully", result: new LocalUserDTO { UserName = user.UserName, Email = user.Email });
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
