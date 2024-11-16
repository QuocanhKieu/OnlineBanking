using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
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

    //public async Task<(IEnumerable<Transaction> Data, int TotalItems)> GetTransactionsAsync(TransactionQueryParameters queryParameters)
    //{
    //    try
    //    {

    //        IQueryable<Transaction> query = _context.Transactions;

    //        if (!string.IsNullOrEmpty(queryParameters.AccountNumber))
    //        {
    //            switch (queryParameters.MoneyFlow?.Trim().ToUpper())
    //            {
    //                case "IN":
    //                    query = query.Where(a => a.DesAccountNumber == queryParameters.AccountNumber.Trim());
    //                    break;
    //                case "OUT":
    //                    query = query.Where(a => a.SourceAccountNumber == queryParameters.AccountNumber.Trim());
    //                    break;
    //                default:
    //                    query = query.Where(a => a.SourceAccountNumber == queryParameters.AccountNumber.Trim() || a.DesAccountNumber == queryParameters.AccountNumber.Trim());
    //                    break;
    //            }
    //        }

    //        var transactionType = queryParameters.TransactionType?.ToUpper().Trim() ?? "";

    //        if (!string.IsNullOrEmpty(transactionType))
    //        {
    //            query = query.Where(a => a.TransactionType == transactionType);
    //        }

    //        if (!string.IsNullOrEmpty(queryParameters.SortColumn))
    //        {
    //            bool isDescending = queryParameters.SortOrder?.ToLower() == "desc";
    //            switch (queryParameters.SortColumn.ToLower())
    //            {
    //                default:
    //                    query = isDescending ? query.OrderByDescending(a => a.TransactionDate) : query.OrderBy(a => a.TransactionDate);
    //                    break;
    //            }
    //        }

    //        // Pagination
    //        int totalItems = await query.CountAsync();
    //        var pagedData = await query
    //            .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
    //            .Take(queryParameters.PageSize)
    //            .ToListAsync();

    //        return (pagedData, totalItems);
    //    }
    //    catch (Exception ex) 
    //    {
    //        throw ex;
    //    }
    //}
    //public async Task<Transaction> CreateTransactionAsync(CreateTransactionDTO createTransactionDTO)
    //{
    //    using (var transaction = await _context.Database.BeginTransactionAsync())
    //    {
    //        try
    //        {
    //            // Map the CreateTransactionDTO properties to the new Transaction entity
    //            var newTransaction = new Transaction
    //            {
    //                Amount = createTransactionDTO.Amount,
    //                TransactionDescription = createTransactionDTO?.TransactionDescription,
    //                DesAccountNumber = createTransactionDTO.DesAccountNumber,
    //                SourceAccountNumber = createTransactionDTO.SourceAccountNumber,
    //                TransactionType = createTransactionDTO.TransactionType,
    //                SourceAccountId = createTransactionDTO.SourceAccountId,
    //                DesAccountId = createTransactionDTO.DesAccountId,
    //                DesAccountBalanceAfter = createTransactionDTO.DesAccountBalanceAfter,
    //                SourceAccountBalanceAfter = createTransactionDTO.SourceAccountBalanceAfter,

    //            };

    //            // Add the new Transaction to the DbSet
    //            _context.Transactions.Add(newTransaction);
    //            await _context.SaveChangesAsync(); // Save the transaction first to generate TransactionId
    //            // Commit the transaction if all is successful
    //            await transaction.CommitAsync();
    //            return newTransaction;
    //        }
    //        catch (Exception ex) // Catch other exceptions
    //        {
    //            await transaction.RollbackAsync(); // Rollback the transaction
    //            throw ex; // Rethrow the exception after rollback
    //        }
    //    }
    //}



    public async Task<(IEnumerable<Transaction> Data, int TotalItems)> GetTransactionsAsync(TransactionQueryParameters queryParameters)
    {
        try
        {
            if (!string.IsNullOrEmpty(queryParameters.Period))
            {
                var dateRange = CalculateDateRange(queryParameters.Period);
                queryParameters.StartDate = dateRange.StartDate;
                queryParameters.EndDate = dateRange.EndDate;
            }
            // Define SQL parameters for stored procedure
            var accountNumberParam = new SqlParameter("@AccountNumber", (object)queryParameters.AccountNumber ?? DBNull.Value);
            var moneyFlowParam = new SqlParameter("@MoneyFlow", (object)queryParameters.MoneyFlow ?? DBNull.Value);
            var transactionTypeParam = new SqlParameter("@TransactionType", (object)queryParameters.TransactionType ?? DBNull.Value);
            var sortColumnParam = new SqlParameter("@SortColumn", ValidateSortColumn(queryParameters.SortColumn));
            var sortOrderParam = new SqlParameter("@SortOrder", queryParameters.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC");
            var startDateParam = new SqlParameter("@StartDate", (object)queryParameters.StartDate ?? DBNull.Value);
            var endDateParam = new SqlParameter("@EndDate", (object)queryParameters.EndDate ?? DBNull.Value);

            // Execute stored procedure to get transactions
            var transactions = await _context.Transactions
                .FromSqlRaw("EXEC dbo.GetTransactions @AccountNumber, @MoneyFlow, @TransactionType, @SortColumn, @SortOrder, @StartDate, @EndDate",
                             accountNumberParam, moneyFlowParam, transactionTypeParam, sortColumnParam, sortOrderParam, startDateParam, endDateParam)
                .ToListAsync();

            // Get total items count
            int totalItems = transactions.Count;

            // Apply pagination
            var pagedData = transactions
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToList();

            return (pagedData, totalItems);
        }
        catch (Exception ex)
        {
            // Consider logging the exception
            throw;
        }
    }
    private (DateTime? StartDate, DateTime? EndDate) CalculateDateRange(string period)
    {
        DateTime endDate = DateTime.Now;
        DateTime startDate;

        switch (period.ToUpper().Trim())
        {
            case "7DAY":
                startDate = endDate.AddDays(-7).Date;
                break;
            case "1MONTH":
                startDate = endDate.AddMonths(-1).Date;
                break;
            case "3MONTH":
                startDate = endDate.AddMonths(-3).Date;
                break;
            case "6MONTH":
                startDate = endDate.AddMonths(-6).Date;
                break;
            case "9MONTH":
                startDate = endDate.AddMonths(-9).Date;
                break;
            case "12MONTH":
                startDate = endDate.AddMonths(-12).Date;
                break;
            default:
                startDate = endDate.AddDays(-7).Date;
                break;
        }

        return (startDate, endDate);
    }
    private string ValidateSortColumn(string? sortColumn)
    {
        // Validate allowed columns to prevent SQL injection
        var allowedColumns = new[] { "TransactionDate", "Amount", "TransactionType", "AccountNumber" };
        return allowedColumns.Contains(sortColumn) ? sortColumn : "TransactionDate";
    }





    public async Task<Transaction> CreateTransactionAsync(CreateTransactionDTO createTransactionDTO)
    {
        // Start a new transaction

        {
            try
            {
                // Define parameters for the stored procedure
                var transactionTypeParam = new SqlParameter("@TransactionType", createTransactionDTO.TransactionType ?? (object)DBNull.Value);
                var sourceAccountNumberParam = new SqlParameter("@SourceAccountNumber", createTransactionDTO.SourceAccountNumber ?? (object)DBNull.Value);
                var desAccountNumberParam = new SqlParameter("@DesAccountNumber", createTransactionDTO.DesAccountNumber ?? (object)DBNull.Value);
                var sourceAccountIdParam = new SqlParameter("@SourceAccountId", createTransactionDTO.SourceAccountId ?? (object)DBNull.Value);
                var desAccountIdParam = new SqlParameter("@DesAccountId", createTransactionDTO.DesAccountId ?? (object)DBNull.Value);
                var amountParam = new SqlParameter("@Amount", createTransactionDTO.Amount );
                var sourceBalanceAfterParam = new SqlParameter("@SourceAccountBalanceAfter", createTransactionDTO.SourceAccountBalanceAfter ?? (object)DBNull.Value);
                var desBalanceAfterParam = new SqlParameter("@DesAccountBalanceAfter", createTransactionDTO.DesAccountBalanceAfter ?? (object)DBNull.Value);
                var descriptionParam = new SqlParameter("@TransactionDescription", createTransactionDTO.TransactionDescription ?? (object)DBNull.Value);
                var messageParam = new SqlParameter("@TransactionMessage", createTransactionDTO.TransactionMessage ?? (object)DBNull.Value);
                var transactionCodeParam = new SqlParameter("@TransactionCode", createTransactionDTO.TransactionCode); // Corrected this line

                var transactionDateOutputParam = new SqlParameter
                {
                    ParameterName = "@TransactionDate",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Output
                };

                // Execute the stored procedure
                var result = await _context.Transactions
                    .FromSqlRaw("EXEC dbo.InsertTransaction @TransactionCode, @TransactionType, @SourceAccountNumber, @DesAccountNumber, @SourceAccountId, @DesAccountId, @Amount, @SourceAccountBalanceAfter, @DesAccountBalanceAfter, @TransactionDescription, @TransactionMessage, @TransactionDate OUT",
                                transactionCodeParam, transactionTypeParam, sourceAccountNumberParam, desAccountNumberParam, sourceAccountIdParam, desAccountIdParam,
                                amountParam, sourceBalanceAfterParam, desBalanceAfterParam, descriptionParam, messageParam, transactionDateOutputParam)
                    .ToListAsync();

                // Commit the transaction if the stored procedure executed successfully

                // Return the first result, as we expect only one inserted record
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine($"Error occurred: {ex.Message}");

                // Rollback will happen automatically when the 'using' block exits without committing
                throw;
            }
        }
    }



    public async Task<Decimal> CalculateTotalTransferedAmountPerDay(string accountNumber)
    {

        // Assuming you have a DbContext named 'context' and a TransferLimit property in your Account entity
        var account = await _context.Accounts.FirstOrDefaultAsync(a=>a.AccountNumber == accountNumber);
        if (account == null)
        {
            throw new Exception("Account not found");
        }

        var today = DateTime.Today;
        var totalTransferredToday = await _context.Transactions
            .Where(t => t.SourceAccountNumber == accountNumber && t.TransactionDate >= today)
            .SumAsync(t => t.Amount);
       
        return totalTransferredToday;
    }

}