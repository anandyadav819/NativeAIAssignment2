using Common.Caching;
using Common.Observability;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Serilog;
using Tracking.Application.Commands;
using Tracking.SignalR.Hubs;
using Tracking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Observability (Logging, Tracing, Health Checks) - MUST be first
builder.AddObservability("Tracking.API");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Tracking API", Version = "v1" });
});

// Add MediatR with application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UpdateDriverLocationCommand>());

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UpdateDriverLocationCommand>();

// Add SignalR
builder.Services.AddSignalR();

// Add Infrastructure (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Hybrid Caching (Memory + Redis)
builder.Services.AddHybridCache(
    builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379");

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:3000" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("SignalRPolicy");

app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<TrackingHub>("/hubs/tracking");

app.Run();
