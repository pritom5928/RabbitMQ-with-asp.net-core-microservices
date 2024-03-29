﻿using MediatR;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Services;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Data.Implementations;
using MicroRabbit.Banking.Domain.CommandHandlers;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Event;
using MicroRabbit.Banking.Domain.EventHandlers;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Application.Services;
using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Data.Implementations;
using MicroRabbit.Transfer.Domain.CommandHandlers;
using MicroRabbit.Transfer.Domain.Commands;
using MicroRabbit.Transfer.Domain.Event;
using MicroRabbit.Transfer.Domain.EventHandlers;
using MicroRabbit.Transfer.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Ioc
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            //Domain bus
            //services.AddTransient<IEventBus, RabbitMQBus>();

            services.AddSingleton<IEventBus, RabbitMQBus>(sp =>
            {
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

                return new RabbitMQBus(sp.GetService<IMediator>(), scopeFactory);
            });

            //services.AddScoped<IEventBus, RabbitMQBus>(sp => 
            //{
            //    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            //    return new RabbitMQBus(sp.GetService<IMediator>(), scopeFactory);
            //});


            //subsciptions
            services.AddTransient<TransferEventHandler>();
            services.AddTransient<TransferLogGeneratedEventHandler>();

            //domain commands
            services.AddTransient<IEventHandler<Transfer.Domain.Event.TransferCreatedEvent>, TransferEventHandler>();
            services.AddTransient<IEventHandler<MicroRabbit.Banking.Domain.Event.TransferLogGeneratedEvent>, TransferLogGeneratedEventHandler>();

            //domain commands
            services.AddTransient<IRequestHandler<CreateTransferCommand, bool>, TransferCommandHandler>();

            services.AddTransient<IRequestHandler<TransferLogGeneratedCommand, bool>, TransferLogGeneratedCommandHandler>();

            //Data services
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<ITransferService, TransferService>();

            //Application services
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<ITransferRepository, TransferRepository>();
            services.AddTransient<BankingDbContext>();
            services.AddTransient<TransferDbContext>();
        }
    }
}
