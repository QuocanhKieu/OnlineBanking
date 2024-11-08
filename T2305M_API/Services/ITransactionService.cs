using T2305M_API.DTO.Transaction;
using T2305M_API.Entities;
using T2305M_API.Models;

namespace T2305M_API.Services
{
    public interface ITransactionService
    {
        Task<PaginatedResult<GetBasicTransactionDTO>> GetBasicTransactionsAsync(TransactionQueryParameters queryParameters);
        Task<List<GetBasicTransactionDTO>> GetAllBasicTransactionsAsync(TransactionQueryParameters queryParameters);


    }
}
