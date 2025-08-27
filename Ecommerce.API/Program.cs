using Ecommerce.API.Consumers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Infrastructure.Auth;
using Ecommerce.Infrastructure.Persistence.Context;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Ecommerce.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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

// Mapear variáveis de ambiente (.env) para chaves de configuração quando presentes
void MapEnv(string key, string envName)
{
    var v = Environment.GetEnvironmentVariable(envName);
    if (!string.IsNullOrWhiteSpace(v)) builder.Configuration[key] = v;
}

// JWT
MapEnv("Jwt:Key", "JWT_KEY");
MapEnv("Jwt:Issuer", "JWT_ISSUER");
MapEnv("Jwt:Audience", "JWT_AUDIENCE");

// Stripe
MapEnv("Stripe:SecretKey", "STRIPE_SECRETKEY");
MapEnv("Stripe:PublishableKey", "STRIPE_PUBLISHABLEKEY");

// Melhor Envio
MapEnv("MelhorEnvio:ApiUrl", "API_URL");
MapEnv("MelhorEnvio:ApiToken", "API_TOKEN");
MapEnv("MelhorEnvio:DefaultOriginZipCode", "DEFAULT_ORIGIN_ZIPCODE");
MapEnv("MelhorEnvio:Services", "SERVICES");
MapEnv("MelhorEnvio:UseMockData", "USE_MOCK_DATA");
MapEnv("MelhorEnvio:DefaultPackageWidth", "DEFAULT_PACKAGE_WIDTH");
MapEnv("MelhorEnvio:DefaultPackageHeight", "DEFAULT_PACKAGE_HEIGHT");
MapEnv("MelhorEnvio:DefaultPackageLength", "DEFAULT_PACKAGE_LENGTH");
MapEnv("MelhorEnvio:DefaultPackageWeight", "DEFAULT_PACKAGE_WEIGHT");
MapEnv("MelhorEnvio:DefaultInsuranceValue", "DEFAULT_INSURANCE_VALUE");

// Connection string (montar a partir de variáveis, se não estiver configurada)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPass = Environment.GetEnvironmentVariable("DB_PASS");
    if (!string.IsNullOrWhiteSpace(dbHost) && !string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPass))
    {
        var port = string.IsNullOrWhiteSpace(dbPort) ? "1433" : dbPort;
        builder.Configuration[$"ConnectionStrings:DefaultConnection"] = $"Server={dbHost},{port};Database={dbName};User Id={dbUser};Password={dbPass};TrustServerCertificate=True;MultipleActiveResultSets=True;";
    }
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
        Version = "v1",
        Description = "API de E-commerce com autenticação JWT"
    });

    // Configurar autenticação JWT no Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --- MASSTRANSIT ---
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Ecommerce.API.Consumers.OrderConsumer).Assembly);

    if (builder.Environment.IsEnvironment("Testing") || builder.Environment.IsDevelopment())
    {
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var rmqUser = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
            var rmqPass = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";

            cfg.Host("rabbitmq", "/", h =>
            {
                h.Username(rmqUser);
                h.Password(rmqPass);
            });

            cfg.ConfigureEndpoints(context);
        });
    }
});

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
builder.Services.AddScoped<IShippingService, MelhorEnvioShippingService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configurar Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("Chave JWT não configurada");
        }

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var certificatePath = builder.Configuration["Kestrel:Certificates:Default:Path"];
var httpsAvailable = !string.IsNullOrWhiteSpace(certificatePath) && File.Exists(certificatePath);

if (httpsAvailable)
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443; // porta HTTPS do container
    });
}

var app = builder.Build();

if (!builder.Environment.IsEnvironment("Testing"))
{
    app.ApplyMigrations();
    await app.SeedDatabaseAsync();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing") || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (httpsAvailable)
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program 
{ 
    protected Program() { }
}