using Microsoft.CodeAnalysis.FlowAnalysis;
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
    private readonly IWebHostEnvironment _env;
    private readonly T2305mApiContext _context;


    public TransactionService(ITransactionRepository transactionRepository,
        IWebHostEnvironment env,
        T2305mApiContext context)
    {
        _transactionRepository = transactionRepository;
        _env = env;
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
                Amount = transaction.Amount,
                BalanceAfter = queryParameters.AccountNumber==transaction.SourceAccountNumber?transaction.SourceAccountBalanceAfter:transaction.DesAccountBalanceAfter,
                TransactionDescription = transaction.TransactionDescription,
                SourceAccountNumber = transaction.SourceAccountNumber,
                DesAccountNumber = transaction.DesAccountNumber,
                TransactionDate = transaction.TransactionDate,
                TransactionType = transaction.TransactionType,
                SourceUserName = transaction.SourceAccount.User.Name,
                DesUserName = transaction.DesAccount.User.Name
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

    //public async Task<GetBasicTransactionDTO> GetDetailTransactionDTOAsync(int transactionId)
    //{
    //    var transaction = await _context.Transactions.FirstOrDefaultAsync(u => u.TransactionId == transactionId);

    //    if (transaction == null)
    //    {
    //        return null; // Or throw an appropriate exception
    //    }

    //    var detailTransactionDTO = new GetBasicTransactionDTO
    //    {
    //        Amount = transaction.Amount,
    //        BalanceAfter = transaction..AccountNumber == transaction.SourceAccountNumber ? transaction.SourceAccountBalanceAfter : transaction.DesAccountBalanceAfter,
    //        TransactionDescription = transaction.TransactionDescription,
    //        SourceAccountNumber = transaction.SourceAccountNumber,
    //        DesAccountNumber = transaction.DesAccountNumber,
    //        TransactionDate = transaction.TransactionDate,
    //        TransactionType = transaction.TransactionType,
    //        SourceUserName = transaction.SourceAccount.User.Name,
    //        DesUserName = transaction.DesAccount.User.Name
    //    };

    //    return detailTransactionDTO;
    //}


}