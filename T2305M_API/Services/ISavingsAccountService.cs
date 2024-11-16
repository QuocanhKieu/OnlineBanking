using T2305M_API.DTO.SavingsAccount;
using T2305M_API.DTO.Transaction;
using T2305M_API.Entities;
using T2305M_API.Models;

namespace T2305M_API.Services
{
    public interface ISavingsAccountService
    {
        Task<PaginatedResult<GetDetailSavingsAccountDTO>> GetBasicSavingsAccountsAsync(SavingsAccountQueryParameters queryParameters);
        public decimal GetInterestRate(int termInMonths);

    }
}
