using Common.Choices;
using Common.Enums;
using Common.Workflows.Manager;
using Spectre.Console;

namespace Common.Clients
{
    public class ManagerClient : ConsoleClient
    {
        public ManagerClient(IServiceProvider serviceProvider) : base(serviceProvider, ClientType.Manager)
        {
        }

        protected override Action ShowMainMenu()
        {
            var options = new List<NamedChoice<Action>>()
            {
                new(Localization.Get("Management_planning"), () => { PlanningMenu(); }),
                new(Localization.Get("Management_users"), () => { UsersMenu(); }),
                new(Localization.Get("Management_export_import"), () => { ExportImportMenu(); }),
                new(Localization.Get("Management_close"), () => { CloseMenu(closeSubMenu: true, logout: true); }),
            };

            if (Contained)
                options.Add(new(Localization.Get("Global_exit"), () => { CloseMenu(closeSubMenu: true, logout: true, closeClient: true); }));

            return Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User);
        }

        private void ExportImportMenu()
        {
            ShowSubmenu = true;

            do
            {
                var options = new List<NamedChoice<Action>>() {
                    new(Localization.Get("Management_Import_tour_data"), ImportTourData),
                    new(Localization.Get("Management_export_tour_data"), ExportTourData),
                    new(Localization.Get("Global_return"), () => { CloseMenu(closeSubMenu: true); }),
                };

                Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User).Invoke();
            } while (ShowSubmenu);
        }

        private void PlanningMenu()
        {
            ShowSubmenu = true;

            do
            {
                var options = new List<NamedChoice<Action>>() {
                    new(Localization.Get("Management_plan_tours_today"), () => { PlanTour(DateTime.Today); }),
                    new(Localization.Get("Management_plan_tours_tomorrow"), () => { PlanTour(DateTime.Today.AddDays(1)); }),
                    new(Localization.Get("Management_plan_tours_in_future"), () => { PlanTour(); }),
                    new(Localization.Get("Management_plan_single_tour", "Enkele rondleiding plannen"), () => { PlanSingleTour(); }),
                    new(Localization.Get("Management_view_tours"), ViewTours),
                    new(Localization.Get("Management_plan_guides_on_tours"), PlanGuidesOnTours),
                    new(Localization.Get("Global_return"), () => { CloseMenu(closeSubMenu: true); }),
                };

                Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User).Invoke();
            } while (ShowSubmenu);
        }

        private void UsersMenu()
        {
            ShowSubmenu = true;

            do
            {
                var options = new List<NamedChoice<Action>>() {
                    new(Localization.Get("Management_user_creation"), CreateUser),
                    new(Localization.Get("Management_user_schedule"), CreateSchedule),
                    new(Localization.Get("Management_view_users"), ViewUsers),
                    new(Localization.Get("Global_return"), () => { CloseMenu(closeSubMenu:true); }),
                };

                Prompts.GetMenu("Management_title", "Management_menu_more_options", options, User).Invoke();
            } while (ShowSubmenu);
        }

        private void PlanGuidesOnTours()
        {
            var flow = GetFlow<PlanGuidesOnToursFlow>();

            var start = Prompts.AskDate("Plan_guides_on_tours_flow_start_date", "Plan_guides_on_tours_flow_more_dates");
            var end = Prompts.AskDate("Plan_guides_on_tours_flow_end_date", "Plan_guides_on_tours_flow_more_dates", startDate: start);

            var setDateSpanResult = flow.SetDateSpan(start, end);
            if (!setDateSpanResult.Succeeded)
            {
                CloseMenu(setDateSpanResult.Message);
                return;
            }

            flow.CreatePreview();

            var previewTable = new Table();
            previewTable.AddColumn(Localization.Get("Plan_guides_on_tours_flow_tour_column"));
            previewTable.AddColumn(Localization.Get("Plan_guides_on_tours_flow_guide_column"));

            foreach (var row in flow.Preview)
                previewTable.AddRow(row.Key.Start.ToString("dd-MM-yyyy HH:mm"), row.Value != null ? $"{row.Value?.Id} {row.Value?.Name}" : "_________");

            var tableHeader = new Rule(Localization.Get("Plan_guides_on_tours_flow_preview_header"));
            tableHeader.Justification = Justify.Left;
            Console.Write(tableHeader);
            Console.Write(previewTable);

            // Commit the flow.
            if (Prompts.AskConfirmation("Plan_guides_on_tours_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void ImportTourData()
        {
            var flow = GetFlow<ImportTourDataFlow>();

            var setUserResult = flow.SetUser(User);
            if (!setUserResult.Succeeded)
            {
                CloseMenu(setUserResult.Message);
                return;
            }

            if (Directory.GetFiles("Csv").Count() <= 0)
            {
                CloseMenu(Localization.Get("Import_tour_data_flow_no_files_to_import"));
                return;
            }

            var setFilePathResult = flow.SetFilePath(Prompts.AskFilePath("Import_tour_data_flow_file_path"));
            if (!setFilePathResult.Succeeded)
            {
                CloseMenu(setFilePathResult.Message);
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
            Console.Write(tableHeader);
            Console.Write(previewTable);

            // Commit the flow.
            if (Prompts.AskConfirmation("Import_tour_data_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void ExportTourData()
        {
            var flow = GetFlow<ExportTourDataFlow>();

            var setUserResult = flow.SetUser(User);
            if (!setUserResult.Succeeded)
            {
                CloseMenu(setUserResult.Message);
                return;
            }

            var start = Prompts.AskDate("Export_tour_data_flow_start_date", "Export_tour_data_flow_more_dates", historical: true, dateRange: 60);
            var end = Prompts.AskDate("Export_tour_data_flow_end_date", "Export_tour_data_flow_more_dates", historical: true, dateRange: 60, startDate: start);

            var setDateSpanResult = flow.SetDateSpan(start, end);
            if (!setDateSpanResult.Succeeded)
            {
                CloseMenu(setDateSpanResult.Message);
                return;
            }

            flow.CreatePreview();

            // Commit the flow.
            if (Prompts.AskConfirmation("Export_tour_data_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void CreateSchedule()
        {
            var flow = GetFlow<CreateUserPlanningFlow>()!;

            var user = Prompts.AskUser(UserService.GetUsersOfRole(RoleType.Guide));

            var setUserResult = flow.SetUser(user);
            if (!setUserResult.Succeeded)
            {
                CloseMenu(setUserResult.Message);
                return;
            }

            var weekdays = Prompts.AskSchedule();


            if (Prompts.AskConfirmation("Create_user_planning_flow_ask_repeat_hours"))
            {
                var startTime = Prompts.AskTime("Create_user_planning_flow_start_time", "Create_user_planning_flow_more_times");
                var endTime = Prompts.AskTime("Create_user_planning_flow_end_time", "Create_user_planning_flow_more_times", startTime: (int)startTime.TotalMinutes);

                foreach (var day in weekdays)
                {
                    Console.MarkupLine(Localization.Get("Create_user_planning_flow_day", replacementStrings: new() { day.ToString() }));
                    Console.MarkupLine(Localization.Get("Create_user_planning_flow_hours", replacementStrings: new() { startTime.ToString("hh\\:mm"), endTime.ToString("hh\\:mm") }));
                    flow.SetPlanningDay(day, startTime, endTime);
                }
            }
            else
            {
                foreach (var day in weekdays)
                {
                    Console.MarkupLine(Localization.Get("Create_user_planning_flow_day", replacementStrings: new() { day.ToString() }));
                    var startTime = Prompts.AskTime("Create_user_planning_flow_start_time", "Create_user_planning_flow_more_times");
                    var endTime = Prompts.AskTime("Create_user_planning_flow_end_time", "Create_user_planning_flow_more_times", startTime: (int)startTime.TotalMinutes);
                    Console.MarkupLine(Localization.Get("Create_user_planning_flow_hours", replacementStrings: new() { startTime.ToString("hh\\:mm"), endTime.ToString("hh\\:mm") }));

                    flow.SetPlanningDay(day, startTime, endTime);
                }
            }


            // Commit the flow.
            if (Prompts.AskConfirmation("Create_user_planning_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void ViewUsers()
        {
            var currentUsers = UserService.GetAll();

            var currentPlanningTable = new Table();
            currentPlanningTable.AddColumn(Localization.Get("View_user_id_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_user_role_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_user_name_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_user_enabled_column"));

            foreach (var user in currentUsers)
            {
                var id = $"[grey]{user.Id}[/]";
                var role = $"[blue]{(RoleType)user.Role}[/]";
                var name = $"[green]{user.Name}[/]";
                var enabled = user.Enabled ? "[green]enabled[/]" : "[red]disabled[/]";

                currentPlanningTable.AddRow(id, role, name, enabled);
            }

            var currentUsersHeader = new Rule(Localization.Get("View_user_current_users"));
            currentUsersHeader.Justification = Justify.Left;
            Console.Write(currentUsersHeader);
            Console.Write(currentPlanningTable);

            Console.WriteLine(Localization.Get("View_user_press_any_key_to_continue"));

            Console.Input.ReadKey(false);
            CloseMenu();
        }

        private void CreateUser()
        {
            var flow = GetFlow<CreateUserFlow>();

            flow.SetUsername(Prompts.AskUsername());
            flow.SetRole(Prompts.AskRole());

            // Commit the flow.
            if (Prompts.AskConfirmation("Create_user_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void ViewTours()
        {
            var start = Prompts.AskDate("View_tour_start_date", "View_tour_more_dates");
            var end = Prompts.AskDate("Create_tour_flow_end_date", "Create_tour_flow_more_dates", startDate: start);

            var currentPlanning = TourService.GetToursForTimespan(start, end);

            var currentPlanningTable = new Table();
            currentPlanningTable.AddColumn(Localization.Get("View_tour_date_column"));
            currentPlanningTable.AddColumn(Localization.Get("View_tour_time_column"));

            foreach (var (date, tours) in currentPlanning)
                currentPlanningTable.AddRow($"[green]{date.ToString("dd/MM/yyyy")}[/]", string.Join(", ", tours.Select(tour => $"[blue]{tour.Start.ToString("HH\\:mm")}[/]")));

            var currentPlanningHeader = new Rule(Localization.Get("View_tour_current_planning"));
            currentPlanningHeader.Justification = Justify.Left;
            Console.Write(currentPlanningHeader);
            Console.Write(currentPlanningTable);

            Console.WriteLine(Localization.Get("View_tour_press_any_key_to_continue"));

            Console.Input.ReadKey(false);
            CloseMenu();
        }

        private void PlanSingleTour()
        {
            var flow = GetFlow<CreateSingleTourScheduleFlow>();

            var date = Prompts.AskDate("Create_tour_flow_date", "Create_tour_flow_more_dates");

            flow.SetDate(date);

            var time = Prompts.AskTime("Create_tour_flow_time", "Create_tour_flow_more_times");

            flow.SetTime(time);

            Console.MarkupLine(Localization.Get("Create_tour_flow_tour_to_create", replacementStrings: new List<string>() { }));

            // Commit the flow.
            if (Prompts.AskConfirmation("Create_tour_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void PlanTour(DateTime? start = null, DateTime? end = null)
        {
            var flow = GetFlow<CreateTourScheduleFlow>();

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
            Console.Write(newPlanningHeader);
            Console.Write(newPlanning);

            var currentPlanning = TourService.GetToursForTimespan(flow.StartDate, flow.EndDate);

            var oldPlanning = new Table();
            oldPlanning.AddColumn(Localization.Get("Create_tour_flow_date_column"));
            oldPlanning.AddColumn(Localization.Get("Create_tour_flow_time_column"));

            foreach (var (date, tours) in currentPlanning)
                oldPlanning.AddRow($"[green]{date.ToString("dd/MM/yyyy")}[/]", string.Join(", ", tours.Select(tour => $"[blue]{tour.Start.ToString("HH\\:mm")}[/]")));

            var oldPlanningHeader = new Rule(Localization.Get("Create_tour_flow_old_planning"));
            oldPlanningHeader.Justification = Justify.Left;
            Console.Write(oldPlanningHeader);
            Console.Write(oldPlanning);

            if (currentPlanning.Any() && Prompts.AskConfirmation("Create_tour_flow_overwrite_current_confirmation"))
                flow.DisposePlanning(currentPlanning);

            // Commit the flow.
            if (Prompts.AskConfirmation("Create_tour_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }
    }
}
