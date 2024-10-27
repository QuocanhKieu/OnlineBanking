using T2305M_API.DTO.User;
using T2305M_API.Entities;

namespace T2305M_API.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int userId);

        //Task<UpdateUserResponseDTO> UpdateUserAsync(int userId, UpdateUserDTO updateUserDTO);
        Task UpdateUserImageAsync(User user);

    }
}
 