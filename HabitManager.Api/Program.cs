using HabitManager.Application;
using HabitManager.Contracts;
using HabitManager.Domain.Entities;
using HabitManager.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IHabitRepository, InMemoryHabitRepository>();
builder.Services.AddSingleton<HabitService>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
var habits = app.MapGroup("/api/habits");
habits.AddEndpointFilter(async (context, next) =>
{
    try
    {
        return await next(context);
    }
    catch (HabitNotFoundException error)
    {
        return Results.Problem(detail: error.Message, statusCode: 404);
    }
    catch (ArgumentException error)
    {
        return Results.Problem(detail: error.Message, statusCode: 400);
    }
    catch (HabitRuleException error)
    {
        return Results.Problem(detail: error.Message, statusCode: 409);
    }
});

habits.MapGet("/", (HabitService service) => service.ListAsync(DemoUser.Id));
habits.MapGet("/{id:guid}", (Guid id, HabitService service) => service.GetAsync(DemoUser.Id, id));
habits.MapPost("/", async (CreateHabitRequest request, HabitService service) =>
{
    var habit = await service.CreateAsync(DemoUser.Id, request);
    return Results.Created($"/api/habits/{habit.Id}", habit);
});
habits.MapPut("/{id:guid}", (Guid id, UpdateHabitRequest request, HabitService service) =>
    service.UpdateAsync(DemoUser.Id, id, request));
habits.MapPut("/{id:guid}/archive", (Guid id, HabitService service) =>
    service.ArchiveAsync(DemoUser.Id, id));
habits.MapDelete("/{id:guid}/archive", (Guid id, HabitService service) =>
    service.RestoreAsync(DemoUser.Id, id));
habits.MapGet("/{id:guid}/completions", (Guid id, HabitService service) =>
    service.GetCompletionsAsync(DemoUser.Id, id));
habits.MapPut("/{id:guid}/completions/{date}", (Guid id, DateOnly date, HabitService service) =>
    service.CompleteAsync(DemoUser.Id, id, date));
habits.MapDelete("/{id:guid}/completions/{date}", (Guid id, DateOnly date, HabitService service) =>
    service.UndoCompletionAsync(DemoUser.Id, id, date));

app.Run();
