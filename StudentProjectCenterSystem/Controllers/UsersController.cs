using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork<LocalUser> unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;

        public UsersController(IUnitOfWork<LocalUser> unitOfWork, IMapper mapper, UserManager<LocalUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        [HttpGet("get-users")]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? userName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = null!;
            if (!string.IsNullOrEmpty(userName))
            {
                filter = x => x.UserName.Contains(userName);
            }

            var usersList = await unitOfWork.userRepository.GetAll(filter, PageSize, PageNumber);

            var userDTOs = new List<UserDTO>();

            foreach (var user in usersList)
            {
                var roles = await userManager.GetRolesAsync(user);
                userDTOs.Add(new UserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }



    }
}
