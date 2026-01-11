using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class BankTransactionService : IBankTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankTransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BankTransaction>> GetAllTransactionsAsync()
        {
            return await _unitOfWork.BankTransactionRepository.GetAllAsync();
        }

        public async Task<BankTransaction> GetTransactionByIdAsync(int id)
        {
            return await _unitOfWork.BankTransactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<BankTransaction>> GetTransactionsByUserIdAsync(int userId)
        {
            return await _unitOfWork.BankTransactionRepository.FindAsync(t => t.UserId == userId);
        }

        public async Task<IEnumerable<BankTransaction>> GetTransactionsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.BankTransactionRepository.FindAsync(t =>
                t.UserId == userId &&
                t.TransactionDate >= startDate &&
                t.TransactionDate <= endDate);
        }

        public async Task<IEnumerable<BankTransaction>> GetUnmatchedTransactionsAsync(int userId)
        {
            return await _unitOfWork.BankTransactionRepository.FindAsync(t => t.UserId == userId && !t.IsMatched);
        }

        public async Task<BankTransaction> CreateTransactionAsync(BankTransaction transaction)
        {
            transaction.CreatedDate = DateTime.UtcNow;
            transaction.ImportDate = DateTime.UtcNow;
            await _unitOfWork.BankTransactionRepository.AddAsync(transaction);
            await _unitOfWork.CommitAsync();
            return transaction;
        }

        public async Task<BankTransaction> UpdateTransactionAsync(int id, BankTransaction transaction)
        {
            var existingTransaction = await GetTransactionByIdAsync(id);
            if (existingTransaction == null)
                return null;

            existingTransaction.Description = transaction.Description;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.BalanceAfterTransaction = transaction.BalanceAfterTransaction;

            _unitOfWork.BankTransactionRepository.Update(existingTransaction);
            await _unitOfWork.CommitAsync();
            return existingTransaction;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await GetTransactionByIdAsync(id);
            if (transaction == null)
                return false;

            _unitOfWork.BankTransactionRepository.Remove(transaction);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<BankTransaction> MatchTransactionAsync(int transactionId, string clCardCode, string clCardName, int userId)
        {
            var transaction = await GetTransactionByIdAsync(transactionId);
            if (transaction == null)
                return null;

            transaction.MatchedClCardCode = clCardCode;
            transaction.MatchedClCardName = clCardName;
            transaction.IsMatched = true;
            transaction.MatchedDate = DateTime.UtcNow;
            transaction.MatchedByUserId = userId;

            _unitOfWork.BankTransactionRepository.Update(transaction);
            await _unitOfWork.CommitAsync();
            return transaction;
        }

        public async Task<BankTransaction> TransferTransactionAsync(int transactionId, int userId)
        {
            var transaction = await GetTransactionByIdAsync(transactionId);
            if (transaction == null || !transaction.IsMatched)
                return null;

            transaction.IsTransferred = true;
            transaction.TransferredDate = DateTime.UtcNow;
            transaction.TransferredByUserId = userId;

            _unitOfWork.BankTransactionRepository.Update(transaction);
            await _unitOfWork.CommitAsync();
            return transaction;
        }

        public async Task<int> ImportTransactionsBatchAsync(List<BankTransaction> transactions, int userId, int bankId)
        {
            try
            {
                foreach (var transaction in transactions)
                {
                    transaction.UserId = userId;
                    transaction.BankId = bankId;
                    transaction.ImportDate = DateTime.UtcNow;
                    transaction.CreatedDate = DateTime.UtcNow;

                    await _unitOfWork.BankTransactionRepository.AddAsync(transaction);
                }

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}