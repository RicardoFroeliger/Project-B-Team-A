﻿using Common.DAL;
using Common.DAL.Models;
using Common.Services.Interfaces;

namespace Common.Services
{
    public class TicketService : BaseService, ITicketService
    {
        public ISettingsService Settings { get; }
        private ILocalizationService Localization { get; }

        public TicketService(DepotContext context, ISettingsService settings, ILocalizationService localization)
            : base(context)
        {
            Localization = localization;
            Settings = settings;
        }

        public (bool Valid, string Message) ValidateTicketNumber(int ticketNumber)
        {
            var ticket = Context.Tickets.FirstOrDefault(ticket => ticket.Id == ticketNumber);

            if (ticket == null)
                return new(false, Localization.Get("Ticket_does_not_exist"));

            if (ticket.ValidOn.Date != DateTime.Now.Date && ticket.Expires)
                return new(false, Localization.Get("Ticket_not_valid_today"));

            return new(true, Localization.Get("Ticket_is_valid"));
        }

        public Ticket? GetTicket(int ticketNumber)
        {
            return Context.Tickets.Find(ticketNumber);
        }
    }
}
