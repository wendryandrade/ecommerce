using Ecommerce.Application.Interfaces;
using Ecommerce.Infrastructure.Persistence.Context;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Início da Configuração dos Serviços ---

// 1. Adicionar serviços ao container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// 2. Adicionar a configuração do Swagger
//    AddEndpointsApiExplorer é necessário para a exploração de metadados da API.
builder.Services.AddEndpointsApiExplorer();
//    AddSwaggerGen gera a documentação JSON que o Swagger UI usa.
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Ecommerce.Application.Products.Queries.Handlers.GetAllProductsQueryHandler).Assembly));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// 3. Configurar o DbContext para usar SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Fim da Configuração dos Serviços ---

var app = builder.Build();

// --- Início da Configuração do Pipeline HTTP ---

// 6. Ativar o Swagger e o Swagger UI apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
