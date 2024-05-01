using Common.Choices;
using Common.DAL.Models;
using Common.Enums;
using Common.Services.Interfaces;
using Spectre.Console;

namespace Common.Services
{
    public class PromptService : IPromptService
    {
        public ISettingsService Settings { get; }
        public ILocalizationService Localization { get; }
        public ITicketService TicketService { get; }
        public ITourService TourService { get; }
        public IUserService UserService { get; }

        public PromptService(ISettingsService settings, ILocalizationService localizationService,
            ITicketService ticketService, ITourService tourService, IUserService userService)
        {
            Localization = localizationService;
            TicketService = ticketService;
            UserService = userService;
            TourService = tourService;
            Settings = settings;
        }

        public List<DayOfWeek> AskSchedule()
        {
            return ConsoleWrapper.Console.Prompt(
                new MultiSelectionPrompt<WorkdayChoice>()
                    .Title(Localization.Get("Ask_schedule_title"))
                    .NotRequired() // Not required to have a workday
                    .PageSize(7)
                    .InstructionsText(Localization.Get("Ask_schedule_instructions"))
                    .AddChoices([
                        new WorkdayChoice(Localization.Get("Day_Monday"), DayOfWeek.Monday),
                        new WorkdayChoice(Localization.Get("Day_Tuesday"), DayOfWeek.Tuesday),
                        new WorkdayChoice(Localization.Get("Day_Wednesday"), DayOfWeek.Wednesday),
                        new WorkdayChoice(Localization.Get("Day_Thursday"), DayOfWeek.Thursday),
                        new WorkdayChoice(Localization.Get("Day_Friday"), DayOfWeek.Friday),
                        new WorkdayChoice(Localization.Get("Day_Saturday"), DayOfWeek.Saturday),
                        new WorkdayChoice(Localization.Get("Day_Sunday"), DayOfWeek.Sunday)
                    ])).Select(q => q.Day).ToList();
        }

        public User AskUser(List<User> options)
        {
            var choice = ConsoleWrapper.Console.Prompt(
               new SelectionPrompt<UserChoice>()
                   .Title(Localization.Get("Ask_user_title"))
                   .PageSize(10)
                    .MoreChoicesText(Localization.Get("Ask_user_more_choices"))
                   .AddChoices(options.Select(q => new UserChoice(q.Name, q))));
            return choice.User;
        }

        public int AskNumber(string questionKey, string validationErrorKey, int? min = null, int? max = null)
        {
            return ConsoleWrapper.Console.Prompt(
                new TextPrompt<int>(Localization.Get(questionKey))
                    .PromptStyle("green")
                    .ValidationErrorMessage(Localization.Get(validationErrorKey))
                    .Validate(inputNumber =>
                    {
                        if (min != null && inputNumber < min)
                            return ValidationResult.Error(Localization.Get("Input_below_minimum"));

                        if (max != null && inputNumber > max)
                            return ValidationResult.Error(Localization.Get("Input_exceeds_capacity"));

                        return ValidationResult.Success();
                    }));
        }

        public int AskTicketNumber()
        {
            return ConsoleWrapper.Console.Prompt(
                new TextPrompt<int>(Localization.Get("Scan_ticket"))
                    .PromptStyle("green")
                    .ValidationErrorMessage(Localization.Get("Invalid_ticket_number"))
                    .Validate(ticketNumberInput =>
                    {
                        var response = TicketService.ValidateTicketNumber(ticketNumberInput);

                        return response.Valid ? ValidationResult.Success()
                            : ValidationResult.Error(response.Message);
                    }));
        }

        public int AskTicketNumberOrUserpass()
        {
            return ConsoleWrapper.Console.Prompt(
                new TextPrompt<int>(Localization.Get("Scan_ticket_or_userpass"))
                    .PromptStyle("green")
                    .ValidationErrorMessage(Localization.Get("Invalid_ticket_number_or_userpass"))
                    .Validate(numberInput =>
                    {
                        var responseUser = UserService.ValidateUserpass(numberInput);
                        if (responseUser.Valid)
                            return ValidationResult.Success();

                        var responseTicket = TicketService.ValidateTicketNumber(numberInput);
                        if (responseTicket.Valid)
                            return ValidationResult.Success();

                        return ValidationResult.Error(responseTicket.Message);
                    }));
        }

        public int AskUserpass()
        {
            return ConsoleWrapper.Console.Prompt(
                new TextPrompt<int>(Localization.Get("Scan_userpass"))
                    .PromptStyle("green")
                    .ValidationErrorMessage(Localization.Get("Invalid_userpass"))
                    .Validate(userpassInput =>
                    {
                        var response = UserService.ValidateUserpass(userpassInput);

                        return response.Valid ? ValidationResult.Success()
                            : ValidationResult.Error(response.Message);
                    }));
        }

