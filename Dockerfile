FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SighApp.csproj", "."]
RUN dotnet restore "SighApp.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "SighApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SighApp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SighApp.dll"]
