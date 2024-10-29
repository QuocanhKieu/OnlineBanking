﻿using Microsoft.AspNetCore.Authorization;
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
        private readonly T2305mApiContext _context;

        public AccountController(
            IAccountService accountService,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            T2305mApiContext context)
        {
            _accountService = accountService;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _context = context;
        }

        [HttpGet("list-user-accounts")]
        public async Task<IActionResult> ListUserAccounts(AccountQueryParameters accountQueryParameters)
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

                var existingAccount = await _accountService.CheckDuplicateAccountAsync(new CheckDuplicateAccountDTO { AccountNumber = createAccountDTO.AccountNumber });
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
        public async Task<IActionResult> CheckDuplicateAccount(CheckDuplicateAccountDTO checkDuplicateAccountDTO)
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
                var existingAccount = await _accountService.CheckDuplicateAccountAsync(checkDuplicateAccountDTO);
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
                var detailAccountDTO = await _accountService.GetDetailAccountDTOAsync(accountNumber.Trim());
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
        public async Task<IActionResult> CheckAccountBalance(CheckBalance checkBalance)
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
        public async Task<IActionResult> MakeMoneyTransfer([FromBody] CreateAccountDTO createAccountDTO)
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
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }

                var existingAccount = await _accountService.CheckDuplicateAccountAsync(new CheckDuplicateAccountDTO { AccountNumber = createAccountDTO.AccountNumber });
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

    }
}




