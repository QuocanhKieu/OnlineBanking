using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using T2305M_API.DTO.SavingsAccount;
using T2305M_API.Entities;
using T2305M_API.Repositories;

public class SavingsAccountRepository : ISavingsAccountRepository
{
    private readonly T2305mApiContext _context;
    private readonly IWebHostEnvironment _env;

    public SavingsAccountRepository(T2305mApiContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<(IEnumerable<SavingsAccount> Data, int TotalItems)> GetSavingsAccountsAsync(SavingsAccountQueryParameters queryParameters)
    {
        IQueryable<SavingsAccount> query = _context.SavingsAccounts;

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
                a.SavingsAccountCode.Trim().Contains(searchTerm)
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

}