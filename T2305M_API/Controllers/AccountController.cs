using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Claims;
using T2305M_API.DTO.Account;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Services;
using T2305M_API.Services.Implements;
using T2305M_API.Repositories.Implements;
using T2305M_API.Repositories;
using T2305M_API.DTO.Account;
using T2305M_API.DTO.Account;
using T2305M_API.DTO.Transaction;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Transactions;
using T2305M_API.DTO.Notification;
using T2305M_API.Helper;

namespace T2305M_API.Controllers
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserService _userService;
        private readonly INotificationRepository _notificationRepository;

        private readonly T2305mApiContext _context;

        public AccountController(
            IAccountService accountService,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUserService userService,
            INotificationRepository notificationRepository,
            T2305mApiContext context)
        {
            _accountService = accountService;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _context = context;
            _userService = userService;
            _notificationRepository = notificationRepository;
        }

        [HttpGet("list-user-accounts")]
        public async Task<IActionResult> ListUserAccounts([FromQuery] AccountQueryParameters accountQueryParameters)
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
                accountQueryParameters.Userid = userId;
                var paginatedResult = await _accountService.GetBasicAccountsAsync(accountQueryParameters);
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

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDTO createAccountDTO)
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

                var existingAccount = await _accountService.CheckExistingAccountAsync(new CheckDuplicateAccountDTO { AccountNumber = createAccountDTO.AccountNumber });
                if (existingAccount != null)
                {
                    return BadRequest(new
                    {
                        message = "AccountNumber is already registered."
                    });
                }

                await _accountRepository.CreateAccountAsync(createAccountDTO, userId);
                return Ok(new
                {
                    message = "AccountNumber Created Successfully."
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("check-existing-account")]
        public async Task<IActionResult> CheckExistingAccount([FromQuery] CheckDuplicateAccountDTO checkDuplicateAccountDTO)
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
                var existingAccount = await _accountService.CheckExistingAccountAsync(checkDuplicateAccountDTO);
                if (existingAccount != null)
                {
                    return BadRequest(new
                    {
                        message = "AccountNumber is already registered."
                    });
                }
                return Ok(new
                {
                    message = " This AccountNumber does not exist."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("get-detail-account/{accountNumber}")]
        public async Task<ActionResult<GetDetailAccountDTO>> GetDetailAccountDTO(string accountNumber)
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
                var detailAccountDTO = await _accountService.GetDetailAccountDTOAsync(accountNumber.Trim(), userId);
                if (detailAccountDTO == null)
                {
                    return NotFound(
                        new
                        {
                            message = "DetailAccount not found."
                        });
                }
                //return Ok(new APIResponse<GetDetailAccountDTO>(detailAccountDTO, "Retrieved paginated basic Books successfully."));
                return Ok(detailAccountDTO); // Return the DTO
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

        [HttpPost("make-money-transfer")]
        public async Task<IActionResult> MakeMoneyTransfer([FromBody] MoneyTransfer moneyTransfer)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                //check user transpassword existence
                var isNotHavingTransPW = await _userService.CheckTransPasswordExistAsync(userId);
                if (isNotHavingTransPW)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please Create TransPassword first"
                    });
                }
                //check input
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "Please input valid user information."
                    });
                }
                //check user transpassword match
                var isMatch = await _userService.VerifyTranspasswordAsync(userId, moneyTransfer.TransPassword);
                if (!isMatch)
                {
                    return BadRequest(
                        new
                        {
                            code = 6,
                            message = "Transaction password not match, Please Check Again!"
                        });
                }

                //Check  account existence
                var sourceAccount = await _accountService.CheckExistingAccountAsync(new CheckDuplicateAccountDTO { AccountNumber = moneyTransfer.SourceAccountNumber });
                if (sourceAccount == null)
                {
                    return BadRequest(new
                    {
                        code = 3,
                        message = "SourceAccountNumber does not exist"
                    });
                }
                Account desAccount = null;
                if (moneyTransfer.DesAccountNumber != null)
                {
                     desAccount = await _accountService.CheckExistingAccountAsync(new CheckDuplicateAccountDTO { AccountNumber = moneyTransfer.DesAccountNumber });
                    if (desAccount == null)
                    {
                        return BadRequest(new
                        {
                            code = 4,
                            message = "DesAccountNumber does not exist"
                        });
                    }
                    if (desAccount.UserId == userId)
                    {
                        return BadRequest(new
                        {
                            code = 9,
                            message = "Can not make transaction to your own account."
                        });
                    }
                }
                bool response = await _accountService.CheckAccountBalance(new CheckBalance { AccountNumber = moneyTransfer.SourceAccountNumber, MoneyAmount = moneyTransfer.MoneyAmount});
                if (!response)
                {
                    return BadRequest(new
                    {
                        code = 5,
                        message = "Balance not enough for the SourceAccount."
                    });
                }

                decimal todaySourceAccountTransferAmmount = await _transactionRepository.CalculateTotalTransferedAmountPerDay(moneyTransfer.SourceAccountNumber);
                if(todaySourceAccountTransferAmmount + moneyTransfer.MoneyAmount > 15000)
                {
                    return BadRequest(
                        new
                        {
                            code = 7,
                            message = "Your Daily Transfer Ammout cuould not get over 15000, Please try again."
                        });
                }
                if (moneyTransfer.MoneyAmount > 5000)
                {
                    return BadRequest(
                        new
                        {
                            code = 8,
                            message = "The Amount per Transaction could not get over 5000, Please try again."
                        });
                }
                var createTransactionDTO = new CreateTransactionDTO
                {
                    Amount = moneyTransfer.MoneyAmount,
                    SourceAccountNumber = moneyTransfer.SourceAccountNumber,
                    SourceAccountId = sourceAccount.AccountId,
                    TransactionType = "BANKTRANSFER",
                    DesAccountNumber = moneyTransfer.DesAccountNumber,
                    DesAccountId = desAccount?.AccountId,
                    TransactionMessage = moneyTransfer.TransactionMessage,
                    DesAccountBalanceAfter = desAccount != null ? desAccount.Balance + moneyTransfer.MoneyAmount : null,
                    SourceAccountBalanceAfter = sourceAccount.Balance - moneyTransfer.MoneyAmount,
                };
                createTransactionDTO.TransactionDescription = $"Online Banking Transfer: Funds moved from Account Number: {moneyTransfer.SourceAccountNumber} to Account Number: {moneyTransfer.DesAccountNumber} | TransCode: {createTransactionDTO.TransactionCode}";
                
                var newTransaction = await _transactionRepository.CreateTransactionAsync(createTransactionDTO);
                var updatedSourceAccount = await _accountService.UpdateAccountBalance(newTransaction.SourceAccountBalanceAfter, newTransaction.SourceAccountNumber);
                if (newTransaction.DesAccountBalanceAfter != null && newTransaction.DesAccountNumber != null)
                {
                    var updatedDesAccount = await _accountService.UpdateAccountBalance(newTransaction.DesAccountBalanceAfter.Value, newTransaction.DesAccountNumber);
                }
                // create notifications
                var sourceAccountBalanceChangeNotificationContent = NotificationHelper.GenerateBalanceChangeNotification(new NotificationHelper.BalanceChangeDTO
                {
                    AccountNumber = newTransaction.SourceAccountNumber,
                    Currency = "USD",
                    NewBalance = newTransaction.SourceAccountBalanceAfter,
                    TransactionAmount = "-" + newTransaction.Amount,
                    TransactionType = newTransaction.TransactionType,
                    TransactionCode = newTransaction.TransactionCode,
                    TransactionDescription = $"Funds Transfered To {newTransaction.DesAccountNumber}",
                    TransactionDateTime = newTransaction.TransactionDate
                });
                await _notificationRepository.CreateNotificationAsync(new CreateBasicNotificationDTO { UserId = userId, Content = sourceAccountBalanceChangeNotificationContent });

                var desAccountBalanceChangeNotificationContent = NotificationHelper.GenerateBalanceChangeNotification(new NotificationHelper.BalanceChangeDTO
                {
                    AccountNumber = newTransaction.DesAccountNumber,
                    Currency = "USD",
                    NewBalance = newTransaction.DesAccountBalanceAfter.Value,
                    TransactionAmount = "+" + newTransaction.Amount,
                    TransactionType = newTransaction.TransactionType,
                    TransactionCode = newTransaction.TransactionCode,
                    TransactionDescription = $"Funds Transfered From {newTransaction.SourceAccountNumber}",
                    TransactionDateTime = newTransaction.TransactionDate
                });
                await _notificationRepository.CreateNotificationAsync(new CreateBasicNotificationDTO { UserId = newTransaction.DesAccount.UserId, Content = desAccountBalanceChangeNotificationContent });


                return Ok(new AfterSuccessTransactionDTO
                {
                    TransactionCode = newTransaction.TransactionCode,
                    SourceAccountNumber = newTransaction.SourceAccountNumber,
                    DesAccountOwnerName = newTransaction.DesAccount?.User?.Name??"DesAccOwnerName",
                    DesAccountNumber = newTransaction.DesAccountNumber,
                    Amount = newTransaction.Amount,
                    TransactionDate = newTransaction.TransactionDate,
                    TransactionMessage = newTransaction.TransactionMessage,
                    TransactionType = newTransaction.TransactionType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("find-like-account")]
        public async Task<IActionResult> FindExistingAccount([FromQuery] int accountNumber)
        {
            try
            {

                IEnumerable<Account> foundAccounts = await _accountRepository.ListLikeAccountsAsync(accountNumber.ToString().Trim());

                // Map found accounts to a custom object list
                var mappedAccounts = foundAccounts.Select(account => new
                {
                    AccountNumber = account.AccountNumber,
                    AccountName = account.User?.Name ?? "UserName",
                    // Add other properties as needed
                }).ToList();

                return Ok(new
                {
                    foundAccounts = mappedAccounts,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }
    }
}




