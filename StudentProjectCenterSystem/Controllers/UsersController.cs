using AutoMapper;
using Microsoft.AspNetCore.Http;
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

        public UsersController(IUnitOfWork<LocalUser> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        // test

        [HttpGet("get-users")]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? userName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = null!;
            if (!string.IsNullOrEmpty(userName))
            {
                filter = x => x.UserName.Contains(userName);
            }
            
            var model = await unitOfWork.userRepository.GetAll(filter, PageSize, PageNumber, "UserProjects");

            var users = model.Select(u => new UserDTO
            {
                Id = u.Id,
                UserName = u.UserName ?? "",
                Role = u.UserProjects.Single().Role,
            });

            return new ApiResponse(200, "Users retrieved successfully", users);
        }

    }
}
