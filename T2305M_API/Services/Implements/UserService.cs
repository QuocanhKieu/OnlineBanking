using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using T2305M_API.DTO.User;
using T2305M_API.Entities;
using T2305M_API.Repositories;
using T2305M_API.Repositories.Implements;

namespace T2305M_API.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _env;
        private readonly T2305mApiContext _context;
        private readonly IMapper _mapper;


        public UserService(T2305mApiContext context, IUserRepository userRepository, IWebHostEnvironment env, IMapper mapper)
        {
            _userRepository = userRepository;
            _env = env;
            _mapper = mapper;
            _context = context;
        }
        public async Task<GetDetailUserDTO> GetDetailUserDTOAsync(int userId)
        {
            try
            {
                var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);


                if (userEntity == null)
                {
                    throw new Exception("User not Found"); // Or throw an appropriate exception if you prefer
                }

                //var variable =  userEntity.UserEvents.Count;
                // Map the user entity to the GetDetailUserDTO
                var detailUserDTO = new GetDetailUserDTO
                {
                   CustomerId = userEntity.CustomerId,
                   Phone = userEntity.Phone,
                   Name = userEntity.Name,
                   CitizenId = userEntity.CitizenId,
                   CitizenIdFront = userEntity.CitizenIdFront,
                   CitizenIdRear = userEntity.CitizenIdRear,
                   DigitalSignature = userEntity.DigitalSignature,
                   Address = userEntity.Address,
                   Avatar = userEntity.Avatar,
                   CreatedAt = userEntity.CreatedAt,
                };

                return detailUserDTO;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        //public async Task<UpdateUserResponseDTO> UpdateUserAsync(int userId, UpdateUserDTO updateUserDTO)
        //{
        //    UpdateUserResponseDTO updateUserResponseDTO = await _userRepository.UpdateUserAsync(userId, updateUserDTO);
        //    return updateUserResponseDTO;
        //}

        public async Task<User> UploadAvatarAsync(int userId, IFormFile file)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            var directoryPath = Path.Combine(_env.WebRootPath, "uploads/images");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception("Invalid file type. Only JPEG and PNG files are allowed.");
            }

            // Generate new file path
            var fileName = $"{Guid.NewGuid().ToString()}_{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_env.WebRootPath, "uploads/images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update user's AvatarUrl
            user.Avatar = $"/uploads/images/{fileName}";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }
    
    
        public async Task<User> MakeTransPasswordAsync(int userId, TransPasswordDTO transPasswordDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("User not Found"); // Or throw an appropriate exception if you prefer
            }
            if (!string.IsNullOrEmpty(user.TransPassword))
            {
                throw new Exception("TransPassword already exists"); // Or throw an appropriate exception if you prefer
            }
            user.TransPassword = transPasswordDTO.TransPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> CheckTransPasswordExistAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("User not Found"); // Or throw an appropriate exception if you prefer
            }
            return string.IsNullOrEmpty(user.TransPassword); // true là empty/null
        }
        public async Task<bool> VerifyTranspasswordAsync(int userId, string transPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("User not Found"); // Or throw an appropriate exception if you prefer
            }
            return user.TransPassword == transPassword; // true là empty/null
        }


    }
}
