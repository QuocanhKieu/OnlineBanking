using Microsoft.CodeAnalysis.FlowAnalysis;
using T2305M_API.DTO.SavingsAccount;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Repositories;
using T2305M_API.Repositories.Implements;
using T2305M_API.Services;
using Microsoft.EntityFrameworkCore;
using T2305M_API.DTO.SavingsAccount;
using ClosedXML.Excel;
using Azure;
using T2305M_API.DTO.Transaction;
using T2305M_API.Services.Implements;
using Azure.Core;

public class SavingsAccountService : ISavingsAccountService
{
    private readonly ISavingsAccountRepository _savingsAccountRepository;
    private readonly IWebHostEnvironment _env;
    private readonly T2305mApiContext _context;
    private readonly ITransactionService _transactionService;
    private readonly EmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly List<InterestRateRange> InterestRateTable = new List<InterestRateRange>
{
    new InterestRateRange { StartMonth = 1, EndMonth = 1, InterestRate = 0.002M }, // 1 month
    new InterestRateRange { StartMonth = 2, EndMonth = 3, InterestRate = 0.017M }, // 2 to 3 months
    new InterestRateRange { StartMonth = 4, EndMonth = 6, InterestRate = 0.02M },  // 4 to 6 months
    new InterestRateRange { StartMonth = 7, EndMonth = 12, InterestRate = 0.03M }, // 7 to 12 months
    new InterestRateRange { StartMonth = 13, EndMonth = 18, InterestRate = 0.04M }, // 14 to 18 months
    new InterestRateRange { StartMonth = 19, EndMonth = int.MaxValue, InterestRate = 0.05M } // Above 36 months
};
    public SavingsAccountService(ISavingsAccountRepository savingsAccountRepository,
        IWebHostEnvironment env,
            ITransactionService transactionService,
        T2305mApiContext context,
        EmailService emailService
        , IHttpContextAccessor httpContextAccessor

        )
    {
        _savingsAccountRepository = savingsAccountRepository;
        _env = env;
        _context = context;
        _transactionService = transactionService;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public decimal GetInterestRate(int termInMonths)
    {
        var rateEntry = InterestRateTable
            .FirstOrDefault(entry => termInMonths >= entry.StartMonth && termInMonths <= entry.EndMonth);

        return rateEntry?.InterestRate ?? 0M; // Return 0 if no match is found
    }
    public async Task<PaginatedResult<GetDetailSavingsAccountDTO>> GetBasicSavingsAccountsAsync(SavingsAccountQueryParameters queryParameters)
    {
        var (data, totalItems) = await _savingsAccountRepository.GetSavingsAccountsAsync(queryParameters);

        var basicSavingsAccountDTOs = new List<GetDetailSavingsAccountDTO>();

        foreach (var savingsAccount in data)
        {
            basicSavingsAccountDTOs.Add(new GetDetailSavingsAccountDTO
            {
                Balance = savingsAccount.Balance + savingsAccount.Balance * GetInterestRate(savingsAccount.Term) * (decimal)((DateTime.Now - savingsAccount.StartDate).Days),
                InterestRate = savingsAccount.InterestRate,
                SavingsAccountCode = savingsAccount.SavingsAccountCode,
                MaturityDate = savingsAccount.MaturityDate,
                SavingsAccountId = savingsAccount.SavingsAccountId,
                StartDate = savingsAccount.StartDate,
                Status = savingsAccount.Status,
                Term = savingsAccount.Term,
                WithdrawnDate = savingsAccount.WithdrawnDate,

            });
        }
        // Calculate total pages
        int totalPages = (int)Math.Ceiling((double)totalItems / queryParameters.PageSize);

        return new PaginatedResult<GetDetailSavingsAccountDTO>
        {
            TotalItems = totalItems,
            PageSize = queryParameters.PageSize,
            CurrentPage = queryParameters.Page,
            TotalPages = totalPages,
            HasNextPage = queryParameters.Page < totalPages,
            HasPreviousPage = queryParameters.Page > 1,
            Data = basicSavingsAccountDTOs
        };
    }

}