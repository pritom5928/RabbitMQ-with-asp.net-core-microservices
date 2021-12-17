using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Banking.Data.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly BankingDbContext _db;
        public AccountRepository(BankingDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Account> GetAccounts()
        {
            return _db.Accounts;
        }

        public void UpdateBalanceByTransferLog(int fromAccount, int toAccount, decimal transferAmount)
        {
            var accounts = _db.Accounts.Where(w => w.Id == fromAccount || w.Id == toAccount).ToList();

            accounts.FirstOrDefault(f => f.Id == fromAccount).Acccountbalance -= transferAmount;
            accounts.FirstOrDefault(f => f.Id == toAccount).Acccountbalance += transferAmount;

            _db.SaveChanges();
        }
    }
}
