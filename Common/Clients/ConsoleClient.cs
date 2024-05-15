using Common.DAL;
using Common.DAL.Models;
using Common.Enums;
using Common.Helpers;
using Common.Services;
using Common.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;

namespace Common.Clients
{
    public abstract class ConsoleClient : IConsoleClient
    {
        private IServiceProvider ServiceProvider { get; }
        protected IAnsiConsole Console { get; }
        protected IDepotContext Context { get; }
        protected ClientType ClientType { get; }

        protected bool IsRunning { get; set; } = true;
        protected bool IsAuthenticated { get; set; } = false;
        protected bool ShowSubmenu { get; set; } = false;
        protected bool Contained { get; set; } = false;

        // Encapsulate DateTime.Now for easier testing
        protected IDateTimeService DateTime { get; }

        // Used in the client for general purposes
        protected ILocalizationService Localization { get; }
        protected IPromptService Prompts { get; }
        protected ISettingsService Settings { get; }

        // Used for validation
        protected ITicketService TicketService { get; }
        protected IUserService UserService { get; }
        protected ITourService TourService { get; }

        protected User? User { get; set; }
        protected Ticket? Ticket { get; set; }

        public ConsoleClient(IServiceProvider serviceProvider, ClientType clientType)
        {
            ClientType = clientType;
            ServiceProvider = serviceProvider;
            Context = serviceProvider.GetService<IDepotContext>()!;
            Console = serviceProvider.GetService<IAnsiConsole>()!;

            Localization = serviceProvider.GetService<ILocalizationService>()!;
            Prompts = serviceProvider.GetService<IPromptService>()!;
            Settings = serviceProvider.GetService<ISettingsService>()!;
            TicketService = serviceProvider.GetService<ITicketService>()!;
            UserService = serviceProvider.GetService<IUserService>()!;
            TourService = serviceProvider.GetService<ITourService>()!;

            DateTime = serviceProvider.GetService<IDateTimeService>()!;

            // Setup context
            try
            {
                ((DepotContext)ServiceProvider.GetService<IDepotContext>()!).Initialize();
            }
            catch (Exception ex) { Console.MarkupLine(ExceptionHandler.HandleException(ex)); }
            
            //Console.MarkupLine($"[green]{clientType} Client Initialized.[/]");
        }

        public virtual void Run()
        {
            while (IsRunning)
            {
                bool hasAccess;
                if (ClientType == ClientType.Master)
                {
                    hasAccess = true;
                }
                else if (ClientType != ClientType.Kiosk)
                {
                    var userId = Prompts.AskUserId();
                    User = UserService.GetOne(userId);
                    hasAccess = UserService.ValidateUserForClient(User, ClientType).Valid;
                }
                else
                {
                    var ticketId = Prompts.AskTicketNumber();
                    Ticket = TicketService.GetOne(ticketId);
                    hasAccess = Ticket != null;
                }
                Console.Clear();

                IsAuthenticated = hasAccess;

                while (IsAuthenticated)
                    ShowMainMenu().Invoke();
            }
        }

        protected void CloseMenu(string? message = null, bool closeSubMenu = false, bool logout = false, bool closeClient = false, int delayInMs = 1500, bool clear = true)
        {
            if (message != null)
            {
                Console.MarkupLine(message);
                Thread.Sleep(delayInMs);
            }

            if (clear)
                Console.Clear();

            if (closeSubMenu)
            {
                ShowSubmenu = false;

                if (logout)
                    IsAuthenticated = false;

                if (closeClient)
                    IsRunning = false;
            }
        }

        protected T GetFlow<T>() where T : Workflow
        {
            return ServiceProvider.GetService<T>()!;
        }

        protected T GetClient<T>(ClientType clientType) where T : ConsoleClient
        {
            ConsoleClient client = clientType switch
            {
                ClientType.Manager => new ManagerClient(ServiceProvider),
                ClientType.Kiosk => new KioskClient(ServiceProvider),
                ClientType.Guide => new GuideClient(ServiceProvider),
                _ => throw new ArgumentOutOfRangeException($"{clientType} has no associated client to instantiate.")
            };

            return (client as T)!;
        }

        protected abstract Action ShowMainMenu();

        public void RunsContained() => Contained = true;
    }
}
