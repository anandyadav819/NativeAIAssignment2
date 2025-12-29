using Common.Caching;
using Common.Observability;
using FluentValidation;
using MediatR;
using Menu.Application.Commands;
using Menu.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Observability (Logging, Tracing, Health Checks) - MUST be first
builder.AddObservability("Menu.API");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Menu API", Version = "v1" });
});

// Add MediatR with application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateRestaurantCommand>());

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateRestaurantCommand>();

// Add Infrastructure (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Hybrid Caching (Memory + Redis)
builder.Services.AddHybridCache(
    builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379");

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
