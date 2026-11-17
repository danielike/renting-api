FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RentingApi.slnx", "."]
COPY ["src/", "src/"]
COPY ["tests/", "tests/"]

RUN dotnet restore RentingApi.slnx

RUN dotnet build RentingApi.slnx -c Release --no-restore

RUN dotnet test RentingApi.slnx -c Release --no-build --verbosity minimal

FROM build AS publish
RUN dotnet publish RentingApi.slnx -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8443

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "RentingApi.Api.dll"]
