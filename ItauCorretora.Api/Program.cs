using ItauCorretora.Application.UseCases;
using ItauCorretora.Domain.Interfaces;
using ItauCorretora.Infrastructure.Data;
using ItauCorretora.Infrastructure.Messaging;
using ItauCorretora.Infrastructure.Repositories;
using ItauCorretora.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CompraProgramadaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<AderirProdutoUseCase>();
builder.Services.AddScoped<IB3ParserService, B3ParserService>();
builder.Services.AddScoped<ProcessarInvestimentoUseCase>();
builder.Services.AddScoped<IKafkaService, KafkaProducerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CompraProgramadaDbContext>();
        context.Database.Migrate();
        Console.WriteLine(">>> BANCO DE DADOS SINCRONIZADO COM SUCESSO! <<<");
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> ERRO AO SINCRONIZAR BANCO: {ex.Message} <<<");
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

app.Run();