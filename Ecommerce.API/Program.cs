using Ecommerce.API.Consumers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Infrastructure;
using Ecommerce.Infrastructure.Auth;
using Ecommerce.Infrastructure.Persistence.Context;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Ecommerce.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ecommerce.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Carregar configuração extra se for Docker
if (builder.Environment.EnvironmentName == "Docker")
{
    builder.Configuration.AddJsonFile("appsettings.Docker.json", optional: false, reloadOnChange: true);
}

// --- Início da Configuração dos Serviços ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define as informações básicas da API para o Swagger
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ecommerce API",
        Version = "v1"
    });
});

// --- MASSTRANSIT ---
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(OrderConsumer).Assembly);

    if (builder.Environment.IsEnvironment("Testing"))
    {
        // Usa broker em memória nos testes
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        // Usa RabbitMQ normalmente
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("rabbitmq", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ConfigureEndpoints(context);
        });
    }
});

// --- FIM DA CORREÇÃO ---

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Ecommerce.Application.Features.Orders.Handlers.CreateOrderCommandHandler).Assembly));

// Injeção de Dependências
builder.Services.AddHttpClient();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IShippingService, CorreiosShippingService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();


// Configurar Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Configurar o DbContext
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

if (!builder.Environment.IsEnvironment("Testing"))
{
    // Garante que as migrations sejam aplicadas na inicialização com retentativas
    app.ApplyMigrations();
}

// --- Início da Configuração do Pipeline HTTP ---

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing") || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { } // Necessário para os testes de integração