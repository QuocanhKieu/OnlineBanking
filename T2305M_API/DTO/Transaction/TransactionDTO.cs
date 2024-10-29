﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2305M_API.Entities;


namespace T2305M_API.DTO.Transaction
{
    //Basic/Detail with the same DTO
    public class GetBasicTransactionDTO
    {
        public string TransactionType { get; set; } // BANKTRANSFER, CHECKPAYMENT
        public string SourceAccountNumber { get; set; }
        public string? DesAccountNumber { get; set; }
        public int SourceAccountId { get; set; }
        public int? DesAccountId { get; set; }
        public string SourceUserName { get; set; } 
        public string? DesUserName { get; set; } 
        public decimal Amount { get; set; }
        public decimal? BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? TransactionDescription { get; set; }
    }

    public class CreateTransactionDTO
    {
        public string TransactionType { get; set; } // BANKTRANSFER, CHECKPAYMENT
        public string SourceAccountNumber { get; set; }
        public string? DesAccountNumber { get; set; }
        public int SourceAccountId { get; set; }
        public int? DesAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal SourceAccountBalanceAfter { get; set; }
        public decimal? DesAccountBalanceAfter { get; set; }
        public string? TransactionDescription { get; set; }
        public string? TransactionMessage { get; set; }
    }

    public class TransactionQueryParameters
    {
        public string AccountNumber { get; set; }
        const int maxPageSize = 50;
        public int Page { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }
        public string? SearchTerm { get; set; }
        public string? TransactionType { get; set; } // INFLOW, OUTFLOW//tùy vào context//khác nhau
        public string? Period { get; set; } // 7DAYS, 1MONTH, 3MONTH, 6MONTH, 9MONTH, 12MONTH
        public DateTime? StartDate {  get; set; }
        public DateTime? EndDate {  get; set; }
        public string? SortColumn { get; set; } = "TransactionDate";
        public string? SortOrder { get; set; } = "desc";  // "asc" or "desc" for sorting order
    }
}