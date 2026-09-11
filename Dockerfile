FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY YumDash.slnx ./
COPY src/YumDash.Web/YumDash.Web.csproj src/YumDash.Web/
RUN dotnet restore YumDash.slnx

COPY . ./
RUN dotnet publish src/YumDash.Web/YumDash.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "YumDash.Web.dll"]
