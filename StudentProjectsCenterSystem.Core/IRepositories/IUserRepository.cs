using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IUserRepository
    {
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<ApiResponse> Register(RegisterationRequestDTO registerationRequestDTO);

        bool IsUniqueUser(string Email); 
    }
}
