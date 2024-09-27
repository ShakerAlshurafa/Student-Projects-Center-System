using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<LocalUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserRepository(ApplicationDbContext dbContext, UserManager<LocalUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public bool IsUniqueUser(string email)
        {
            return dbContext.LocalUsers.FirstOrDefault(u => u.Email == email) == null;
        }

        public Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            throw new NotImplementedException();
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

                    // Validate the roles exist, if not return an error
                    foreach (var role in registerationRequestDTO.Roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            return new ApiValidationResponse(new List<string> { $"Role '{role}' does not exist." }, 400);
                        }
                    }

                    // Assign roles to the user
                    var addRolesResult = await userManager.AddToRolesAsync(user, registerationRequestDTO.Roles);
                    if (!addRolesResult.Succeeded)
                    {
                        var errors = addRolesResult.Errors.Select(e => e.Description);
                        return new ApiValidationResponse(errors, 400);
                    }

                    // If everything is successful, commit the transaction
                    await transaction.CommitAsync();

                    // Return success response
                    return new ApiResponse(201, "User registered successfully", result:new LocalUserDTO { UserName = user.UserName, Email = user.Email });
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
