FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BasketAPI.API/BasketAPI.API.csproj", "BasketAPI.API/"]
COPY ["BasketAPI.Application/BasketAPI.Application.csproj", "BasketAPI.Application/"]
COPY ["BasketAPI.Domain/BasketAPI.Domain.csproj", "BasketAPI.Domain/"]
COPY ["BasketAPI.Infrastructure/BasketAPI.Infrastructure.csproj", "BasketAPI.Infrastructure/"]
RUN dotnet restore "BasketAPI.API/BasketAPI.API.csproj"
COPY . .
WORKDIR "/src/BasketAPI.API"
RUN dotnet build "BasketAPI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasketAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasketAPI.API.dll"]
