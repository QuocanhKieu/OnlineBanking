using Microsoft.EntityFrameworkCore;
using T2305M_API.DTO.User;
using T2305M_API.DTO.User;
using T2305M_API.Entities;

namespace T2305M_API.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly T2305mApiContext _context;

        public UserRepository(T2305mApiContext context, IWebHostEnvironment env)
        {
            _context = context;
        }

    }

}
