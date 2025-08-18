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
RUN dotnet restore "Ecommerce.API/Ecommerce.API.csproj"
COPY . .
WORKDIR "/src/Ecommerce.API"
RUN dotnet build "Ecommerce.API.csproj" -c Release -o /app/build

# Publicação
FROM build AS publish
RUN dotnet publish "Ecommerce.API.csproj" -c Release -o /app/publish

# Stage final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Ecommerce.API/entrypoint.sh .
RUN chmod +x entrypoint.sh

# Usa o script de entrada
ENTRYPOINT ["./entrypoint.sh"]
