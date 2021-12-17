using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Transfer.Domain.Commands;
using MicroRabbit.Transfer.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroRabbit.Transfer.Domain.CommandHandlers
{
    public class TransferLogGeneratedCommandHandler : IRequestHandler<TransferLogGeneratedCommand, bool>
    {
        private readonly IEventBus _bus;
        public TransferLogGeneratedCommandHandler(IEventBus bus)
        {
            _bus = bus;
        }

        public Task<bool> Handle(TransferLogGeneratedCommand request, CancellationToken cancellationToken)
        {
            //publish event to rabbitmq server
            _bus.Publish(new TransferLogGeneratedEvent(request.FromAccount, request.ToAccount, request.TransferAmount));

            return Task.FromResult(true);
        }
    }
}
