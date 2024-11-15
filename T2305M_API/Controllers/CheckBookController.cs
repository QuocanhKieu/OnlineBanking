using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Claims;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Services;
using T2305M_API.Services.Implements;
using T2305M_API.Repositories.Implements;
using T2305M_API.Repositories;
using T2305M_API.DTO.Account;
using T2305M_API.DTO.CheckBook;


namespace T2305M_API.Controllers
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class CheckBookController : ControllerBase
    {
        private readonly ICheckBookService _checkBookService;
        private readonly ICheckBookRepository _checkBookRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly T2305mApiContext _context;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;


        public CheckBookController(
            ICheckBookService checkBookService,
            ICheckBookRepository checkBookRepository,
            ITransactionRepository transactionRepository,
            T2305mApiContext context,
                        IUserService userService,
                        IAccountService accountService)
        {
            _checkBookService = checkBookService;
            _checkBookRepository = checkBookRepository;
            _transactionRepository = transactionRepository;
            _context = context;
            _userService = userService;
            _accountService = accountService;
        }

        [HttpGet("list-checkbooks")]
        public async Task<IActionResult> ListCheckBooks([FromQuery] CheckBookQueryParameters queryParameters)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(new
                //    {
                //        code = 1,
                //        message = "Please input valid user information."
                //    });
                //}
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }
                int userId = int.Parse(userIdClaim);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "USER")
                {
                    queryParameters.UserId = userId;
                }

                if (!string.IsNullOrEmpty(queryParameters.AccountNumber))
                {
                    var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == queryParameters.AccountNumber);

                    if (existingAccount == null) return BadRequest(new { message = "AccountNumber not found" });
                }
               
                var paginatedResult = await _checkBookService.GetCheckBooksAsync(queryParameters);
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

        [HttpGet("check-inprogress-checkbook")]
        public async Task<IActionResult> CheckInprogressCheckbook()
        {
            try
            {

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);

                //var isHavingInProgressCheckBook = await _checkBookService.CheckHavingInProgressCheckBookAsync(queryParameters);
                var excludedStatuses = new[] { "STOPPED", "CLOSED", "CANCELED", "LOCKED", "EXHAUSTED" };
                var includedStatuses = new[] { "PENDING", "APPROVED", "SHIPPING", "DELIVERED", "WORKING" };

                var checkBook = await _context.CheckBooks
                    .FirstOrDefaultAsync(cb => cb.UserId == userId
                        && !excludedStatuses.Contains(cb.Status)
                        && includedStatuses.Contains(cb.Status));
                if (checkBook == null) {
                    return Ok(new
                    {
                        isExist = false,
                        message = "Does not exist"

                    });
                }

                return Ok(new
                {
                    isExist = true,
                    message = "Exist already"

                });

                //return Ok(new GetDetailCheckBookDTO
                //{
                //    Status = checkBook.Status,
                //    AssociatedAccountNumber = checkBook.Account.AccountNumber,
                //    CheckBookCode = checkBook.CheckBookCode,
                //    CheckBookId = checkBook.CheckBookId,
                //    ChecksRemaining = checkBook.ChecksRemaining,
                //    DeliveryAddress = checkBook.DeliveryAddress,
                //    ExpiryDate = checkBook.ExpiryDate,
                //    LastCheckClearedDate = checkBook.LastCheckClearedDate,
                //    LastClearedCheckCode = checkBook.LastClearedCheckCode,
                //    TotalChecks = checkBook.TotalChecks,
                //    TotalClearedCheckAmount = checkBook.TotalClearedCheckAmount

                //});

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }




        [HttpPut("stop-checkbook")]
        public async Task<IActionResult> StopCheckbook([FromBody] StopCheckbookRequest stopCheckbookRequest)
        {
            try
            {
                // Get the user ID from the claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (emailClaim == null)
                {
                    return Unauthorized(new { message = "Email claim is missing" });
                }

                // Ensure the checkBookCode parameter is provided
                if (string.IsNullOrEmpty(stopCheckbookRequest.CheckBookCode))
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "checkBookCode is required",
                    });
                }


                bool isOtpValid = await _accountService.VerifyOtpAsync(emailClaim, stopCheckbookRequest.Otp);
                if (!isOtpValid)
                {
                    return BadRequest("Invalid or expired OTP.");
                }

                // Find the checkbook based on the user ID and checkBookCode
                var checkbook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.UserId == userId && cb.CheckBookCode == stopCheckbookRequest.CheckBookCode);
                if (checkbook == null)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "Checkbook not found",
                    });
                }

                // Update the status and save changes
                checkbook.Status = "STOPPED";
                checkbook.StatusChangedDate = DateTime.Now;
                _context.CheckBooks.Update(checkbook);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Checkbook stopped successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }


        [HttpPost("approve-checkbook")]
        public async Task<IActionResult> ApproveCheckbook([FromBody] string checkBookCode)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {

                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim == null)
                    {
                        return Unauthorized(new { message = "Invalid token or user not authenticated" });
                    }

                    int userId = int.Parse(userIdClaim);
                    if (!string.IsNullOrEmpty(checkBookCode))
                    {
                        return BadRequest(new
                        {
                            code = 1,
                            message = "checkBookCode is required",
                        });
                    }
                    var checkbook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.UserId == userId && cb.CheckBookCode == checkBookCode);
                    if (checkbook == null)
                    {
                        return BadRequest(new
                        {
                            code = 2,
                            message = "checkBookCode not found",
                        });
                    }
                    checkbook.Status = "APPROVED";
                    checkbook.StatusChangedDate = DateTime.Now;
                    _context.CheckBooks.Update(checkbook);
                    await _context.SaveChangesAsync();

                    //var isSuccesfulCreateRelatedChecks = await _checkBookService.CreateChecksAsync(checkbook, 50);

                    await transaction.CommitAsync();
                    return Ok("Approve Check Book Successfully and Checks has ");
                }
                catch (Exception ex) // Catch other exceptions
                {
                    await transaction.RollbackAsync(); // Rollback the transaction
                    return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
                }
            }
        }





        [HttpPost("change-checkbook-to-shipping")]
        public async Task<IActionResult> ChangeCheckBookToShipping([FromBody] string checkBookCode)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                if (!string.IsNullOrEmpty(checkBookCode))
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "checkBookCode is required",
                    });
                }
                var checkbook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.UserId == userId && cb.CheckBookCode == checkBookCode);
                if (checkbook == null)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "checkBookCode not found",
                    });
                }
                checkbook.Status = "SHIPPING";
                checkbook.StatusChangedDate = DateTime.Now;
                _context.CheckBooks.Update(checkbook);
                await _context.SaveChangesAsync();

                return Ok("Stop Check Book Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }
        [HttpPost("change-checkbook-to-delivered")]
        public async Task<IActionResult> ChangeCheckBookToDelivered([FromBody] string checkBookCode)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                if (!string.IsNullOrEmpty(checkBookCode))
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "checkBookCode is required",
                    });
                }
                var checkbook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.UserId == userId && cb.CheckBookCode == checkBookCode);
                if (checkbook == null)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "checkBookCode not found",
                    });
                }
                checkbook.Status = "DELIVERED";
                checkbook.StatusChangedDate = DateTime.Now;
                _context.CheckBooks.Update(checkbook);
                await _context.SaveChangesAsync();

                return Ok("Stop Check Book Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }
        [HttpPost("change-checkbook-to-working")]
        public async Task<IActionResult> ChangeCheckBookToWorking([FromBody] string checkBookCode)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                if (!string.IsNullOrEmpty(checkBookCode))
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "checkBookCode is required",
                    });
                }
                var checkbook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.UserId == userId && cb.CheckBookCode == checkBookCode);
                if (checkbook == null)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "checkBookCode not found",
                    });
                }
                checkbook.Status = "WORKING";
                checkbook.StatusChangedDate = DateTime.Now;
                _context.CheckBooks.Update(checkbook);
                await _context.SaveChangesAsync();

                return Ok("Stop Check Book Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpPost("request-check-book")]
        public async Task<IActionResult> RequestCheckBook([FromBody] CreateCheckBookDTO createCheckBookDTO)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {

                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim == null)
                    {
                        return Unauthorized(new { message = "Invalid token or user not authenticated" });
                    }

                    int userId = int.Parse(userIdClaim);
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    var account = await _context.Accounts.FirstOrDefaultAsync(ac => ac.AccountNumber == createCheckBookDTO.AccountNumber && ac.UserId == userId);
                    if (account == null)
                    {
                        return BadRequest(new
                        {
                            code = 2,
                            message = "checkBookCode not found",
                        });
                    }
                    var newCreatedPendingCheckBook = await _checkBookService.CreateCheckBookAsync(createCheckBookDTO, userId, account.AccountId);

                    var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    foundUser.DigitalSignature = createCheckBookDTO.DigitalSignatureUrl;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok("Request opening checkbook has been created.");
                }
                catch (Exception ex) // Catch other exceptions
                {
                    await transaction.RollbackAsync(); // Rollback the transaction
                    return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
                }
            }
        }



        [HttpPost("create-checks")]
        public async Task<IActionResult> CreateChecks([FromBody] string checkBookCode, int quantity)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {

                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim == null)
                    {
                        return Unauthorized(new { message = "Invalid token or user not authenticated" });
                    }

                    int userId = int.Parse(userIdClaim);
                    if (!string.IsNullOrEmpty(checkBookCode) || quantity <= 0)
                    {
                        return BadRequest(new
                        {
                            code = 1,
                            message = "checkBookCode or quantity is invalid",
                        });
                    }
                    var checkbook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.UserId == userId && cb.CheckBookCode == checkBookCode);
                    if (checkbook == null)
                    {
                        return BadRequest(new
                        {
                            code = 2,
                            message = "checkBookCode not found",
                        });
                    }
                    //checkbook.Status = "APPROVED";
                    //checkbook.StatusChangedDate = DateTime.Now;
                    //_context.CheckBooks.Update(checkbook);
                    //await _context.SaveChangesAsync();

                    var isSuccesfulCreateRelatedChecks = await _checkBookService.CreateChecksAsync(checkbook, 50);

                    await transaction.CommitAsync();
                    return Ok("Approve Check Book Successfully and Checks has ");
                }
                catch (Exception ex) // Catch other exceptions
                {
                    await transaction.RollbackAsync(); // Rollback the transaction
                    return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
                }
            }
        }

        [HttpGet("verify-checkbook-eligibility-creation")]
        public async Task<ActionResult> VerifyCheckBookEligibilityCreation([FromQuery] string checkBookCode)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid User information."
                    });
                }
                // Validate that checkBookCode is not null or empty
                if (string.IsNullOrEmpty(checkBookCode))
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "CheckBookCode is required."
                    });
                }

                var UserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (UserIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or User not authenticated" });
                }

                int userId = int.Parse(UserIdClaim);

                // xem user có chữ ký số chưa

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);


                if (!string.IsNullOrEmpty(user.DigitalSignature))
                {
                    return BadRequest(new
                    {
                        code = 3,
                        message = "Don't have digital signature. Provide your digital signature to continue",
                    });
                }
                // xem user đã sở hữu 1 checkBook bất kỳ với 1 số trạng thái có sẵn PENDING... WORKING hay không
                var checkBook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.UserId == userId
                && cb.Status != "STOPPED"
                && cb.Status != "CLOSED"
                && cb.Status != "CANCELED"
                && cb.Status != "LOCKED"
                && cb.Status != "EXHAUSTED"
                &&
                (
                    cb.Status == "PENDING"
                    || cb.Status == "APPROVED"
                    || cb.Status == "SHIPPING"
                    || cb.Status == "DELIVERED"
                    || cb.Status == "WORKING"
                )
                );

                if (checkBook != null)
                {
                    return BadRequest(new
                    {
                        code = 4,
                        message = "Having an in-process or Working Checkbook.",
                    });
                }

                // xem account gắn checkbook có đủ thanh khoản hay không? 
                if (checkBook?.Account?.Balance != null)
                {
                    if (checkBook?.Account?.Balance < 10000)
                    {
                        return BadRequest(new
                        {
                            code = 5,
                            message = "Not meet the balance requirement of at least 10000USD.",
                        });
                    }
                }

                return Ok(
                new
                {
                    message = "Passes the 3 requirements of having digital signature, one in-process checkbook, balance of at least 10000USD "
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }



        [HttpPost("submit-check-process-request")]
        public async Task<IActionResult> SubmitCheckProcessRequest([FromBody] ProcessCheckRequirements processCheckRequirements)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim == null)
                    {
                        return Unauthorized(new { message = "Invalid token or user not authenticated" });
                    }

                    int userId = int.Parse(userIdClaim);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    var check = await _context.Checks.FirstOrDefaultAsync(c => c.CheckCode == processCheckRequirements.CheckCode && c.CheckBook.Account.AccountNumber == processCheckRequirements.OnCheckAccountNumber);
                    if (check == null)
                    {
                        return BadRequest(new
                        {
                            code = 1,
                            message = "Invalid credential. Please check again!",
                        });
                    }
                    check.Status = "PENDING";
                    check.StatusChangedDate = DateTime.Now;
                    check.Amount = processCheckRequirements.AmountOnCheck;
                    check.CheckImageUrl = processCheckRequirements.CheckFrontImageUrl;
                    check.DesAccountNumber = processCheckRequirements.DesAccountNumber;
                    _context.Checks.Update(check);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    // MAKE NOTIFICATION TO THE 2 USER + email
                    return Ok("Request to Process the Check has been submitted, The request takes from 1-3 days to process.");
                }
                catch (Exception ex) // Catch other exceptions
                {
                    await transaction.RollbackAsync(); // Rollback the transaction
                    return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
                }
            }
        }


        //admin maybe need role admin to access

        [HttpGet("verify-checkbook-and-Account-requirements-to-approve-check-payment-and-notify")]
        public async Task<ActionResult> VerifyCheckbookAndAccountRequirementsToApproveCheckPayment([FromBody] VerifyCheckbookAndAccountRequirements verifyCheckbookAndAccountRequirements)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid User information."
                    });
                }

                var UserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (UserIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or User not authenticated" });
                }

                int userId = int.Parse(UserIdClaim);

                // xem user đã sở hữu 1 checkBook bất kỳ với 1 số trạng thái có sẵn PENDING... WORKING hay không
                var checkBook = await _context.CheckBooks.FirstOrDefaultAsync(cb => cb.CheckBookCode == verifyCheckbookAndAccountRequirements.checkBookCode);

                if (checkBook.Status != "WORKING")
                {
                    return BadRequest(new
                    {
                        code = 4,
                        message = "The check book is not working.",
                    });
                }

                // xem account gắn checkbook có đủ thanh khoản hay không? 
                if (checkBook?.Account?.Balance != null)
                {
                    if (checkBook?.Account?.Balance < verifyCheckbookAndAccountRequirements.amountToSubtract)
                    {
                        return BadRequest(new
                        {
                            code = 5,
                            message = "Account Balance not enough to complete the check payment.",
                        });
                        // notify, warning the user by both notification and email
                    }
                }

                return Ok(
                new
                {
                    message = "Account and CheckBook Eligiable to complete the Check payment."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }






        [HttpPost("approve-check-request-making-transaction")]
        public async Task<IActionResult> ApproveCheckRequestMakingTransaction([FromBody] string checkCode)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim == null)
                    {
                        return Unauthorized(new { message = "Invalid token or user not authenticated" });
                    }

                    int userId = int.Parse(userIdClaim);
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    var check = await _context.Checks.FirstOrDefaultAsync(c => c.CheckCode == checkCode);
                    if (check == null)
                    {
                        return BadRequest(new
                        {
                            code = 1,
                            message = "Check not found.",
                        });
                    }
                    check.Status = "COMPLETE";
                    check.StatusChangedDate = DateTime.Now;
                    _context.Checks.Update(check);
                    await _context.SaveChangesAsync();
                    // MAKE TRANSACTION TO BOTH ACCOUNTNUMBER
                    // NOTIFY BOTH


                    await transaction.CommitAsync();
                    return Ok("The Check Succesfully Cleared.");
                }
                catch (Exception ex) // Catch other exceptions
                {
                    await transaction.RollbackAsync(); // Rollback the transaction
                    return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
                }
            }
        }

        [HttpPost("reject-check-request-making-transaction")]
        public async Task<IActionResult> RejectCheckRequestMakingTransaction([FromBody] RejectCheckRequestModel rejectCheckRequestModel)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim == null)
                    {
                        return Unauthorized(new { message = "Invalid token or user not authenticated" });
                    }

                    int userId = int.Parse(userIdClaim);
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    var check = await _context.Checks.FirstOrDefaultAsync(c => c.CheckCode == rejectCheckRequestModel.CheckCode);
                    if (check == null)
                    {
                        return BadRequest(new
                        {
                            code = 1,
                            message = "Check not found.",
                        });
                    }
                    check.Status = "REJECTED";
                    check.StatusChangedDate = DateTime.Now;
                    check.Note = rejectCheckRequestModel.Note;
                    _context.Checks.Update(check);
                    await _context.SaveChangesAsync();
                    // NOTIFY BOTH

                    await transaction.CommitAsync();
                    return Ok("The Check Succesfully Rejected.");
                }
                catch (Exception ex) // Catch other exceptions
                {
                    await transaction.RollbackAsync(); // Rollback the transaction
                    return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
                }
            }
        }
    }
}




