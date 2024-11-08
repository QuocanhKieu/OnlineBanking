using T2305M_API.DTO.CheckBook;
using T2305M_API.Entities;
using T2305M_API.Models;

namespace T2305M_API.Services
{
    public interface ICheckBookService
    {
        Task<PaginatedResult<GetDetailCheckBookDTO>> GetCheckBooksAsync(CheckBookQueryParameters queryParameters);
        Task<CheckBook> CreateCheckBookAsync(CreateCheckBookDTO createCheckBookDTO, int userId, int accountId);

        Task<bool> CreateChecksAsync(CheckBook checkBook, int Quantity);


    }
}
