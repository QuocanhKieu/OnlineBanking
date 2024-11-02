using T2305M_API.DTO.Account;
using T2305M_API.Entities;

namespace T2305M_API.Repositories
{
    public interface IAccountRepository
    {
        Task<(IEnumerable<Account> Data, int TotalItems)> GetAccountsAsync(AccountQueryParameters queryParameters);
        Task<IEnumerable<Account>> ListLikeAccountsAsync(string accountNumber);
        Task CreateAccountAsync(CreateAccountDTO createAccountDTO, int userId);
    }
}
