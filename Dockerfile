FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 5160

ENV ASPNETCORE_URLS=http://+:5160

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
ARG configuration=Release
WORKDIR /src
COPY ["restaurant-demo-website.csproj", "./"]
RUN dotnet restore "restaurant-demo-website.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "restaurant-demo-website.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "restaurant-demo-website.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "restaurant-demo-website.dll"]
