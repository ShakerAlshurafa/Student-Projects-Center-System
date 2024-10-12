using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain;
using StudentProjectsCenterSystem.Core.Entities.DTO.Authentication;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IUserRepository
    {
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<ApiResponse> Register(RegisterationRequestDTO registerationRequestDTO);

        bool IsUniqueUser(string Email);
    }
}
