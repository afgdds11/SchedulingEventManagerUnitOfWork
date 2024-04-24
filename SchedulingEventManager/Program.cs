using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulingEventManager.Contract;
using SchedulingEventManager.Infrastructure;
using SchedulingEventManager.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ScheduleEventContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ScheduleEventDB")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("demo", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithHeaders("Context-Type");
    });
});
builder.Services.AddScoped<IScheduleEventRepository, ScheduleEventRepository>();

#region Main
var app = builder.Build();

// Middleware
app.UseCors("demo");
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Creación de eventos de ejemplo
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<ScheduleEventContext>();
    context.Database.EnsureCreated();
    if (!context.ScheduleEvents.Any())
    {
        var scheduleEventsToAdd = new List<ScheduleEvent>
        {
            new ScheduleEvent
            {
                Description = "Hacer la compra",
                IsCompleted = false,
                DueDate = DateTime.Now.AddDays(1)
            },
            new ScheduleEvent
            {
                Description = "Estudiar para el examen",
                IsCompleted = false,
                DueDate = DateTime.Now.AddDays(5)
            },
            new ScheduleEvent
            {
                Description = "Llamar al médico",
                IsCompleted = false,
                DueDate = DateTime.Now.AddDays(13)
            }
        };
        context.ScheduleEvents.AddRange(scheduleEventsToAdd);
        context.SaveChanges();
    }
}

// Definición de los endpoints
var scheduleEventsEndPoint = app.MapGroup("/scheduleevent");

#region EndPoints
// Endpoints
scheduleEventsEndPoint.MapGet("/", GetAllScheduleEvents)
    .WithName("GetAllScheduleEvents")
    .WithMetadata(new
    {
        Summary = "Obtiene todos los eventos",
        Tags = new[] { "schedule", "event", "todos", "eventos" }
    });

scheduleEventsEndPoint.MapGet("/{days}", GetAllScheduleEventsByCustomRange)
    .WithName("GetScheduleEventsInCustomRange")
    .WithMetadata(new
    {
        Description = "Obtiene los eventos cuya fecha de vencimiento está dentro del rango de días especificado, comenzando desde la fecha actual.",
        Summary = "Obtiene los eventos próximos en un rango personalizado",
        Tags = new[] { "schedule", "event", "próximos", "eventos" }
    });

scheduleEventsEndPoint.MapPost("/", CreateScheduleEvent)
    .WithName("CreateScheduleEvent")
    .WithMetadata(new
    {
        Description = "Crea un nuevo evento de programación.",
        Tags = new[] { "schedule", "event", "crear", "nuevo" }
    });

scheduleEventsEndPoint.MapPut("/", UpdateScheduleEvent)
    .WithName("UpdateScheduleEvent")
    .WithMetadata(new
    {
        Description = "Modifica un evento existente.",
        Tags = new[] { "schedule", "event", "modificar", "actualizar" }
    });

scheduleEventsEndPoint.MapDelete("/{id}", DeleteScheduleEvent)
    .WithName("DeleteScheduleEventById")
    .WithMetadata(new
    {
        Description = "Elimina un evento por su ID."
    });
#endregion

app.Run();

#endregion

#region Funciones
// Funciones
async Task<IResult> GetAllScheduleEvents(IScheduleEventRepository repository)
{
    var schedEvents = await repository.FindAll();
    return Results.Ok(schedEvents);
}

async Task<IResult> GetAllScheduleEventsByCustomRange(int days, IScheduleEventRepository repository)
{
    var startDate = DateTime.Now;
    var endDate = startDate.AddDays(days);
    var schedEvents = await repository.FindByCondition(se => se.DueDate >= startDate && se.DueDate <= endDate);
    return Results.Ok(schedEvents);
}

async Task<IResult> CreateScheduleEvent([FromBody] ScheduleEvent body, IScheduleEventRepository repository)
{
    await repository.Create(body);
    return Results.Created($"/scheduleevent/{body.Id}", body);
}

async Task<IResult> UpdateScheduleEvent([FromBody] ScheduleEvent body, IScheduleEventRepository repository)
{
    string description = body.Description;
    bool isCompleted = body.IsCompleted;
    DateTime dueDate = body.DueDate;

    var existingEvents = await repository.FindByCondition(e => e.Id == body.Id);
    var existingEvent = existingEvents.FirstOrDefault();

    if (existingEvent == null)
    {
        return Results.NotFound();
    }

    existingEvent.Description = description;
    existingEvent.IsCompleted = isCompleted;
    existingEvent.DueDate = dueDate;

    await repository.Update(existingEvent);
    return Results.Ok(existingEvent);
}

async Task<IResult> DeleteScheduleEvent(int id, IScheduleEventRepository repository)
{
    var deleted = await repository.Delete(id);
    if (deleted)
    {
        return Results.Ok();
    }
    else
    {
        return Results.NotFound();
    }
}
#endregion