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

        //[HttpPost("Create-Transaction")]
        //public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDTO createTransactionDTO)
        //{
        //    try
        //    {
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new { message = "Invalid token or user not authenticated" });
        //        }

        //        int userId = int.Parse(userIdClaim);

        //        var existingTransaction = await _transactionService.CheckDuplicateTransactionAsync(new CheckDuplicateTransactionDTO { TransactionNumber = createTransactionDTO.TransactionNumber });
        //        if (existingTransaction != null)
        //        {
        //            return BadRequest(new
        //            {
        //                message = "TransactionNumber is already registered."
        //            });
        //        }

        //        await _transactionRepository.CreateTransactionAsync(createTransactionDTO, userId);
        //        return Ok(new
        //        {
        //            message = "TransactionNumber Created Successfully."
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
        //    }
        //}


        //[HttpGet("{transactionId}")]
        //public async Task<ActionResult<GetBasicTransactionDTO>> GetDetailTransactionDTO(int transactionId)
        //{
        //    try
        //    {
        //        var detailTransactionDTO = await _transactionService.GetDetailTransactionDTOAsync(transactionId);
        //        if (detailTransactionDTO == null)
        //        {
        //            return NotFound(
        //                new
        //                {
        //                    message = "DetailTransaction not found."
        //                });
        //        }
        //        //return Ok(new APIResponse<GetDetailTransactionDTO>(detailTransactionDTO, "Retrieved paginated basic Books successfully."));
        //        return Ok(detailTransactionDTO); // Return the DTO
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
        //    }
        //}
        //[HttpGet("List-Account-Transactions")]
        //public async Task<IActionResult> CalculateTotalTransferedAmountPerDay(TransactionQueryParameters transactionQueryParameters)
        //{
        //    try
        //    {
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new { message = "Invalid token or user not authenticated" });
        //        }

        //        int userId = int.Parse(userIdClaim);
        //        //transactionQueryParameters.Userid = userId;
        //        var paginatedResult = await _transactionService.GetBasicTransactionsAsync(transactionQueryParameters);
        //        return Ok(new
        //        {
        //            result = paginatedResult,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
        //    }
        //}
    }
}




