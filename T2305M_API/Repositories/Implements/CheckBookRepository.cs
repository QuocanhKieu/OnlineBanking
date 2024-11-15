using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using T2305M_API.DTO.CheckBook;
using T2305M_API.Entities;
using T2305M_API.Repositories;

public class CheckBookRepository : ICheckBookRepository
{
    private readonly T2305mApiContext _context;
    private readonly IWebHostEnvironment _env;

    public CheckBookRepository(T2305mApiContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<(IEnumerable<CheckBook> Data, int TotalItems)> GetCheckBooksAsync(CheckBookQueryParameters queryParameters)
    {
        try
        {

            IQueryable<CheckBook> query = _context.CheckBooks.Include(cb => cb.User).Include(cb => cb.Account);

            if (queryParameters.UserId > 0)
            {
                query = query.Where(cb => cb.UserId == queryParameters.UserId);

            }

            if (!string.IsNullOrEmpty(queryParameters.AccountNumber))
            {
                query = query.Where(cb => cb.Account.AccountNumber == queryParameters.AccountNumber);
            }

            if (!string.IsNullOrEmpty(queryParameters.CheckBookCode))
            {
                query = query.Where(cb => cb.CheckBookCode.Contains(queryParameters.CheckBookCode));
            }

            if (!string.IsNullOrEmpty(queryParameters.Status))
            {
                query = query.Where(cb => cb.Status == queryParameters.Status);
            }

            if (!string.IsNullOrEmpty(queryParameters.SortColumn))
            {
                bool isDescending = queryParameters.SortOrder?.ToLower() == "desc";
                switch (queryParameters.SortColumn.ToLower())
                {
                    default:
                        query = isDescending ? query.OrderByDescending(a => a.StatusChangedDate) : query.OrderBy(a => a.StatusChangedDate);
                        break;
                }
            }




            int totalItems = await query.CountAsync();

            var pagedData = await query
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return (pagedData, totalItems);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    //public async Task<CheckBook> CreateCheckBookAsync(CreateCheckBookDTO createCheckBookDTO)
    //{
    //    using (var transaction = await _context.Database.BeginCheckBookAsync())
    //    {
    //        try
    //        {
    //            // Map the CreateCheckBookDTO properties to the new CheckBook entity
    //            var newCheckBook = new CheckBook
    //            {
    //                Amount = createCheckBookDTO.Amount,
    //                CheckBookDescription = createCheckBookDTO?.CheckBookDescription,
    //                DesAccountNumber = createCheckBookDTO.DesAccountNumber,
    //                SourceAccountNumber = createCheckBookDTO.SourceAccountNumber,
    //                CheckBookType = createCheckBookDTO.CheckBookType,
    //                SourceAccountId = createCheckBookDTO.SourceAccountId,
    //                DesAccountId = createCheckBookDTO.DesAccountId,
    //                DesAccountBalanceAfter = createCheckBookDTO.DesAccountBalanceAfter,
    //                SourceAccountBalanceAfter = createCheckBookDTO.SourceAccountBalanceAfter,

    //            };

    //            // Add the new CheckBook to the DbSet
    //            _context.CheckBooks.Add(newCheckBook);
    //            await _context.SaveChangesAsync(); // Save the transaction first to generate CheckBookId
    //            // Commit the transaction if all is successful
    //            await transaction.CommitAsync();
    //            return newCheckBook;
    //        }
    //        catch (Exception ex) // Catch other exceptions
    //        {
    //            await transaction.RollbackAsync(); // Rollback the transaction
    //            throw ex; // Rethrow the exception after rollback
    //        }
    //    }
    //}



    //public async Task<(IEnumerable<CheckBook> Data, int TotalItems)> GetCheckBooksAsync(CheckBookQueryParameters queryParameters)
    //{
    //    try
    //    {
    //        if (!string.IsNullOrEmpty(queryParameters.Period))
    //        {
    //            var dateRange = CalculateDateRange(queryParameters.Period);
    //            queryParameters.StartDate = dateRange.StartDate;
    //            queryParameters.EndDate = dateRange.EndDate;
    //        }
    //        // Define SQL parameters for stored procedure
    //        var accountNumberParam = new SqlParameter("@AccountNumber", (object)queryParameters.AccountNumber ?? DBNull.Value);
    //        var moneyFlowParam = new SqlParameter("@MoneyFlow", (object)queryParameters.MoneyFlow ?? DBNull.Value);
    //        var transactionTypeParam = new SqlParameter("@CheckBookType", (object)queryParameters.CheckBookType ?? DBNull.Value);
    //        var sortColumnParam = new SqlParameter("@SortColumn", ValidateSortColumn(queryParameters.SortColumn));
    //        var sortOrderParam = new SqlParameter("@SortOrder", queryParameters.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC");
    //        var startDateParam = new SqlParameter("@StartDate", (object)queryParameters.StartDate ?? DBNull.Value);
    //        var endDateParam = new SqlParameter("@EndDate", (object)queryParameters.EndDate ?? DBNull.Value);

    //        // Execute stored procedure to get transactions
    //        var transactions = await _context.CheckBooks
    //            .FromSqlRaw("EXEC dbo.GetCheckBooks @AccountNumber, @MoneyFlow, @CheckBookType, @SortColumn, @SortOrder, @StartDate, @EndDate",
    //                         accountNumberParam, moneyFlowParam, transactionTypeParam, sortColumnParam, sortOrderParam, startDateParam, endDateParam)
    //            .ToListAsync();

    //        // Get total items count
    //        int totalItems = transactions.Count;

    //        // Apply pagination
    //        var pagedData = transactions
    //            .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
    //            .Take(queryParameters.PageSize)
    //            .ToList();

    //        return (pagedData, totalItems);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Consider logging the exception
    //        throw;
    //    }
    //}
    //private (DateTime? StartDate, DateTime? EndDate) CalculateDateRange(string period)
    //{
    //    DateTime endDate = DateTime.Now;
    //    DateTime startDate;

    //    switch (period.ToUpper().Trim())
    //    {
    //        case "7DAY":
    //            startDate = endDate.AddDays(-7).Date;
    //            break;
    //        case "1MONTH":
    //            startDate = endDate.AddMonths(-1).Date;
    //            break;
    //        case "3MONTH":
    //            startDate = endDate.AddMonths(-3).Date;
    //            break;
    //        case "6MONTH":
    //            startDate = endDate.AddMonths(-6).Date;
    //            break;
    //        case "9MONTH":
    //            startDate = endDate.AddMonths(-9).Date;
    //            break;
    //        case "12MONTH":
    //            startDate = endDate.AddMonths(-12).Date;
    //            break;
    //        default:
    //            startDate = endDate.AddDays(-7).Date;
    //            break;
    //    }

    //    return (startDate, endDate);
    //}
    //private string ValidateSortColumn(string? sortColumn)
    //{
    //    // Validate allowed columns to prevent SQL injection
    //    var allowedColumns = new[] { "CheckBookDate", "Amount", "CheckBookType", "AccountNumber" };
    //    return allowedColumns.Contains(sortColumn) ? sortColumn : "CheckBookDate";
    //}





    //public async Task<CheckBook> CreateCheckBookAsync(CreateCheckBookDTO createCheckBookDTO)
    //{
    //    // Start a new transaction
    //    using (var transaction = await _context.Database.BeginCheckBookAsync())
    //    {
    //        try
    //        {
    //            // Define parameters for the stored procedure
    //            var transactionTypeParam = new SqlParameter("@CheckBookType", createCheckBookDTO.CheckBookType ?? (object)DBNull.Value);
    //            var sourceAccountNumberParam = new SqlParameter("@SourceAccountNumber", createCheckBookDTO.SourceAccountNumber ?? (object)DBNull.Value);
    //            var desAccountNumberParam = new SqlParameter("@DesAccountNumber", createCheckBookDTO.DesAccountNumber ?? (object)DBNull.Value);
    //            var sourceAccountIdParam = new SqlParameter("@SourceAccountId", createCheckBookDTO.SourceAccountId);
    //            var desAccountIdParam = new SqlParameter("@DesAccountId", createCheckBookDTO.DesAccountId ?? (object)DBNull.Value);
    //            var amountParam = new SqlParameter("@Amount", createCheckBookDTO.Amount);
    //            var sourceBalanceAfterParam = new SqlParameter("@SourceAccountBalanceAfter", createCheckBookDTO.SourceAccountBalanceAfter);
    //            var desBalanceAfterParam = new SqlParameter("@DesAccountBalanceAfter", createCheckBookDTO.DesAccountBalanceAfter ?? (object)DBNull.Value);
    //            var descriptionParam = new SqlParameter("@CheckBookDescription", createCheckBookDTO.CheckBookDescription ?? (object)DBNull.Value);
    //            var messageParam = new SqlParameter("@CheckBookMessage", createCheckBookDTO.CheckBookMessage ?? (object)DBNull.Value);
    //            var transactionCodeParam = new SqlParameter("@CheckBookCode", createCheckBookDTO.CheckBookCode); // Corrected this line

    //            var transactionDateOutputParam = new SqlParameter
    //            {
    //                ParameterName = "@CheckBookDate",
    //                SqlDbType = System.Data.SqlDbType.DateTime,
    //                Direction = System.Data.ParameterDirection.Output
    //            };

    //            // Execute the stored procedure
    //            var result = await _context.CheckBooks
    //                .FromSqlRaw("EXEC dbo.InsertCheckBook @CheckBookCode, @CheckBookType, @SourceAccountNumber, @DesAccountNumber, @SourceAccountId, @DesAccountId, @Amount, @SourceAccountBalanceAfter, @DesAccountBalanceAfter, @CheckBookDescription, @CheckBookMessage, @CheckBookDate OUT",
    //                            transactionCodeParam, transactionTypeParam, sourceAccountNumberParam, desAccountNumberParam, sourceAccountIdParam, desAccountIdParam,
    //                            amountParam, sourceBalanceAfterParam, desBalanceAfterParam, descriptionParam, messageParam, transactionDateOutputParam)
    //                .ToListAsync();

    //            // Commit the transaction if the stored procedure executed successfully
    //            await transaction.CommitAsync();

    //            // Return the first result, as we expect only one inserted record
    //            return result.FirstOrDefault();
    //        }
    //        catch (Exception ex)
    //        {
    //            // Log the exception if needed
    //            Console.WriteLine($"Error occurred: {ex.Message}");

    //            // Rollback will happen automatically when the 'using' block exits without committing
    //            throw;
    //        }
    //    }
    //}



    //public async Task<Decimal> CalculateTotalTransferedAmountPerDay(string accountNumber)
    //{

    //    // Assuming you have a DbContext named 'context' and a TransferLimit property in your Account entity
    //    var account = await _context.Accounts.FirstOrDefaultAsync(a=>a.AccountNumber == accountNumber);
    //    if (account == null)
    //    {
    //        throw new Exception("Account not found");
    //    }

    //    var today = DateTime.Today;
    //    var totalTransferredToday = await _context.CheckBooks
    //        .Where(t => t.SourceAccountNumber == accountNumber && t.CheckBookDate >= today)
    //        .SumAsync(t => t.Amount);

    //    return totalTransferredToday;
    //}

}