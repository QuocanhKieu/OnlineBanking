using T2305M_API.DTO.Account;
using T2305M_API.DTO.Transaction;
using T2305M_API.Entities;
using T2305M_API.Models;

namespace T2305M_API.Services
{
    public interface IAccountService
    {
        Task<PaginatedResult<GetBasicAccountDTO>> GetBasicAccountsAsync(AccountQueryParameters queryParameters);
        Task<Account> CheckExistingAccountAsync(CheckDuplicateAccountDTO checkDuplicateAccountDTO);
        Task<GetDetailAccountDTO> GetDetailAccountDTOAsync(string accountNumber, int userId);
        Task<bool> CheckAccountBalance(CheckBalance checkBalance);
        Task<Account> UpdateAccountBalance(decimal newBalance, string accountNumber);
        Task<string> GenerateAccountStatementReport(TransactionQueryParameters queryParameters);

        Task<string> GenerateAndStoreOtpAsync(string email);

        Task<bool> VerifyOtpAsync(string email, string otp);

        Task SendOtpEmailAsync(string email, string otp);


    }
}
