using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface IBankTransactionService
    {
        Task<IEnumerable<BankTransaction>> GetAllTransactionsAsync();
        Task<BankTransaction> GetTransactionByIdAsync(int id);
        Task<IEnumerable<BankTransaction>> GetTransactionsByUserIdAsync(int userId);
        Task<IEnumerable<BankTransaction>> GetTransactionsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<BankTransaction>> GetUnmatchedTransactionsAsync(int userId);
        Task<BankTransaction> CreateTransactionAsync(BankTransaction transaction);
        Task<BankTransaction> UpdateTransactionAsync(int id, BankTransaction transaction);
        Task<bool> DeleteTransactionAsync(int id);
        Task<BankTransaction> MatchTransactionAsync(int transactionId, string clCardCode, string clCardName, int userId);
        Task<BankTransaction> TransferTransactionAsync(int transactionId, int userId);
        Task<int> ImportTransactionsBatchAsync(List<BankTransaction> transactions, int userId, int bankId);
    }
}