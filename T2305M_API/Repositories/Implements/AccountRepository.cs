using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using T2305M_API.DTO.Account;
using T2305M_API.Entities;
using T2305M_API.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly T2305mApiContext _context;
    private readonly IWebHostEnvironment _env;

    public AccountRepository(T2305mApiContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<(IEnumerable<Account> Data, int TotalItems)> GetAccountsAsync(AccountQueryParameters queryParameters)
    {
        IQueryable<Account> query = _context.Accounts;

        // Filter by IsRecommended
        if (queryParameters.Userid > 0)
        {
            query = query.Where(a => a.UserId == queryParameters.Userid);
        }
        var searchTerm = queryParameters.SearchTerm?.ToLower().Trim() ?? "";
        // Filter by search term (title, author, etc.)
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(a =>
                a.AccountNumber.Trim().Contains(searchTerm)
            );
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(queryParameters.SortColumn))
        {
            bool isDescending = queryParameters.SortOrder?.ToLower() == "desc";
            switch (queryParameters.SortColumn.ToLower())
            {
                default:
                    query = isDescending ? query.OrderByDescending(a => a.Balance) : query.OrderBy(a => a.Balance);
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
    public async Task CreateAccountAsync(CreateAccountDTO createAccountDTO, int userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Map the CreateAccountDTO properties to the new Account entity
                var newAccount = new Account
                {
                    UserId = userId,
                    AccountNumber = createAccountDTO.AccountNumber
                };

                // Add the new Account to the DbSet
                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync(); // Save the account first to generate AccountId
                // Commit the transaction if all is successful
                await transaction.CommitAsync();
            }
            catch (Exception ex) // Catch other exceptions
            {
                await transaction.RollbackAsync(); // Rollback the transaction
                throw ex; // Rethrow the exception after rollback
            }
        }
    }
    public async Task<IEnumerable<Account>> ListLikeAccountsAsync(string accountNumber, int userId)
    {
        // Assuming you have a DbContext named 'context' and an Account entity
        return await _context.Accounts
                            .Where(a => a.AccountNumber.Contains(accountNumber) && a.UserId != userId)
                            .ToListAsync();
    }

}