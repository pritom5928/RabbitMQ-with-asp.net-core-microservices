using MicroRabbit.Banking.Domain.Event;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Banking.Domain.EventHandlers
{
    public class TransferLogGeneratedEventHandler : IEventHandler<TransferLogGeneratedEvent>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEventBus _bus;

        public TransferLogGeneratedEventHandler(IAccountRepository accountRepository, IEventBus bus)
        {
            _accountRepository = accountRepository;
            _bus = bus;
        }

        public Task Handle(TransferLogGeneratedEvent @event)
        {
            _accountRepository.UpdateBalanceByTransferLog(@event.FromAccount, @event.ToAccount, @event.TransferAmount);

            return Task.CompletedTask;
        }
    }
}
