using T2305M_API.DTO.CheckBook;
using T2305M_API.Entities;

namespace T2305M_API.Repositories
{
    public interface ICheckBookRepository
    {
        Task<(IEnumerable<CheckBook> Data, int TotalItems)> GetCheckBooksAsync(CheckBookQueryParameters queryParameters);
        //Task <CheckBook>CreateCheckBookAsync(CreateCheckBookDTO createCheckBookDTO);
        //Task <Decimal> CalculateTotalTransferedAmountPerDay(string accountNumber);

    }
}
