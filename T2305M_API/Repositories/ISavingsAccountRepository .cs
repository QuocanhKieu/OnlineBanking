using DocumentFormat.OpenXml.Spreadsheet;
using T2305M_API.DTO.SavingsAccount;
using T2305M_API.Entities;

namespace T2305M_API.Repositories
{
    public interface ISavingsAccountRepository
    {
        Task<(IEnumerable<SavingsAccount> Data, int TotalItems)> GetSavingsAccountsAsync(SavingsAccountQueryParameters queryParameters);
      
    }
}
