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
    }
}
