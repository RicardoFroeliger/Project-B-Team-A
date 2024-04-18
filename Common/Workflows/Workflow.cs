﻿using Common.DAL;
using Common.DAL.Models;
using Common.Services;

namespace Common.Workflows
{
    public class Workflow
    {
        public LocalizationService Localization { get; }
        public DepotContext Context { get; }
        public TicketService TicketService { get; }

        public Workflow(DepotContext context, LocalizationService localizationService, TicketService ticketService)
        {
            Context = context;
            Localization = localizationService;
            TicketService = ticketService;
        }

        public virtual (bool Succeeded, string Message) Commit()
        {
            Context.SaveChanges();

            return (true, Localization.Get("Commit_successful"));
        }

        public virtual (bool Succeeded, string Message) Rollback()
        {
            return (true, Localization.Get("Rollback_successful"));
        }

        protected (bool Success, string Message) ValidateTicket(int ticketNumber) => ValidateTicket(TicketService.GetTicket(ticketNumber));

        protected (bool Success, string Message) ValidateTicket(Ticket? ticket)
        {
            if (ticket == null)
                return new(false, Localization.Get("Flow_ticket_invalid"));

            if (!TicketService.ValidateTicketNumber(ticket.Id).Valid)
                return new(false, Localization.Get("Flow_ticket_invalid"));

            return new(true, Localization.Get("Flow_ticket_valid"));
        }
    }
}
