#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["PaymentGateway.ITests/PaymentGateway.ITests.csproj", "PaymentGateway.ITests/"]
COPY ["PaymentGateway.Tests/PaymentGateway.Tests.csproj", "PaymentGateway.Tests/"]
COPY ["PaymentGateway.Model/PaymentGateway.Model.csproj", "PaymentGateway.Model/"]
COPY ["PaymentGateway.Tests/PaymentGateway.Tests.csproj", "PaymentGateway.Tests/"]
COPY ["PaymentGateway/PaymentGateway.csproj", "PaymentGateway/"]
COPY ["PaymentGateway.Client/PaymentGateway.Client.csproj", "PaymentGateway.Client/"]
RUN dotnet restore "PaymentGateway/PaymentGateway.csproj"
RUN dotnet restore "PaymentGateway.Tests/PaymentGateway.Tests.csproj"
RUN dotnet restore "PaymentGateway.ITests/PaymentGateway.ITests.csproj"
COPY . .

# Unit tests
RUN dotnet test --verbosity n PaymentGateway.Tests

# Integration tests
RUN dotnet test --verbosity n PaymentGateway.ITests

# Build
WORKDIR "/src/PaymentGateway"
RUN dotnet build "PaymentGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentGateway.dll"]