using Microsoft.CodeAnalysis.FlowAnalysis;
using T2305M_API.DTO.Account;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Repositories;
using T2305M_API.Repositories.Implements;
using T2305M_API.Services;
using Microsoft.EntityFrameworkCore;
using T2305M_API.DTO.Account;
using ClosedXML.Excel;

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

    public async Task<Account> CheckExistingAccountAsync(CheckDuplicateAccountDTO checkDuplicateAccountDTO)
    {
        var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == checkDuplicateAccountDTO.AccountNumber);
        return existingAccount;
    }

    public async Task<bool> CheckAccountBalance(CheckBalance checkBalance)
    {
        var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == checkBalance.AccountNumber);
        if (existingAccount == null) throw new Exception("Account does not exist. Please check again!");
        return existingAccount.Balance >= checkBalance.MoneyAmount;
    }

    public async Task<GetDetailAccountDTO> GetDetailAccountDTOAsync(string accountNumber, int userId)
    {
        var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == accountNumber && u.UserId== userId);

        if (existingAccount == null)
        {
            return null; // Or throw an appropriate exception
        }


        var detailAccountDTO = new GetDetailAccountDTO
        {
            //AccountId = existingAccount.AccountId,
            AccountNumber = existingAccount.AccountNumber,
            Balance = existingAccount.Balance,
            isDefault = existingAccount.IsDefault,
            Status = existingAccount.Status
        };

        return detailAccountDTO;
    }
    public async Task<Account> UpdateAccountBalance(decimal newBalance , string accountNumber)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);
        if (account == null)
        {
            return null; // Or throw an appropriate exception
        }
        account.Balance = newBalance;
        await _context.SaveChangesAsync();
        return account;
    }





    public string GenerateAccountStatementReport(int accountNumber)
    {
        // Define paths
        string templatePath = Path.Combine(_env.WebRootPath, "Templates", "ExcelTemplate", "account_statement.xlsx");
        string reportFolder = Path.Combine(_env.WebRootPath, "Reports");
        string documentName = $"AccountStatement-{accountNumber}-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        string fullPath = Path.Combine(reportFolder, documentName);

        // Ensure the report directory exists
        if (!Directory.Exists(reportFolder))
        {
            Directory.CreateDirectory(reportFolder);
        }

        try
        {
            // Fetch transactions for the account
            List<Transaction> transactions = GetTransactionsByAccountId(accountId);

            // Load template and fill it
            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheet("Statement"); // Change "Statement" to match the template sheet name
                int startRow = 2; // Assuming headers are in row 1

                foreach (var transaction in transactions)
                {
                    worksheet.Cell(startRow, 1).Value = transaction.Date;
                    worksheet.Cell(startRow, 2).Value = transaction.Description;
                    worksheet.Cell(startRow, 3).Value = transaction.Amount;
                    worksheet.Cell(startRow, 4).Value = transaction.Balance;
                    startRow++;
                }

                // Save the filled template
                workbook.SaveAs(fullPath);
            }

            return fullPath;
        }
        catch (Exception ex)
        {
            // Handle exceptions (logging, rethrowing, etc.)
            throw new Exception("Error generating account statement report.", ex);
        }
    }
}