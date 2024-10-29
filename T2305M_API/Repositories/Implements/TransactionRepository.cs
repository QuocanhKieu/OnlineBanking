using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using T2305M_API.DTO.Transaction;
using T2305M_API.Entities;
using T2305M_API.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly T2305mApiContext _context;
    private readonly IWebHostEnvironment _env;

    public TransactionRepository(T2305mApiContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<(IEnumerable<Transaction> Data, int TotalItems)> GetTransactionsAsync(TransactionQueryParameters queryParameters)
    {
        IQueryable<Transaction> query = _context.Transactions;

        // Filter by IsRecommended
        if (!string.IsNullOrEmpty(queryParameters.AccountNumber))
        {
            query = query.Where(a => a.SourceAccountNumber == queryParameters.AccountNumber.Trim()|| a.DesAccountNumber == queryParameters.AccountNumber.Trim());
        }
        var searchTerm = queryParameters.SearchTerm?.ToLower().Trim() ?? "";
        // Filter by search term (title, author, etc.)
        //if (!string.IsNullOrEmpty(searchTerm))
        //{
        //    query = query.Where(a =>
        //        (a.Description ?? "").ToLower().Contains(searchTerm)||
        //        (a.User.Name.ToLower().Contains(searchTerm))
        //    );
        //}
        var transactionType = queryParameters.SearchTerm?.ToUpper().Trim() ?? "";

        if (!string.IsNullOrEmpty(transactionType))
        {
            if(transactionType == "INFLOW" || transactionType == "OUTFLOW")
            {
                query = query.Where(a => a.TransactionType == transactionType
                    
                );
            }
        }

        if (!string.IsNullOrEmpty(queryParameters.SortColumn))
        {
            bool isDescending = queryParameters.SortOrder?.ToLower() == "desc";
            switch (queryParameters.SortColumn.ToLower())
            {
                default:
                    query = isDescending ? query.OrderByDescending(a => a.TransactionDate) : query.OrderBy(a => a.TransactionDate);
                    break;
            }
        }

        // Pagination
        int totalItems = await query.CountAsync();
        var pagedData = await query
            .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .ToListAsync();

        return (pagedData, totalItems);
    }
    public async Task<Transaction> CreateTransactionAsync(CreateTransactionDTO createTransactionDTO)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Map the CreateTransactionDTO properties to the new Transaction entity
                var newTransaction = new Transaction
                {
                   Amount = createTransactionDTO.Amount,
                   TransactionDescription = createTransactionDTO?.TransactionDescription,
                    DesAccountNumber = createTransactionDTO.DesAccountNumber,
                   SourceAccountNumber = createTransactionDTO.SourceAccountNumber,
                   TransactionType = createTransactionDTO.TransactionType,
                   SourceAccountId = createTransactionDTO.SourceAccountId,
                   DesAccountId = createTransactionDTO.DesAccountId,
                   DesAccountBalanceAfter = createTransactionDTO.DesAccountBalanceAfter,
                   SourceAccountBalanceAfter = createTransactionDTO.SourceAccountBalanceAfter,

                };

                // Add the new Transaction to the DbSet
                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync(); // Save the transaction first to generate TransactionId
                // Commit the transaction if all is successful
                await transaction.CommitAsync();
                return newTransaction;
            }
            catch (Exception ex) // Catch other exceptions
            {
                await transaction.RollbackAsync(); // Rollback the transaction
                throw ex; // Rethrow the exception after rollback
            }
        }
    }

}