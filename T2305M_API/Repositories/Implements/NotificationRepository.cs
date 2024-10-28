using AutoMapper; // Assuming you are using AutoMapper for DTO mapping
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using T2305M_API.DTO.Notification;
using T2305M_API.Entities;
using T2305M_API.Models;

namespace T2305M_API.Repositories.Implements
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly T2305mApiContext _context;
        private readonly IMapper _mapper; // For mapping DTOs to entities and vice versa

        public NotificationRepository(T2305mApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Notification> CreateNotificationAsync(CreateBasicNotificationDTO createBasicNotificationDTO)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var notification = new Notification
                    {
                        Content = createBasicNotificationDTO.Content,
                        UserId = createBasicNotificationDTO.UserId,
                    };

                    await _context.Notifications.AddAsync(notification);
                    await _context.SaveChangesAsync();

                    return notification;
                }
                catch (Exception ex) // Catch other exceptions
                {
                    await transaction.RollbackAsync(); // Rollback the transaction
                    throw ex; // Rethrow the exception after rollback
                }
            }
        }

        public async Task<PaginatedResult<GetBasicNotificationDTO>> GetNotificationsAsync(NotificationQueryParameters queryParameters)
        {
            var query = _context.Notifications.AsQueryable(); // Create a queryable for flexibility

            // Apply filters based on the query parameters
            if (queryParameters.UserId > 0)
            {
                query = query.Where(n => n.UserId == queryParameters.UserId);
            }

            if (queryParameters.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == queryParameters.IsRead);
            }

            // Sorting notifications
            bool isDescending = queryParameters.SortOrder?.ToLower() == "desc";
            switch (queryParameters.SortColumn?.ToLower())
            {
                default:
                    query = isDescending ? query.OrderByDescending(n => n.CreatedAt) : query.OrderBy(n => n.CreatedAt); // Default to sorting by CreatedAt
                    break;
            }

            // Pagination
            int totalItems = await query.CountAsync();
            var pagedData = await query
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .Select(n => _mapper.Map<GetBasicNotificationDTO>(n)) // Map entities to DTOs
                .ToListAsync();

            return new PaginatedResult<GetBasicNotificationDTO>
            {
                TotalItems = totalItems,
                PageSize = queryParameters.PageSize,
                CurrentPage = queryParameters.Page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParameters.PageSize),
                HasNextPage = queryParameters.Page < (int)Math.Ceiling(totalItems / (double)queryParameters.PageSize),
                HasPreviousPage = queryParameters.Page > 1,
                Data = pagedData
            };
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            
        }
    }
}
