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
using T2305M_API.DTO.Transaction;

namespace T2305M_API.Controllers
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly T2305mApiContext _context;

        public TransactionController(
            ITransactionService transactionService,
            ITransactionRepository transactionRepository,
            T2305mApiContext context)
        {
            _transactionService = transactionService;
            _transactionRepository = transactionRepository;
            _context = context;

        }

        [HttpGet("List-Account-Transactions")]
        public async Task<IActionResult> ListUserTransactions([FromQuery] TransactionQueryParameters transactionQueryParameters)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                //transactionQueryParameters.Userid = userId;
                var paginatedResult = await _transactionService.GetBasicTransactionsAsync(transactionQueryParameters);
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
        [HttpGet("find-one-Transactions")]
        public async Task<IActionResult> FindONeTrans([FromQuery] string transactionCode)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);

                var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionCode == transactionCode);
                if (transaction == null)
                {
                    return NotFound("Transaction not Found");
                }
                return Ok(new GetBasicTransactionDTO
                {
                    Amount = transaction.Amount,
                    BalanceAfter = transaction.SourceAccount.UserId == userId? transaction.SourceAccountBalanceAfter : transaction.DesAccountBalanceAfter,// Caution!
                    TransactionDescription = transaction.TransactionDescription,
                    SourceAccountNumber = transaction.SourceAccountNumber,
                    DesAccountNumber = transaction.DesAccountNumber,
                    TransactionDate = transaction.TransactionDate,
                    TransactionType = transaction.TransactionType,
                    SourceUserName = transaction.SourceAccount.User.Name,
                    DesUserName = transaction.DesAccount.User.Name,
                    DesAccountId = transaction.DesAccountId,
                    SourceAccountId = transaction.SourceAccountId.Value,
                    TransactionMesage = transaction.TransactionMessage,
                    TransactionCode = transaction.TransactionCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }
    }
}




