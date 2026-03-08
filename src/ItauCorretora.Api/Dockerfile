FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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
RUN dotnet publish "ItauCorretora.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

RUN mkdir -p /app/cotacoes

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ItauCorretora.Api.dll"]