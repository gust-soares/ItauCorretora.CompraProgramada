using System.Diagnostics.CodeAnalysis;
using ItauCorretora.Application.UseCases;
using ItauCorretora.Domain.Interfaces;
using ItauCorretora.Infrastructure.Data;
using ItauCorretora.Infrastructure.Messaging;
using ItauCorretora.Infrastructure.Repositories;
using ItauCorretora.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ItauCorretora.Application.Queries;
using ItauCorretora.Infrastructure.Queries;

[assembly: ExcludeFromCodeCoverage]

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Iniciando a API Compra Programada Itaú...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddScoped<ICarteiraQuery, CarteiraQuery>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=compra_programada_db;Uid=root;Pwd=rootpassword;";

    builder.Services.AddDbContext<CompraProgramadaDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    builder.Services.AddHealthChecks()
        .AddMySql(connectionString, name: "banco-de-dados-mysql")
        .AddKafka(setup => setup.BootstrapServers = "localhost:9092", name: "mensageria-kafka");

    builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
    builder.Services.AddScoped<AderirProdutoUseCase>();
    builder.Services.AddScoped<IB3ParserService, B3ParserService>();
    builder.Services.AddScoped<ProcessarInvestimentoUseCase>();
    builder.Services.AddScoped<IKafkaService, KafkaProducerService>();
    builder.Services.AddScoped<ProcessarRebalanceamentoUseCase>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<CompraProgramadaDbContext>();
            context.Database.Migrate();
            Log.Information("Banco de dados sincronizado e atualizado com sucesso.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Erro fatal ao sincronizar o banco de dados.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A API falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush(); 
}