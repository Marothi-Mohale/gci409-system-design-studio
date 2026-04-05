FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Gci409.slnx ./
COPY src/backend/Gci409.Domain/Gci409.Domain.csproj src/backend/Gci409.Domain/
COPY src/backend/Gci409.Application/Gci409.Application.csproj src/backend/Gci409.Application/
COPY src/backend/Gci409.Infrastructure/Gci409.Infrastructure.csproj src/backend/Gci409.Infrastructure/
COPY src/backend/Gci409.Worker/Gci409.Worker.csproj src/backend/Gci409.Worker/

RUN dotnet restore src/backend/Gci409.Worker/Gci409.Worker.csproj

COPY . .
RUN dotnet publish src/backend/Gci409.Worker/Gci409.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gci409.Worker.dll"]
