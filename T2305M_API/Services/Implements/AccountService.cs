using Microsoft.CodeAnalysis.FlowAnalysis;
using T2305M_API.DTO.Account;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Repositories;
using T2305M_API.Repositories.Implements;
using T2305M_API.Services;
using Microsoft.EntityFrameworkCore;
using T2305M_API.DTO.Account;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IWebHostEnvironment _env;
    private readonly T2305mApiContext _context;


    public AccountService(IAccountRepository accountRepository,
        IWebHostEnvironment env,
        T2305mApiContext context)
    {
        _accountRepository = accountRepository;
        _env = env;
        _context = context;

    }

    public async Task<PaginatedResult<GetBasicAccountDTO>> GetBasicAccountsAsync(AccountQueryParameters queryParameters)
    {
        var (data, totalItems) = await _accountRepository.GetAccountsAsync(queryParameters);

        var basicAccountDTOs = new List<GetBasicAccountDTO>();

        foreach (var account in data)
        {
            basicAccountDTOs.Add(new GetBasicAccountDTO
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                isDefault = account.IsDefault,
                Status = account.Status,
            });
        }
        // Calculate total pages
        int totalPages = (int)Math.Ceiling((double)totalItems / queryParameters.PageSize);

        return new PaginatedResult<GetBasicAccountDTO>
        {
            TotalItems = totalItems,
            PageSize = queryParameters.PageSize,
            CurrentPage = queryParameters.Page,
            TotalPages = totalPages,
            HasNextPage = queryParameters.Page < totalPages,
            HasPreviousPage = queryParameters.Page > 1,
            Data = basicAccountDTOs
        };
    }

    public async Task<Account> CheckDuplicateAccountAsync(CheckDuplicateAccountDTO checkDuplicateAccountDTO)
    {
        var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == checkDuplicateAccountDTO.AccountNumber);

        return existingAccount;
    }

    public async Task<bool> CheckAccountBalance(CheckBalance checkBalance)
    {
        var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == checkBalance.AccountNumber);

        return existingAccount.Balance >= checkBalance.MoneyAmount ;
    }

    public async Task<GetDetailAccountDTO> GetDetailAccountDTOAsync(string accountNumber)
    {
        var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);

        if (existingAccount == null)
        {
            return null; // Or throw an appropriate exception
        }


        var detailAccountDTO = new GetDetailAccountDTO
        {
            AccountId = existingAccount.AccountId,
            AccountNumber = existingAccount.AccountNumber,
            Balance = existingAccount.Balance,
            isDefault = existingAccount.IsDefault,
            Status = existingAccount.Status
        };

        return detailAccountDTO;
    }


}