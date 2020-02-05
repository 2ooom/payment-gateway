#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["PaymentGateway.Tests/PaymentGateway.Tests.csproj", "PaymentGateway.Tests/"]
COPY ["PaymentGateway.Model/PaymentGateway.Model.csproj", "PaymentGateway.Model/"]
COPY ["PaymentGateway/PaymentGateway.csproj", "PaymentGateway/"]
RUN dotnet restore "PaymentGateway.Tests/PaymentGateway.Tests.csproj"
COPY . .
WORKDIR "/src/PaymentGateway.Tests"
RUN dotnet build "PaymentGateway.Tests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentGateway.Tests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentGateway.Tests.dll"]