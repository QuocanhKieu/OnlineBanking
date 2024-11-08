using Microsoft.CodeAnalysis.FlowAnalysis;
using T2305M_API.DTO.CheckBook;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Repositories;
using T2305M_API.Repositories.Implements;
using T2305M_API.Services;
using Microsoft.EntityFrameworkCore;
using T2305M_API.DTO.CheckBook;
using T2305M_API.DTO.Transaction;

public class CheckBookService : ICheckBookService
{
    private readonly ICheckBookRepository _transactionRepository;
    private readonly IWebHostEnvironment _env;
    private readonly T2305mApiContext _context;


    public CheckBookService(ICheckBookRepository transactionRepository,
        IWebHostEnvironment env,
        T2305mApiContext context)
    {
        _transactionRepository = transactionRepository;
        _env = env;
        _context = context;

    }

    public async Task<PaginatedResult<GetDetailCheckBookDTO>> GetCheckBooksAsync(CheckBookQueryParameters queryParameters)
    {
        var (data, totalItems) = await _transactionRepository.GetCheckBooksAsync(queryParameters);

        var basicTransactionDTOs = new List<GetDetailCheckBookDTO>();

        foreach (var transaction in data)
        {
            basicTransactionDTOs.Add(new GetDetailCheckBookDTO
            {
                AssociatedAccountNumber = transaction.Account.AccountNumber,
                CheckBookCode = transaction.CheckBookCode,
                CheckBookId = transaction.CheckBookId,
                ChecksRemaining = transaction.ChecksRemaining,
                DeliveryAddress = transaction.DeliveryAddress,
                ExpiryDate = transaction.ExpiryDate,
                LastCheckClearedDate = transaction.LastCheckClearedDate,
                LastClearedCheckCode = transaction.LastClearedCheckCode,
                Status = transaction.Status,
                TotalChecks = transaction.TotalChecks,
                TotalClearedCheckAmount = transaction.TotalClearedCheckAmount,
                
            });
        }
        // Calculate total pages
        int totalPages = (int)Math.Ceiling((double)totalItems / queryParameters.PageSize);

        return new PaginatedResult<GetDetailCheckBookDTO>
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

    public async Task<CheckBook> CreateCheckBookAsync(CreateCheckBookDTO createCheckBookDTO, int userId, int accountId)
    {

            try
            {
                var newCheckBook = new CheckBook
                {
                    Status = "PENDING",
                    StatusChangedDate = DateTime.Now,
                    UserId = userId,
                    AccountId = accountId,
                    CheckBookCode = $"CB{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 13)}",
                    ChecksRemaining = 0,
                    TotalChecks = 0,
                    TotalClearedCheckAmount = 0,
                    DeliveryAddress = createCheckBookDTO.DeliveryAddress,
                    ExpiryDate = DateTime.Now.AddMonths(6),
                    
                };


                await _context.CheckBooks.AddAsync(newCheckBook);
                await _context.SaveChangesAsync();





                return newCheckBook;

            }
            catch (Exception ex) // Catch other exceptions
            {
                throw ex; // Rethrow the exception after rollback
            }
    }

    public async Task<bool> CreateChecksAsync(CheckBook checkBook, int quantity)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var checks = new List<Check>();

                for (int i = 0; i < quantity; i++)
                {
                    var newCheck = new Check
                    {
                        CheckBookId = checkBook.CheckBookId,
                        StatusChangedDate = checkBook.StatusChangedDate,
                        // Initialize other fields as necessary
                    };

                    checks.Add(newCheck);
                }

                await _context.Checks.AddRangeAsync(checks);

                checkBook.TotalChecks = quantity;
                checkBook.ChecksRemaining = quantity;
                _context.CheckBooks.Update(checkBook);
                await _context.SaveChangesAsync();



                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex) // Catch other exceptions
            {
                await transaction.RollbackAsync(); // Rollback the transaction
                throw; // Rethrow the exception after rollback
            }
        }
    }

}