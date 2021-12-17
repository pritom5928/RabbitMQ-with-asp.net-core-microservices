using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Transfer.Domain.Events
{
    public class TransferLogGeneratedEvent : MicroRabbit.Domain.Core.Events.Event
    {
        public int FromAccount { get; private set; }
        public int ToAccount { get; private set; }
        public decimal TransferAmount { get; private set; }

        public TransferLogGeneratedEvent(int fromAccount, int toAccount, decimal transferAmount)
        {
            FromAccount = fromAccount;
            ToAccount = toAccount;
            TransferAmount = transferAmount;
        }

    }
}
