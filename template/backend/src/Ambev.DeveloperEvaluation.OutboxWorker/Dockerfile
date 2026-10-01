FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Ambev.DeveloperEvaluation.OutboxWorker/Ambev.DeveloperEvaluation.OutboxWorker.csproj", "src/Ambev.DeveloperEvaluation.OutboxWorker/"]
COPY ["src/Ambev.DeveloperEvaluation.Messaging/Ambev.DeveloperEvaluation.Messaging.csproj", "src/Ambev.DeveloperEvaluation.Messaging/"]
COPY ["src/Ambev.DeveloperEvaluation.ORM/Ambev.DeveloperEvaluation.ORM.csproj", "src/Ambev.DeveloperEvaluation.ORM/"]
COPY ["src/Ambev.DeveloperEvaluation.Application/Ambev.DeveloperEvaluation.Application.csproj", "src/Ambev.DeveloperEvaluation.Application/"]
COPY ["src/Ambev.DeveloperEvaluation.Domain/Ambev.DeveloperEvaluation.Domain.csproj", "src/Ambev.DeveloperEvaluation.Domain/"]
COPY ["src/Ambev.DeveloperEvaluation.Common/Ambev.DeveloperEvaluation.Common.csproj", "src/Ambev.DeveloperEvaluation.Common/"]
RUN dotnet restore "./src/Ambev.DeveloperEvaluation.OutboxWorker/Ambev.DeveloperEvaluation.OutboxWorker.csproj"
COPY . .
WORKDIR "/src/src/Ambev.DeveloperEvaluation.OutboxWorker"
RUN dotnet build "./Ambev.DeveloperEvaluation.OutboxWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ambev.DeveloperEvaluation.OutboxWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ambev.DeveloperEvaluation.OutboxWorker.dll"]
