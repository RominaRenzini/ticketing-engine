using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Infrastructure.BackgroundServices;
using TicketingEngine.Infrastructure.Publishing;
using TicketingEngine.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
});
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddSingleton<IReservationPublisher, KafkaSeatReservationPublisher>();

if (builder.Configuration.GetValue<bool>("Kafka:EnableConsumer"))
{
    builder.Services.AddHostedService<SeatReservationConsumerBackgroundService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
