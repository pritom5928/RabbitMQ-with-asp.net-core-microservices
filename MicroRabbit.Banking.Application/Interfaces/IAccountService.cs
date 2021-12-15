using MicroRabbit.Banking.Application.ViewModels;
using MicroRabbit.Banking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Banking.Application.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<Account> GetAllAccounts();
        void Transfer(AccountTransfer accountTransfer);
    }
}
