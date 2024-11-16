using T2305M_API.Entities;

namespace T2305M_API.Helper
{
    public static class NotificationHelper
    {
        public static string GenerateBalanceChangeNotification(BalanceChangeDTO balanceChangeDTO)
        {
            return $"Balance Change Notification\n" +
                   $"Account: {balanceChangeDTO.AccountNumber} | " +
                   $"Type: {balanceChangeDTO.TransactionType}\n" +
                   $"Amount: {balanceChangeDTO.TransactionAmount}{balanceChangeDTO.Currency}\n" +
                   $"Time: {balanceChangeDTO.TransactionDateTime:yyyy-MM-dd HH:mm:ss}\n" +
                   $"Balance: {balanceChangeDTO.NewBalance} | " +
                   $"Message: {balanceChangeDTO.TransactionDescription}\n" +
                   $"TransCode: {balanceChangeDTO.TransactionCode}";
        }
        public class BalanceChangeDTO
        {
            public string? AccountNumber { get; set; }
            public string? Currency { get; set; }
            public string? TransactionType { get; set; }
            public string? TransactionDescription { get; set; }
            public string? TransactionCode { get; set; }
            public string? TransactionAmount { get; set; } // - or +
            public decimal? NewBalance { get; set; } 
            public DateTime TransactionDateTime { get; set; }
        }
    }
}
