using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Claims;
using T2305M_API.DTO.SavingsAccount;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Services;
using T2305M_API.Services.Implements;
using T2305M_API.Repositories.Implements;
using T2305M_API.Repositories;
using T2305M_API.DTO.Transaction;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Transactions;
using T2305M_API.DTO.Notification;
using T2305M_API.Helper;
using T2305M_API.DTO.Account;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using DocumentFormat.OpenXml.Spreadsheet;

namespace T2305M_API.Controllers
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class SavingsAccountController : ControllerBase
    {
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly IAccountService _accountService;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserService _userService;
        private readonly INotificationRepository _notificationRepository;
        private readonly EmailService _emailService;
        private readonly T2305mApiContext _context;
        public SavingsAccountController(
            IAccountService accountService,
            IAccountRepository accountRepository,
            ISavingsAccountService savingsAccountService,
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository,
            IUserService userService,
            INotificationRepository notificationRepository,
            T2305mApiContext context,
            EmailService emailService)
        {
            _accountService = accountService;
            _accountRepository = accountRepository;
            _savingsAccountService = savingsAccountService;
            _savingsAccountRepository = savingsAccountRepository;
            _transactionRepository = transactionRepository;
            _context = context;
            _userService = userService;
            _notificationRepository = notificationRepository;
            _emailService = emailService;
        }


        [HttpGet("list-user-savings-accounts")]
        public async Task<IActionResult> ListUserSavingsAccounts([FromQuery] SavingsAccountQueryParameters savingsSavingsAccountQueryParameters)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                savingsSavingsAccountQueryParameters.Userid = userId;
                var paginatedResult = await _savingsAccountService.GetBasicSavingsAccountsAsync(savingsSavingsAccountQueryParameters);
                return Ok(new
                {
                    result = paginatedResult,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }


        [HttpGet("calculate-estimated-interest")]
        public async Task<IActionResult> CalculateInterest([FromQuery] CalculateInterestDTO calculateInterestDTO)
        {
            // Validate the DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Calculate total days
                int totalDays = calculateInterestDTO.Term * 30;

                // Calculate the maturity date
                DateTime maturityDate = calculateInterestDTO.StartDate.AddDays(totalDays);

                // Calculate interest
                decimal interest = calculateInterestDTO.DepositAmount
                                   * (_savingsAccountService.GetInterestRate(calculateInterestDTO.Term));


                // Create a response object
                var response = new
                {
                    DepositAmount = calculateInterestDTO.DepositAmount,
                    TermInDays = totalDays,
                    InterestRate = _savingsAccountService.GetInterestRate(calculateInterestDTO.Term),
                    MaturityDate = maturityDate,
                    Interest = Math.Round(interest, 2) // Rounded to 2 decimal places
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, new { Message = "An error occurred while calculating interest.", Details = ex.Message });
            }
        }


        [HttpPost("create-savings-account")]
        public async Task<IActionResult> CreateSavingsAccount([FromBody] CreateSavingAccountDTO createSavingAccountDTO)
        {
            //using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (emailClaim == null)
                {
                    return Unauthorized(new { message = "Email claim is missing" });
                }

                int userId = int.Parse(userIdClaim);
                bool isOtpValid = await _accountService.VerifyOtpAsync(emailClaim, createSavingAccountDTO.Otp);
                if (!isOtpValid)
                {
                    return BadRequest("Invalid or expired OTP.");
                }
                var sourceAccount = await _context.Accounts.FirstOrDefaultAsync(acc=> acc.AccountNumber == createSavingAccountDTO.SourceAccountNumber && acc.UserId == userId);
                if (sourceAccount == null)
                {
                    return BadRequest(new
                    {
                        code = 3,
                        message = "SourceAccountNumber does not exist or does not belong to you."
                    });
                }

                var newSavingsAccount = new SavingsAccount
                {
                    Balance = createSavingAccountDTO.DepositAmount,
                    InterestRate = _savingsAccountService.GetInterestRate(createSavingAccountDTO.Term),
                    Term = createSavingAccountDTO.Term,
                    MaturityDate = DateTime.Now.AddMonths(createSavingAccountDTO.Term),
                    StartDate = DateTime.Now,
                    Status = "ACTIVE",
                    SavingsAccountCode = $"SA{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8)}",
                    UserId = userId,
                };

                _context.SavingsAccounts.Add(newSavingsAccount);

                var createTransactionDTO = new CreateTransactionDTO
                {
                    Amount = createSavingAccountDTO.DepositAmount,
                    SourceAccountNumber = createSavingAccountDTO.SourceAccountNumber,
                    SourceAccountId = sourceAccount.AccountId,
                    TransactionType = "DEPOSITSAVING",
                    //DesAccountNumber = moneyTransfer.DesAccountNumber,
                    //DesAccountId = desAccount?.AccountId,
                    //TransactionMessage = moneyTransfer.TransactionMessage,
                    //DesAccountBalanceAfter = desAccount != null ? desAccount.Balance + moneyTransfer.MoneyAmount : null,
                    SourceAccountBalanceAfter = sourceAccount.Balance - createSavingAccountDTO.DepositAmount,
                };
                createTransactionDTO.TransactionDescription = $"Deposit Savings Received From: {createSavingAccountDTO.SourceAccountNumber} | TransCode: {createTransactionDTO.TransactionCode}";

                var newTransaction = await _transactionRepository.CreateTransactionAsync(createTransactionDTO);
                var updatedSourceAccount = await _accountService.UpdateAccountBalance(newTransaction.SourceAccountBalanceAfter.Value, newTransaction.SourceAccountNumber);
                //if (newTransaction.DesAccountBalanceAfter != null && newTransaction.DesAccountNumber != null)
                //{
                //    var updatedDesAccount = await _accountService.UpdateAccountBalance(newTransaction.DesAccountBalanceAfter.Value, newTransaction.DesAccountNumber);
                //}
                // create notifications
                var sourceAccountBalanceChangeNotificationContent = NotificationHelper.GenerateBalanceChangeNotification(new NotificationHelper.BalanceChangeDTO
                {
                    AccountNumber = newTransaction.SourceAccountNumber,
                    Currency = "USD",
                    NewBalance = newTransaction.SourceAccountBalanceAfter,
                    TransactionAmount = "-" + newTransaction.Amount,
                    TransactionType = newTransaction.TransactionType,
                    TransactionCode = newTransaction.TransactionCode,
                    TransactionDescription = $"Funds Transfered For Deposit Savings",
                    TransactionDateTime = newTransaction.TransactionDate
                });
                await _notificationRepository.CreateNotificationAsync(new CreateBasicNotificationDTO { UserId = userId, Content = sourceAccountBalanceChangeNotificationContent, Target = "USER" });


                // send mail
                string templateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "EmailTemplate", "TransactionGmailTemplate.html");
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                //var baseUrl = "http://localhost:5018";
                // create specific iamge relative url
                var headerImageUrl = $"{baseUrl}/uploads/images/header.png";
                var footerImageUrl = $"{baseUrl}/uploads/images/footer.png";
                var placeholders = new Dictionary<string, string>
                {
                    { "{{Title}}", "Thank you for using MB eBanking services.<br>MB would like to inform you that your transaction has been processed as follows:<br><br>" },
                    { "{{HeaderImageUrl}}", headerImageUrl },
                    { "{{FooterImageUrl}}", footerImageUrl },
                    { "{{TransactionDate}}", newTransaction.TransactionDate.ToString("dd-MM-yyyy HH:mm:ss") },
                    { "{{TransactionType}}", newTransaction.TransactionType },
                    { "{{TransactionCode}}", newTransaction.TransactionCode },
                    { "{{Currency}}", "USD" },
                    { "{{SrcAccountNumber}}", newTransaction.SourceAccountNumber },
                    { "{{SrcUserName}}", newTransaction.SourceAccount.User.Name },
                    { "{{DesAccountNumber}}", "SavingsAccount" },
                    { "{{DesUserName}}", "SavingsAccount" },
                    { "{{TransactionAmount}}", newTransaction.Amount.ToString("N2") }, // Formatting to 2 decimal places
                    { "{{TransactionMessage}}", newTransaction.TransactionMessage }
                };
                // Send email
                await _emailService.SendEmailTemplateAsync(
                    to: emailClaim,
                    subject: "Successful Transaction Notification",
                    templateFilePath: templateFilePath,
                    placeholders: placeholders
                );



                await _context.SaveChangesAsync();

                //await transaction.CommitAsync();
                return Ok(new 
                {
                    TransactionCode = newTransaction.TransactionCode,
                    SourceAccountNumber = newTransaction.SourceAccountNumber,
                    DesAccountOwnerName = newTransaction.DesAccount?.User?.Name ?? "DesAccOwnerName",
                    DesAccountNumber = newTransaction.DesAccountNumber,
                    Amount = newTransaction.Amount,
                    TransactionDate = newTransaction.TransactionDate,
                    TransactionMessage = newTransaction.TransactionMessage,
                    TransactionType = newTransaction.TransactionType
                });
            }

            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("get-detail-savings-account/{savingsAccountCode}")]
        public async Task<ActionResult<GetDetailSavingsAccountDTO>> GetDetailSavingsAccountDTO(string savingsAccountCode)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                var detailSavingsAccount = await _context.SavingsAccounts.FirstOrDefaultAsync(sa => sa.SavingsAccountCode == savingsAccountCode);
                if (detailSavingsAccount == null)
                {
                    return NotFound(
                        new
                        {
                            message = "DetailSavingsAccount not found."
                        });
                }

                return Ok(new GetDetailSavingsAccountDTO
                {
                    SavingsAccountCode = savingsAccountCode,
                    Balance = detailSavingsAccount.Balance * _savingsAccountService.GetInterestRate(detailSavingsAccount.Term) * (decimal)((DateTime.Now - detailSavingsAccount.StartDate).Days),
                    InterestRate = detailSavingsAccount.InterestRate,
                    MaturityDate = detailSavingsAccount.MaturityDate,
                    StartDate = detailSavingsAccount.StartDate,
                    Term = detailSavingsAccount.Term,
                    Status = detailSavingsAccount.Status,
                    WithdrawnDate = detailSavingsAccount.WithdrawnDate,
                }); // Return the DTO
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("check-account-balance")]
        public async Task<IActionResult> CheckAccountBalance([FromQuery] CheckBalance checkBalance)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }
                bool response = await _accountService.CheckAccountBalance(checkBalance);
                if (!response)
                {
                    return BadRequest(new
                    {
                        message = "Balance not enough."
                    });
                }
                return Ok(new
                {
                    message = "Balance is enough. you Can continue make money transfer"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }
        [HttpPost("withdraw-savings-account")]
        public async Task<IActionResult> WithdrawFromSavings([FromBody] WithdrawFromSavingsDTO withdrawFromSavingsDTO)
        {
            using var transaction = _context.Database.BeginTransaction();


            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (emailClaim == null)
                {
                    return Unauthorized(new { message = "Email claim is missing" });
                }

                int userId = int.Parse(userIdClaim);
                bool isOtpValid = await _accountService.VerifyOtpAsync(emailClaim, withdrawFromSavingsDTO.Otp);
                if (!isOtpValid)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "Invalid or expired OTP."
                    });
                }
                // Retrieve the savings account from the database
                var savingsAccount = _context.SavingsAccounts
                    .FirstOrDefault(sa => sa.SavingsAccountCode == withdrawFromSavingsDTO.SavingsAccountCode);

                if (savingsAccount == null)
                {
                    return NotFound(new { Message = "Savings account not found." });
                }
                var desAccount = await _accountService.CheckExistingAccountAsync(new CheckDuplicateAccountDTO { AccountNumber = withdrawFromSavingsDTO.DesAccountNumber });
                if (desAccount == null)
                {
                    return BadRequest(new
                    {
                        code = 3,
                        message = "SourceAccountNumber does not exist"
                    });
                }
                // Check if the account is active and has sufficient balance
                if (savingsAccount.Status != "ACTIVE")
                {
                    return BadRequest(new { Message = "Savings account is not active or already expired." });
                }
                decimal totalDrawAmount = 0;
                // Check if withdrawal is before the maturity date
                if (DateTime.Now < savingsAccount.MaturityDate)
                {
                    // Calculate elapsed days
                    int elapsedDays = (DateTime.Now - savingsAccount.StartDate).Days;

                    // Calculate penalty: 2% interest rate on elapsed days
                    double elapsedYears = elapsedDays / savingsAccount.Term * 30;
                    totalDrawAmount = savingsAccount.Balance +  savingsAccount.Balance * _savingsAccountService.GetInterestRate(1) * (decimal)elapsedYears;
                    // Deduct penalty and withdrawal amount
                }
                else
                {
                    // Calculate elapsed days
                    int elapsedDays = (DateTime.Now - savingsAccount.StartDate).Days;

                    // Calculate penalty: 2% interest rate on elapsed days
                    double elapsedYears = elapsedDays / savingsAccount.Term * 30;
                    totalDrawAmount = savingsAccount.Balance + savingsAccount.Balance * _savingsAccountService.GetInterestRate(savingsAccount.Term) * (decimal)(elapsedYears);
                }




                var createTransactionDTO = new CreateTransactionDTO
                {
                    Amount = totalDrawAmount,
                    TransactionType = "DEPOSITWITHDRAWN",
                    DesAccountNumber = withdrawFromSavingsDTO.DesAccountNumber,
                    DesAccountId = desAccount?.AccountId,
                    //TransactionMessage = moneyTransfer.TransactionMessage,
                    DesAccountBalanceAfter = desAccount != null ? desAccount.Balance + totalDrawAmount : null,
                    //SourceAccountBalanceAfter = sourceAccount.Balance - createSavingAccountDTO.DepositAmount,
                };
                createTransactionDTO.TransactionDescription = $"Deposit Savings WithDrawn From Savings Account: {savingsAccount.SavingsAccountCode} | TransCode: {createTransactionDTO.TransactionCode}";

                var newTransaction = await _transactionRepository.CreateTransactionAsync(createTransactionDTO);
                var updatedDesAccount = await _accountService.UpdateAccountBalance(newTransaction.DesAccountBalanceAfter.Value, newTransaction.DesAccountNumber);

                var desAccountBalanceChangeNotificationContent = NotificationHelper.GenerateBalanceChangeNotification(new NotificationHelper.BalanceChangeDTO
                {
                    AccountNumber = newTransaction.DesAccountNumber,
                    Currency = "USD",
                    NewBalance = newTransaction.DesAccountBalanceAfter.Value,
                    TransactionAmount = "+" + newTransaction.Amount,
                    TransactionType = newTransaction.TransactionType,
                    TransactionCode = newTransaction.TransactionCode,
                    TransactionDescription = $"Deposit Savings WithDrawn",
                    TransactionDateTime = newTransaction.TransactionDate
                });
                await _notificationRepository.CreateNotificationAsync(new CreateBasicNotificationDTO { UserId = userId, Content = desAccountBalanceChangeNotificationContent, Target = "USER" });


                // send mail
                string templateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "EmailTemplate", "TransactionGmailTemplate.html");
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                //var baseUrl = "http://localhost:5018";
                // create specific iamge relative url
                var headerImageUrl = $"{baseUrl}/uploads/images/header.png";
                var footerImageUrl = $"{baseUrl}/uploads/images/footer.png";
                var placeholders = new Dictionary<string, string>
                {
                    { "{{Title}}", "Thank you for using MB eBanking services.<br>MB would like to inform you that your transaction has been processed as follows:<br><br>" },
                    { "{{HeaderImageUrl}}", headerImageUrl },
                    { "{{FooterImageUrl}}", footerImageUrl },
                    { "{{TransactionDate}}", newTransaction.TransactionDate.ToString("dd-MM-yyyy HH:mm:ss") },
                    { "{{TransactionType}}", newTransaction.TransactionType },
                    { "{{TransactionCode}}", newTransaction.TransactionCode },
                    { "{{Currency}}", "USD" },
                    { "{{SrcAccountNumber}}", "SavingsAccount" },
                    { "{{SrcUserName}}", "SavingsAccount" },
                    { "{{DesAccountNumber}}", newTransaction.DesAccountNumber },
                    { "{{DesUserName}}", newTransaction.DesAccount.User.Name },
                    { "{{TransactionAmount}}", newTransaction.Amount.ToString("N2") }, // Formatting to 2 decimal places
                    { "{{TransactionMessage}}", newTransaction.TransactionMessage }
                };
                // Send email
                await _emailService.SendEmailTemplateAsync(
                    to: emailClaim,
                    subject: "Successful Transaction Notification",
                    templateFilePath: templateFilePath,
                    placeholders: placeholders
                );




                savingsAccount.Balance = 0;
                savingsAccount.Status = "WITHDRAWN";
                savingsAccount.WithdrawnDate = DateTime.Now;
                // Save changes to the database
                _context.SaveChanges();

                await transaction.CommitAsync();
                return Ok(new
                {
                    Message = "Withdrawal successful.",
                    Status = savingsAccount.Status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new { Message = "An error occurred while processing the withdrawal.", Details = ex.Message });
            }
        }


    }
}




