
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ItauCorretora.Api/ItauCorretora.Api.csproj", "src/ItauCorretora.Api/"]
COPY ["src/ItauCorretora.Application/ItauCorretora.Application.csproj", "src/ItauCorretora.Application/"]
COPY ["src/ItauCorretora.Domain/ItauCorretora.Domain.csproj", "src/ItauCorretora.Domain/"]
COPY ["src/ItauCorretora.Infrastructure/ItauCorretora.Infrastructure.csproj", "src/ItauCorretora.Infrastructure/"]

RUN dotnet restore "src/ItauCorretora.Api/ItauCorretora.Api.csproj"

COPY . .
WORKDIR "/src/src/ItauCorretora.Api"
RUN dotnet build "ItauCorretora.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ItauCorretora.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN mkdir -p /app/cotacoes

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ItauCorretora.Api.dll"]# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ItauCorretora.Api/ItauCorretora.Api.csproj", "src/ItauCorretora.Api/"]
COPY ["src/ItauCorretora.Application/ItauCorretora.Application.csproj", "src/ItauCorretora.Application/"]
COPY ["src/ItauCorretora.Domain/ItauCorretora.Domain.csproj", "src/ItauCorretora.Domain/"]
COPY ["src/ItauCorretora.Infrastructure/ItauCorretora.Infrastructure.csproj", "src/ItauCorretora.Infrastructure/"]

RUN dotnet restore "src/ItauCorretora.Api/ItauCorretora.Api.csproj"

COPY . .
WORKDIR "/src/src/ItauCorretora.Api"
RUN dotnet build "ItauCorretora.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ItauCorretora.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN mkdir -p /app/cotacoes

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ItauCorretora.Api.dll"]