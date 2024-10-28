using Microsoft.CodeAnalysis.FlowAnalysis;
using T2305M_API.DTO.Culture;
using T2305M_API.DTO.Transaction;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Repositories;
using T2305M_API.Repositories.Implements;
using T2305M_API.Services;
using Microsoft.EntityFrameworkCore;
using T2305M_API.DTO.Transaction;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly IWebHostEnvironment _env;
    private readonly T2305mApiContext _context;


    public TransactionService(ITransactionRepository transactionRepository,
        IWebHostEnvironment env,
        ICreatorRepository creatorRepository,
        T2305mApiContext context)
    {
        _transactionRepository = transactionRepository;
        _env = env;
        _creatorRepository = creatorRepository;
        _context = context;

    }

    public async Task<PaginatedResult<GetBasicTransactionDTO>> GetBasicTransactionsAsync(TransactionQueryParameters queryParameters)
    {
        var (data, totalItems) = await _transactionRepository.GetTransactionsAsync(queryParameters);

        var basicTransactionDTOs = new List<GetBasicTransactionDTO>();

        foreach (var transaction in data)
        {
            basicTransactionDTOs.Add(new GetBasicTransactionDTO
            {
                TransactionId = transaction.TransactionId,
                Amount = transaction.Amount,
                BalanceAfter = transaction.BalanceAfter,
                Description = transaction.Description,
                FromAccountNumber = transaction.FromAccountNumber,
                Status = transaction.Status,
                ToAccountNumber = transaction.ToAccountNumber,
                TransactionDate = transaction.TransactionDate,
                TransactionType = transaction.TransactionType,
                FromUserName = transaction.FromAccount.User.Name,
                ToUserName = transaction.ToAccount.User.Name
            });
        }
        // Calculate total pages
        int totalPages = (int)Math.Ceiling((double)totalItems / queryParameters.PageSize);

        return new PaginatedResult<GetBasicTransactionDTO>
        {
            TotalItems = totalItems,
            PageSize = queryParameters.PageSize,
            CurrentPage = queryParameters.Page,
            TotalPages = totalPages,
            HasNextPage = queryParameters.Page < totalPages,
            HasPreviousPage = queryParameters.Page > 1,
            Data = basicTransactionDTOs
        };
    }

    public async Task<GetBasicTransactionDTO> GetDetailTransactionDTOAsync(int transactionId)
    {
        var existingTransaction = await _context.Transactions.FirstOrDefaultAsync(u => u.TransactionId == transactionId);

        if (existingTransaction == null)
        {
            return null; // Or throw an appropriate exception
        }

        var detailTransactionDTO = new GetBasicTransactionDTO
        {
            TransactionId = existingTransaction.TransactionId,
            Amount = existingTransaction.Amount,
            BalanceAfter = existingTransaction.BalanceAfter,
            Description = existingTransaction.Description,
            FromAccountNumber = existingTransaction.FromAccountNumber,
            Status = existingTransaction.Status,
            ToAccountNumber = existingTransaction.ToAccountNumber,
            TransactionDate = existingTransaction.TransactionDate,
            TransactionType = existingTransaction.TransactionType,
            FromUserName = existingTransaction.FromAccount.User.Name,
            ToUserName = existingTransaction.ToAccount.User.Name
        };

        return detailTransactionDTO;
    }


}