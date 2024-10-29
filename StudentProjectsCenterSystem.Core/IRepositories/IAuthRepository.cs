using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO.Authentication;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IAuthRepository
    {
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<ApiResponse> Register(RegisterationRequestDTO registerationRequestDTO);

        bool IsUniqueUser(string Email);
    }
}
