# Imagem base com runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Imagem para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ecommerce.API/Ecommerce.API.csproj", "Ecommerce.API/"]
COPY ["Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj", "Ecommerce.Infrastructure/"]
COPY ["Ecommerce.Application/Ecommerce.Application.csproj", "Ecommerce.Application/"]
COPY ["Ecommerce.Domain/Ecommerce.Domain.csproj", "Ecommerce.Domain/"]
RUN dotnet restore "Ecommerce.API/Ecommerce.API.csproj"
# Copia apenas o código necessário para o build 
COPY Ecommerce.API/ ./Ecommerce.API/
COPY Ecommerce.Application/ ./Ecommerce.Application/
COPY Ecommerce.Infrastructure/ ./Ecommerce.Infrastructure/
COPY Ecommerce.Domain/ ./Ecommerce.Domain/
WORKDIR "/src/Ecommerce.API"
RUN dotnet build "Ecommerce.API.csproj" -c Release -o /app/build

# Publica��o
FROM build AS publish
RUN dotnet publish "Ecommerce.API.csproj" -c Release -o /app/publish

# Stage final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Ecommerce.API/entrypoint.sh .
RUN chmod +x entrypoint.sh

# Cria usuário não-root e ajusta permissões
RUN useradd -m -u 10001 appuser && chown -R appuser:appuser /app
USER appuser

# Usa o script de entrada
ENTRYPOINT ["./entrypoint.sh"]
