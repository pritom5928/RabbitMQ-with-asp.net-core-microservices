using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Bus
{
    public interface IEventBus
    {
        //takes any type of command by mediator
        Task SendCommand<T>(T command) where T : Command;


        //publish any type of event 
        void Publish<T>(T @event) where T : Event;
        

        //subscribe T event on TH eventhandler
        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;

    }
}
