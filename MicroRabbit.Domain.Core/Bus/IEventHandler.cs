using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Bus
{
    public interface IEventHandler
    {
    }

    //takes any type of event
    public interface IEventHandler<in TEvent>  : IEventHandler where TEvent : Event
    {
        Task Handle(TEvent @event);
    }
}
