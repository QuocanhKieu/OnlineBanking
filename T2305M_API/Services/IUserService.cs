using T2305M_API.DTO.User;
using T2305M_API.DTO.User;
using T2305M_API.Entities;

namespace T2305M_API.Services
{
    public interface IUserService
    {
        Task<GetDetailUserDTO> GetDetailUserDTOAsync(int userId);
        //Task<UpdateUserResponseDTO> UpdateUserAsync(int userId, UpdateUserDTO user);
        //Task<Dictionary<string, List<string>>> ValidateUpdateUserDTO( UpdateUserDTO updateUserDTO);

        Task<User> UploadAvatarAsync(int userId, IFormFile file);
        Task<User> MakeTransPasswordAsync(int userId, TransPasswordDTO transPasswordDTO);
        Task<bool> CheckTransPasswordExistAsync(int userId);
        Task<bool> VerifyTranspasswordAsync(int userId, string transPassword);

    }
}

