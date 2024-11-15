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
using Azure;
using T2305M_API.DTO.Transaction;
using T2305M_API.Services.Implements;
using Azure.Core;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IWebHostEnvironment _env;
    private readonly T2305mApiContext _context;
    private readonly ITransactionService _transactionService;
    private readonly EmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public AccountService(IAccountRepository accountRepository,
        IWebHostEnvironment env,
            ITransactionService transactionService,
        T2305mApiContext context,
        EmailService emailService
        , IHttpContextAccessor httpContextAccessor

        )
    {
        _accountRepository = accountRepository;
        _env = env;
        _context = context;
        _transactionService = transactionService;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
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
        var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == accountNumber && u.UserId == userId);

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
    public async Task<Account> UpdateAccountBalance(decimal newBalance, string accountNumber)
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

    public async Task<string> GenerateAccountStatementReport(TransactionQueryParameters queryParameters)
    {
        // Define paths
        string templatePath = Path.Combine(_env.WebRootPath, "Templates", "ExcelTemplate", "account_statement.xlsx");
        string reportFolder = Path.Combine(_env.WebRootPath, "Reports");
        string documentName = $"AccountStatement-{queryParameters.AccountNumber}-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        string fullPath = Path.Combine(reportFolder, documentName);

        // Ensure the report directory exists
        if (!Directory.Exists(reportFolder))
        {
            Directory.CreateDirectory(reportFolder);
        }

        try
        {
            // Fetch transactions for the account
            List<GetBasicTransactionDTO> allBasicTransactionsList = await _transactionService.GetAllBasicTransactionsAsync(queryParameters);
            var firstTransaction = allBasicTransactionsList.FirstOrDefault();
            var lastTransaction = allBasicTransactionsList.LastOrDefault();
            decimal totalDebit = 0;
            decimal totalCredit = 0;


            // Load template and fill it
            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheet("Account_Statement"); // Change "Statement" to match the template sheet name


                // here should be the sample code to add value to a certain cell that I want.
                // Assuming cells C3 to G3 are pre-merged in the template
                var mergedRangeCell = worksheet.Cell(4, 11); // Top-left cell of the merged range
                mergedRangeCell.Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); // Format date and time as day/month/year hour:minute:second
                                                                                      //mergedRangeCell.Style.Alignment.WrapText = true; // Optional: wrap text if the content is long

                worksheet.Cell(5, 11).Value = queryParameters.AccountNumber; // Format date and time as day/month/year hour:minute:second

                worksheet.Cell(6, 11).Value = "Customer" + queryParameters.UserId; // Format date and time as day/month/year hour:minute:second

                worksheet.Cell(15, 9).Value = $"From date {queryParameters.StartDate?.ToString("dd/MM/yyyy")}     To date {queryParameters.EndDate?.ToString("dd/MM/yyyy")}";

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == queryParameters.UserId);

                worksheet.Cell(10, 1).Value = "Customer: " + user.Name;
                worksheet.Cell(11, 1).Value = "Email: " + user.Email;
                worksheet.Cell(12, 1).Value = "Address: " + user.Address;
                worksheet.Cell(14, 1).Value = "Phone: " + user.Phone;
                worksheet.Cell(13, 13).Value = lastTransaction.BalanceAfter;
                worksheet.Cell(13, 13).Style.NumberFormat.Format = "0.00"; // or "0.00" for two decimal places
                var beginningBalance = "" + queryParameters.AccountNumber == firstTransaction.SourceAccountNumber ? firstTransaction.BalanceAfter + firstTransaction.Amount : firstTransaction.BalanceAfter - firstTransaction.Amount;
                var beginningBalanceCell = worksheet.Cell(12, 13);
                beginningBalanceCell.Value = beginningBalance;
                beginningBalanceCell.Style.NumberFormat.Format = "0.00"; // or "0.00" for two decimal places

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == queryParameters.AccountNumber);

                var currentBalanceCell = worksheet.Cell(19, 13);
                currentBalanceCell.Value = account.Balance.ToString();
                // Set the font color (e.g., Red)
                currentBalanceCell.Style.Font.FontColor = XLColor.White;


                int startRow = 17; // Assuming headers are in row 1
                foreach (var basicTransactions in allBasicTransactionsList)
                {
                    worksheet.Row(startRow).InsertRowsBelow(1);

                    worksheet.Cell(startRow, 1).Value = basicTransactions.TransactionDate.Date;
                    worksheet.Cell(startRow, 2).Value = basicTransactions.TransactionDate.TimeOfDay;
                    var descriptionCell = worksheet.Range(startRow, 3, startRow, 7).Merge();
                    descriptionCell.Value = basicTransactions.TransactionDescription;
                    descriptionCell.Style.Alignment.WrapText = true; // Enable text wrapping for long text
                    if (queryParameters.AccountNumber == basicTransactions.SourceAccountNumber)
                    {
                        totalDebit += basicTransactions.Amount;
                        worksheet.Cell(startRow, 9).Value = basicTransactions.Amount.ToString();
                    }
                    else
                    {
                        totalCredit += basicTransactions.Amount;
                        worksheet.Cell(startRow, 11).Value = basicTransactions.Amount.ToString();
                    }
                    worksheet.Cell(startRow, 13).Value = basicTransactions.BalanceAfter;
                    worksheet.Row(startRow).Height = 40; // Adjust the height as needed
                    startRow++;
                }
                worksheet.Cell(11, 13).Value = totalCredit;
                worksheet.Cell(11, 13).Style.NumberFormat.Format = "0.00"; // or "0.00" for two decimal places
                worksheet.Cell(10, 13).Value = totalDebit;
                worksheet.Cell(10, 13).Style.NumberFormat.Format = "0.00"; // or "0.00" for two decimal places
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


    public async Task<string> GenerateAndStoreOtpAsync(string email)
    {
        // Generate a 6-digit OTP
        var otp = new Random().Next(10000000, 99999999).ToString();

        // Set expiry time to 60 seconds from now
        var expiryTime = DateTime.UtcNow.AddSeconds(60 * 5);

        // Retrieve the user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            throw new Exception("User not found.");

        // Store the OTP and expiry time in the database
        user.OtpCode = otp;
        user.OtpExpiryTime = expiryTime;

        await _context.SaveChangesAsync();

        return otp;
    }


    public async Task SendOtpEmailAsync(string email, string otp)
    {
        //var subject = "Your OTP Code";
        //var message = $"Your OTP code is {otp}. It will expire in 180 seconds.";

        //// Configure and send the email (use your email service here)
        //await _emailService.SendEmailAsync(email, subject, message);
        var request = _httpContextAccessor.HttpContext.Request;

        string templateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "EmailTemplate", "Common.html");
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        //var baseUrl = "http://localhost:5018";
        // create specific iamge relative url
        var headerImageUrl = $"{baseUrl}/uploads/images/header.png";
        var footerImageUrl = $"{baseUrl}/uploads/images/footer.png";
        var placeholders = new Dictionary<string, string>
                {
                    { "{{Title}}", "" },
                    { "{{HeaderImageUrl}}", headerImageUrl },
                    { "{{FooterImageUrl}}", footerImageUrl },
                    { "{{Content}}", $"Your OTP code is <span style=\" font-size: 2em \">{otp}</span>. It will expire in 180 seconds." }
                    
                };
        // Send email
        await _emailService.SendEmailTemplateAsync(
            to: email,
            subject: "Your OTP Code",
            templateFilePath: templateFilePath,
            placeholders: placeholders
        );

    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return false;

        // Check if the OTP is correct and within the expiry time
        if (user.OtpCode == otp && user.OtpExpiryTime > DateTime.UtcNow)
        {
            // Clear the OTP after successful verification
            user.OtpCode = null;
            user.OtpExpiryTime = null;
            await _context.SaveChangesAsync();

            return true;
        }

        return false;
    }

}