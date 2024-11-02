using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace T2305M_API.Entities;

public partial class T2305mApiContext : DbContext
{
    public static string ConnectionString;
    public T2305mApiContext()
    {
    }

    public T2305mApiContext(DbContextOptions<T2305mApiContext> options)
        : base(options)
    {
    }


    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Check> Checks { get; set; }
    public DbSet<CheckBook> CheckBooks { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<Message> Messages { get; set; }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Define the relationship for TransactionsFrom (Outgoing transactions)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.SourceAccount)
            .WithMany(a => a.TransactionsFrom)
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes if needed

        // Define the relationship for TransactionsTo (Incoming transactions)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.DesAccount)
            .WithMany(a => a.TransactionsTo)
            .HasForeignKey(t => t.DesAccountId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes if needed


        base.OnModelCreating(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}