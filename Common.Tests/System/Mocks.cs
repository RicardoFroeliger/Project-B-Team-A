using Common.DAL;
using Common.Services;
using Common.Workflows.Guide;
using Common.Workflows.Kiosk;
using Common.Workflows.Manager;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests.System;

public class Mocks
{
    public static IServiceCollection ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<IDepotContext, DepotContext>()
            .AddSingleton<ILocalizationService, LocalizationService>()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddSingleton<IPromptService, PromptService>()
            .AddSingleton<ITicketService, TicketService>()
            .AddSingleton<ITourService, TourService>()
            .AddSingleton<IGroupService, GroupService>()
            .AddSingleton<IUserService, UserService>()
            .AddSingleton<IDataSetService, DataSetService>()
            .AddSingleton((IAnsiConsole) new TestConsole().Interactive().EmitAnsiSequences())

            // Manager flows
            .AddTransient<CreateUserFlow>()
            .AddTransient<CreateUserPlanningFlow>()
            .AddTransient<CreateTourScheduleFlow>()
            .AddTransient<ExportScheduleFlow>()
            .AddTransient<ExportTourDataFlow>()
            .AddTransient<ImportScheduleFlow>()
            .AddTransient<ImportTourDataFlow>()
            .AddTransient<PlanGuidesOnToursFlow>()

            // Kiosk flows
            .AddTransient<CancelReservationFlow>()
            .AddTransient<CreateReservationFlow>()
            .AddTransient<ModifyReservationFlow>()

            // Guide flows
            .AddTransient<RemoveTicketTourGuideFlow>()
            .AddTransient<AddTicketTourGuideFlow>()
            .AddTransient<StartTourGuideFlow>();
    }

    public static ServiceProvider BuildServices()
    {
        var services = ConfigureServices();
        return services.BuildServiceProvider();
    }
}