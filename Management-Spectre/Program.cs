using Common.Choices;
using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
using Common.Services.Interfaces;
using Common.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Management_Spectre
{
    public static class Program
    {
        public static bool Running { get; set; } = true;
        public static bool ShowMenu { get; set; } = true;
        public static User? User { get; set; }
        public static IServiceProvider ServiceProvider { get; set; }
        public static ILocalizationService Localization { get; set; }
        public static IPromptService Prompts { get; set; }

        public static void Main()
        {
            // Setup services
            ServiceProvider = new ServiceCollection()
                .AddSingleton<IDepotContext, DepotContext>()
                .AddSingleton<ILocalizationService, LocalizationService>()
                .AddSingleton<ISettingsService, SettingsService>()
                .AddSingleton<IPromptService, PromptService>()
                .AddSingleton<ITicketService, TicketService>()
                .AddSingleton<ITourService, TourService>()
                .AddSingleton<IGroupService, GroupService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IDataSetService, DataSetService>()
                .AddTransient<CreateUserFlow>()
                .AddTransient<CreateUserPlanningFlow>()
                .AddTransient<CreateTourScheduleFlow>()
                .AddTransient<ExportScheduleFlow>()
                .AddTransient<ExportTourDataFlow>()
                .AddTransient<ImportScheduleFlow>()
                .AddTransient<ImportTourDataFlow>()
                .BuildServiceProvider();

            // Get services
            Localization = ServiceProvider.GetService<ILocalizationService>()!;
            Prompts = ServiceProvider.GetService<IPromptService>()!;
            var userService = ServiceProvider.GetService<IUserService>()!;

            // Setup context
            ((DepotContext)ServiceProvider.GetService<IDepotContext>()!).LoadContext();

            // Menu loop
            while (Running)
            {
                var userpass = Prompts.AskUserpass();
                var hasAccess = userService.ValidateUserForRole(userpass, Role.Manager);
                User = userService.GetOne(userpass)!;
                AnsiConsole.Clear(); // Clear the console after the ticket has been scanned

                ShowMenu = hasAccess.Valid;

                while (ShowMenu)
                    ManagementMenu().NavigationAction();
            }

            Console.ReadLine();
        }

        public static NavigationChoice ManagementMenu()
        {
            var options = new List<NavigationChoice>()
            {
                new(Localization.Get("Management_planning"), () => { PlanningMenu().NavigationAction(); }),
                new(Localization.Get("Management_users"), () => { UsersMenu().NavigationAction(); }),
                new(Localization.Get("Management_export_import"), () => { ExportImportMenu().NavigationAction(); }),
                new(Localization.Get("Management_close"), () => { CloseMenu(); }),
                new(Localization.Get("Global_exit"), () => { Environment.Exit(1); }),
            };

            return Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User);
        }

        private static NavigationChoice ExportImportMenu()
        {
            var options = new List<NavigationChoice>() {
                new(Localization.Get("Management_Import_tour_data"), () => { ImportTourData(); }),
                new(Localization.Get("Management_export_tour_data"), () => { ExportTourData(); }),
                new(Localization.Get("Management_Import_planning"), () => { ImportPlanning(); }),
                new(Localization.Get("Management_export_planning"), () => { ExportPlanning(); }),
                new(Localization.Get("Management_close"), () => { CloseMenu(); }),
            };

            return Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User);
        }

        private static NavigationChoice PlanningMenu()
        {
            var options = new List<NavigationChoice>() {
                new(Localization.Get("Management_plan_tours_today"), () => { PlanTour(DateTime.Today); }),
                new(Localization.Get("Management_plan_tours_tomorrow"), () => { PlanTour(DateTime.Today.AddDays(1)); }),
                new(Localization.Get("Management_plan_tours_in_future"), () => { PlanTour(); }),
                new(Localization.Get("Management_view_tours"), ViewTours),
                new(Localization.Get("Management_close"), () => { CloseMenu(); }),
            };

            return Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User);
        }

        private static NavigationChoice UsersMenu()
        {
            var options = new List<NavigationChoice>() {
                new(Localization.Get("Management_user_creation"), CreateUser),
                new(Localization.Get("Management_user_schedule"), CreateSchedule),
                new(Localization.Get("Management_view_users"), ViewUsers),
                new(Localization.Get("Management_close"), () => { CloseMenu(); }),
            };

            return Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User);
        }

        private static void ImportTourData()
        {
            var flow = ServiceProvider.GetService<ImportTourDataFlow>()!;

            var setUserResult = flow.SetUser(User);
            if (!setUserResult.Succeeded)
            {
                CloseMenu(setUserResult.Message, false);
                return;
            }

            if (Directory.GetFiles("Csv").Count() <= 0)
            {
                CloseMenu(Localization.Get("Import_tour_data_flow_no_files_to_import"), false);
                return;
            }

            var setFilePathResult = flow.SetFilePath(Prompts.AskFilePath("Import_tour_data_flow_file_path"));
            if (!setFilePathResult.Succeeded)
            {
                CloseMenu(setFilePathResult.Message, false);
                return;
            }

            flow.CreatePreview();

            var previewTable = new Table();
            foreach (var column in flow.Preview[0])
                previewTable.AddColumn(column);

            foreach (var row in flow.Preview.Skip(1))
                previewTable.AddRow(row);


            var tableHeader = new Rule(Localization.Get("Import_tour_data_flow_preview_header"));
            tableHeader.Justification = Justify.Left;
            AnsiConsole.Write(tableHeader);
            AnsiConsole.Write(previewTable);

            // Commit the flow.
            if (Prompts.AskConfirmation("Import_tour_data_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message, false);
                return;
            }

            flow.Rollback();
            CloseMenu(closeMenu: false);
        }

        private static void ExportTourData()
        {
            var flow = ServiceProvider.GetService<ExportTourDataFlow>()!;

            var setUserResult = flow.SetUser(User);
            if (!setUserResult.Succeeded)
            {
                CloseMenu(setUserResult.Message, false);
                return;
            }

            var start = Prompts.AskDate("Export_tour_data_flow_start_date", "Export_tour_data_flow_more_dates", historical: true, dateRange: 60);
            var end = Prompts.AskDate("Export_tour_data_flow_end_date", "Export_tour_data_flow_more_dates", historical: true, dateRange: 60, startDate: start);

            var setDateSpanResult = flow.SetDateSpan(start, end);
            if (!setDateSpanResult.Succeeded)
            {
                CloseMenu(setDateSpanResult.Message, false);
                return;
            }

            flow.CreatePreview();

            // Commit the flow.
            if (Prompts.AskConfirmation("Export_tour_data_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message, false);
                return;
            }

            flow.Rollback();
            CloseMenu(closeMenu: false);
        }

        private static void ImportPlanning()
        {

        }

        private static void ExportPlanning()
        {

        }

        private static void CreateSchedule()
        {
            var userService = ServiceProvider.GetService<IUserService>()!;
            var flow = ServiceProvider.GetService<CreateUserPlanningFlow>()!;

            var user = Prompts.AskUser(userService.GetUsersOfRole(Role.Guide));

            var setUserResult = flow.SetUser(user);
            if (!setUserResult.Succeeded)
            {
                CloseMenu(setUserResult.Message, false);
                return;
            }

            var weekdays = Prompts.AskSchedule();


            if (Prompts.AskConfirmation("Create_user_planning_flow_ask_repeat_hours"))
            {
                var startTime = Prompts.AskTime("Create_user_planning_flow_start_time", "Create_user_planning_flow_more_times");
                var endTime = Prompts.AskTime("Create_user_planning_flow_end_time", "Create_user_planning_flow_more_times", startTime: (int)startTime.TotalMinutes);

                foreach (var day in weekdays)
                {
                    AnsiConsole.MarkupLine(Localization.Get("Create_user_planning_flow_day", replacementStrings: new() { day.ToString() }));
                    AnsiConsole.MarkupLine(Localization.Get("Create_user_planning_flow_hours", replacementStrings: new() { startTime.ToString("hh\\:mm"), endTime.ToString("hh\\:mm") }));
                    flow.SetPlanningDay(day, startTime, endTime);
                }
            }
            else
            {
                foreach (var day in weekdays)
                {
                    AnsiConsole.MarkupLine(Localization.Get("Create_user_planning_flow_day", replacementStrings: new() { day.ToString() }));
                    var startTime = Prompts.AskTime("Create_user_planning_flow_start_time", "Create_user_planning_flow_more_times");
                    var endTime = Prompts.AskTime("Create_user_planning_flow_end_time", "Create_user_planning_flow_more_times", startTime: (int)startTime.TotalMinutes);
                    AnsiConsole.MarkupLine(Localization.Get("Create_user_planning_flow_hours", replacementStrings: new() { startTime.ToString("hh\\:mm"), endTime.ToString("hh\\:mm") }));

                    flow.SetPlanningDay(day, startTime, endTime);
                }
            }


            // Commit the flow.
            if (Prompts.AskConfirmation("Create_user_planning_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message, false);
                return;
            }

            flow.Rollback();
            CloseMenu(closeMenu: false);
        }

        private static void ViewUsers()
        {
            var userService = ServiceProvider.GetService<IUserService>()!;

            var currentUsers = userService.GetAll();

            var currentPlanningTable = new Table();
            currentPlanningTable.AddColumn(Localization.Get("View_user_id_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_user_role_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_user_name_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_user_enabled_column"));

            foreach (var user in currentUsers)
            {
                var id = $"[grey]{user.Id}[/]";
                var role = $"[blue]{(Role)user.Role}[/]";
                var name = $"[green]{user.Name}[/]";
                var enabled = user.Enabled ? "[green]enabled[/]" : "[red]disabled[/]";

                currentPlanningTable.AddRow(id, role, name, enabled);
            }

            var currentUsersHeader = new Rule(Localization.Get("View_user_current_users"));
            currentUsersHeader.Justification = Justify.Left;
            AnsiConsole.Write(currentUsersHeader);
            AnsiConsole.Write(currentPlanningTable);

            AnsiConsole.WriteLine(Localization.Get("View_user_press_any_key_to_continue"));

            Console.ReadKey();

            CloseMenu(closeMenu: false);
            return;
        }

        private static void CreateUser()
        {
            var flow = ServiceProvider.GetService<CreateUserFlow>()!;

            flow.SetUsername(Prompts.AskUsername());
            flow.SetRole(Prompts.AskRole());

            // Commit the flow.
            if (Prompts.AskConfirmation("Create_user_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message, false);
                return;
            }

            flow.Rollback();
            CloseMenu(closeMenu: false);
        }

        private static void ViewTours()
        {
            var tourService = ServiceProvider.GetService<ITourService>()!;

            var start = Prompts.AskDate("View_tour_start_date", "View_tour_more_dates");
            var end = Prompts.AskDate("Create_tour_flow_end_date", "Create_tour_flow_more_dates", startDate: start);

            var currentPlanning = tourService.GetToursForTimespan(start, end);

            var currentPlanningTable = new Table();
            currentPlanningTable.AddColumn(Localization.Get("View_tour_date_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_tour_time_column"));

            foreach (var (date, tours) in currentPlanning)
                currentPlanningTable.AddRow($"[green]{date.ToString("dd/MM/yyyy")}[/]", string.Join(", ", tours.Select(tour => $"[blue]{tour.Start.ToString("HH\\:mm")}[/]")));

            var currentPlanningHeader = new Rule(Localization.Get("View_tour_current_planning"));
            currentPlanningHeader.Justification = Justify.Left;
            AnsiConsole.Write(currentPlanningHeader);
            AnsiConsole.Write(currentPlanningTable);

            AnsiConsole.WriteLine(Localization.Get("View_tour_press_any_key_to_continue"));

            Console.ReadKey();

            CloseMenu(closeMenu: false);
            return;
        }

        private static void PlanTour(DateTime? start = null, DateTime? end = null)
        {
            var tourService = ServiceProvider.GetService<ITourService>()!;
            var flow = ServiceProvider.GetService<CreateTourScheduleFlow>()!;

            if (!flow.SetDateSpan(start, end).Success)
            {
                start = Prompts.AskDate("Create_tour_flow_start_date", "Create_tour_flow_more_dates");
                end = Prompts.AskDate("Create_tour_flow_end_date", "Create_tour_flow_more_dates", startDate: start);

                flow.SetDateSpan(start, end);
            }

            var startTime = Prompts.AskTime("Create_tour_flow_start_time", "Create_tour_flow_more_times");
            var endTime = Prompts.AskTime("Create_tour_flow_end_time", "Create_tour_flow_more_times", startTime: (int)startTime.TotalMinutes);

            flow.SetTimeSpan(startTime, endTime);

            var interval = Prompts.AskNumber("Create_tour_flow_interval", "Create_tour_flow_interval_invalid", 1, 60);

            flow.SetInterval(interval);

            var previewChanges = flow.GetPreviewChanges();

            var newPlanning = new Table();
            newPlanning.AddColumn(Localization.Get("Create_tour_flow_date_column"));
            newPlanning.AddColumn(Localization.Get("Create_tour_flow_time_column"));

            foreach (var (date, times) in previewChanges)
                newPlanning.AddRow($"[green]{date.ToString("dd/MM/yyyy")}[/]", string.Join(", ", times.Select(time => $"[blue]{time.ToString("hh\\:mm")}[/]")));

            var newPlanningHeader = new Rule(Localization.Get("Create_tour_flow_new_planning"));
            newPlanningHeader.Justification = Justify.Left;
            AnsiConsole.Write(newPlanningHeader);
            AnsiConsole.Write(newPlanning);

            var currentPlanning = tourService.GetToursForTimespan(flow.StartDate, flow.EndDate);

            var oldPlanning = new Table();
            oldPlanning.AddColumn(Localization.Get("Create_tour_flow_date_column"));
            oldPlanning.AddColumn(Localization.Get("Create_tour_flow_time_column"));

            foreach (var (date, tours) in currentPlanning)
                oldPlanning.AddRow($"[green]{date.ToString("dd/MM/yyyy")}[/]", string.Join(", ", tours.Select(tour => $"[blue]{tour.Start.ToString("HH\\:mm")}[/]")));

            var oldPlanningHeader = new Rule(Localization.Get("Create_tour_flow_old_planning"));
            oldPlanningHeader.Justification = Justify.Left;
            AnsiConsole.Write(oldPlanningHeader);
            AnsiConsole.Write(oldPlanning);

            if (currentPlanning.Any() && Prompts.AskConfirmation("Create_tour_flow_overwrite_current_confirmation"))
                flow.DisposePlanning(currentPlanning);

            // Commit the flow.
            if (Prompts.AskConfirmation("Create_tour_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message, false);
                return;
            }

            flow.Rollback();
            CloseMenu(closeMenu: false);
        }

        public static NavigationChoice TourMenu(Tour tour)
        {
            var options = new List<NavigationChoice>() {
                new(Localization.Get("Management_close"), () => { CloseMenu(); }),
            };

            return Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User);
        }


        private static void CloseMenu(string? message = null, bool closeMenu = true)
        {
            if (message != null)
                AnsiConsole.MarkupLine(message);

            AnsiConsole.MarkupLine(Localization.Get("Management_close_message"));
            Thread.Sleep(2000);

            AnsiConsole.Clear();
            ShowMenu = !closeMenu;
            return;
        }
    }
}
