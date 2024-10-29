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


        public UserService(T2305mApiContext context, IUserRepository userRepository, IWebHostEnvironment env,  IMapper mapper)
        {
            _userRepository = userRepository;
            _env = env;
            _mapper = mapper;
            _context = context;
        }
//        public async Task<GetDetailUserDTO> GetDetailUserDTOByIdAsync(int userId)
//        {
//            // Fetch the user entity by ID
///*            var userEntity = await _userRepository.GetUserByIdAsync(userId)*/;
//            var userEntity = await _userRepository.GetUserByIdAsync(userId,  true, true);


//            if (userEntity == null)
//            {
//                return null; // Or throw an appropriate exception if you prefer
//            }

//            //var variable =  userEntity.UserEvents.Count;
//            // Map the user entity to the GetDetailUserDTO
//            var detailUserDTO = new GetDetailUserDTO
//            {
//                UserId = userEntity.UserId,
//                FullName = userEntity.FullName,
//                Email = userEntity.Email,
//                Age = userEntity.Age,
//                Education = userEntity.Education,
//                ShortBiography = userEntity.ShortBiography,
//                LongBiography = userEntity.LongBiography,
//                PhotoUrl = userEntity.PhotoUrl,
//                Facebook = userEntity.Facebook,
//                LinkedIn = userEntity.LinkedIn,
//                Twitter = userEntity.Twitter,
//                PersonalWebsiteUrl = userEntity.PersonalWebsiteUrl,
//                PersonalWebsiteTitle = userEntity.PersonalWebsiteTitle,
//                ReceiveNotifications = userEntity.ReceiveNotifications,
//                IsActive = userEntity.IsActive,
                

//                // Mapping UserEvents to BasicUserSavedEventDTO
//                BasicUserSavedEvents = userEntity.UserEvents?
//                    .Where(e => e.Event != null) // Ensure that the Event is not null
//                    .Select(e =>  _mapper.Map<GetBasicEventDTO>(e.Event))
//                    .ToList(),

//                // Mapping UserArticles to BasicUserArticleDTO
//                BasicUserArticles = userEntity.UserArticles?.Select(a => new GetBasicUserArticleDTO
//                {
//                    UserArticleId = a.UserArticleId,
//                    Title = a.Title,
//                    Description = a.Description,
//                    ThumbnailImage = a.ThumbnailImage,
//                    IsPromoted = a.IsPromoted,
//                    UserId = a.UserId,
//                    UserName = a.User != null ? a.User.FullName : "Unknown", // Assuming User has a Name property
//                    CreatedAt = a.CreatedAt,
//                    Status = a.Status,
//                    UserArticleTags = a.userArticleUserArticleTags?.Select(tag => new UserArticleTagDTO
//                    {
//                        UserArticleTagId = tag.UserArticleTag.UserArticleTagId,
//                        Name = tag.UserArticleTag.Name
//                    }).ToList()
//                }).ToList()
//            };

//            return detailUserDTO;
//        }

//        public async Task<UpdateUserResponseDTO> UpdateUserAsync(int userId, UpdateUserDTO updateUserDTO)
//        {
//            UpdateUserResponseDTO updateUserResponseDTO = await _userRepository.UpdateUserAsync( userId,  updateUserDTO);
//            return updateUserResponseDTO;
//        }

//        public async Task<Dictionary<string, List<string>>> ValidateUpdateUserDTO(UpdateUserDTO updateUserDTO)
//        {
//            var errors = new Dictionary<string, List<string>>();

//            //Validate CustomerId
//            //if (updateUserDTO.UserId <= 0 || updateUserDTO.UserId != UserId)
//            //{
//            //    AddError(errors, "UserId", "UserId is not provided or UserId mismatch.");
//            //}

//            return errors.Count > 0 ? errors : null;
//        }

//        private static void AddError(Dictionary<string, List<string>> errors, string key, string errorMessage)
//        {
//            if (!errors.ContainsKey(key))
//            {
//                errors[key] = new List<string>();
//            }
//            errors[key].Add(errorMessage);
//        }


        public async Task<Object> UploadAvatarAsync(int userId, IFormFile file)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
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

            return new 
            {
                FilePath = user.Avatar,
                Message  = "File Uploaded Successfully",
            };
        }
    }
}
