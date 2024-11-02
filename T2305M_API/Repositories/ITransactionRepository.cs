using T2305M_API.DTO.Transaction;
using T2305M_API.Entities;

namespace T2305M_API.Repositories
{
    public interface ITransactionRepository
    {
        Task<(IEnumerable<Transaction> Data, int TotalItems)> GetTransactionsAsync(TransactionQueryParameters queryParameters);
        Task <Transaction>CreateTransactionAsync(CreateTransactionDTO createTransactionDTO);
        Task <Decimal> CalculateTotalTransferedAmountPerDay(string accountNumber);

    }
}