        public DateTime AskDate(string titleTranslationKey, string moreOptionsTranslationKey, int dateRange = 31, DateTime? startDate = null, bool historical = false)
        {
            var start = startDate ?? DateTime.Today.Date;
            var dateChoices = Enumerable.Range(0, dateRange).Select(offset => new DateChoice(start.AddDays(historical ? -offset : offset)));

            var choice = ConsoleWrapper.Console.Prompt(
                new SelectionPrompt<DateChoice>()
                    .Title(Localization.Get(titleTranslationKey))
                    .PageSize(10)
                    .MoreChoicesText(Localization.Get(moreOptionsTranslationKey))
                    .AddChoices(dateChoices));

            return choice.Date;
        }


        public TimeSpan AskTime(string titleTranslationKey, string moreOptionsTranslationKey, int timeInterval = 30, int startTime = 0)
        {
            var minutes = startTime;
            var timeChoices = new List<TimeChoice>();
            while (minutes <= 1440)
            {
                minutes = Math.Min(1440, minutes);
                timeChoices.Add(new TimeChoice(new TimeSpan(0, minutes, 0)));
                minutes += timeInterval;
            }

            var choice = ConsoleWrapper.Console.Prompt(
                new SelectionPrompt<TimeChoice>()
                    .Title(Localization.Get(titleTranslationKey))
                    .PageSize(10)
                    .MoreChoicesText(Localization.Get(moreOptionsTranslationKey))
                    .AddChoices(timeChoices));

            return choice.Span;
        }

        public NavigationChoice GetMenu(string titleTranslationKey, string moreOptionsTranslationKey, List<NavigationChoice> navigationChoices, User? user = null)
        {
            var replacementList = new List<string>();
            if (user != null)
                replacementList = new() { user.Name, Localization.Get(((Role)user.Role).ToString()) };

            return ConsoleWrapper.Console.Prompt(
                new SelectionPrompt<NavigationChoice>()
                    .Title(Localization.Get(titleTranslationKey, replacementStrings: replacementList))
                    .PageSize(10)
                    .MoreChoicesText(Localization.Get(moreOptionsTranslationKey))
                    .AddChoices(navigationChoices));
        }

        public Tour AskTour(string titleTranslationKey, string moreOptionsTranslationKey, int minimumCapacity, int recentTours = 0, int upcomingTours = -1)
        {
            int maxCapacity = Settings.GetValueAsInt("Max_capacity_per_tour")!.Value;

            var tourChoices = TourService.GetToursForToday(minimumCapacity, recentTours, upcomingTours)
                .Select(tour => new TourChoice(Localization.Get("Select_tour_item", replacementStrings: new() { tour.Start.ToString("HH:mm"), $"[green]({tour.RegisteredTickets.Count}/{maxCapacity})[/]" }), tour));

            var choice = ConsoleWrapper.Console.Prompt(
                new SelectionPrompt<TourChoice>()
                    .Title(Localization.Get(titleTranslationKey))
                    .PageSize(10)
                    .MoreChoicesText(Localization.Get(moreOptionsTranslationKey))
                    .AddChoices(tourChoices));

            return choice.Tour;
        }

        public bool AskConfirmation(string titleTranslationKey)
        {
            var choice = ConsoleWrapper.Console.Prompt(
               new SelectionPrompt<BoolChoice>()
                   .Title(Localization.Get(titleTranslationKey))
                   .PageSize(10)
                   .AddChoices(new List<BoolChoice>() {
                        new(Localization.Get("Choice_yes"), true),
                        new(Localization.Get("Choice_no"), false)
                   }));
            return choice.Choice;
        }

        public string AskUsername()
        {
            return ConsoleWrapper.Console.Prompt(
                new TextPrompt<string>(Localization.Get("Ask_username"))
                    .PromptStyle("green")
                    .ValidationErrorMessage(Localization.Get("Invalid_username"))
                    .Validate(username =>
                    {
                        return ValidationResult.Success();
                    }));
        }

        public Role AskRole()
        {
            return ConsoleWrapper.Console.Prompt(
                new SelectionPrompt<Role>()
                    .Title(Localization.Get("Ask_role"))
                    .PageSize(10)
                    .AddChoices(Role.Guide, Role.Manager));
        }

        public string AskFilePath(string titleTranslationKey)
        {
            return ConsoleWrapper.Console.Prompt(
                new SelectionPrompt<string>().Title(Localization.Get(titleTranslationKey))
                    .PageSize(10)
                    .MoreChoicesText(Localization.Get("Ask_file_more_choices"))
                    .AddChoices(Directory.GetFiles("Csv")));
        }
    }
}
