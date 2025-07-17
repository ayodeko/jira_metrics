using Akka.Actor;
using Akka.Hosting;
using MetricsServer.Akka.Actors;
using MetricsServer.Akka.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(corsBuilder => corsBuilder.AddDefaultPolicy(
    policyBuilder => policyBuilder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
    ));
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MetricsServer.Services.JiraMetricsProviderCache>();

// --- Akka.NET Configuration ---
builder.Services.AddAkka("MetricsActorSystem", (akkaBuilder, provider) =>
{
    akkaBuilder.WithActors((system, registry, resolver) =>
    {
        // Create and register the coordinator actor
        var coordinatorProps = Props.Create(() => new MetricsCoordinatorActor(provider));
        var coordinatorActor = system.ActorOf(coordinatorProps, "metrics-coordinator");
        registry.Register<MetricsCoordinatorActor>(coordinatorActor);
    });
});
builder.Services.AddSingleton<AkkaMetricsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.UseCors();
app.Run();
