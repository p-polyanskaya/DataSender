FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR .
COPY ["EndPoint/EndPoint.csproj", "EndPoint/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["GrpcServices/GrpcServices.csproj", "GrpcServices/"]
RUN dotnet restore "EndPoint/EndPoint.csproj"
COPY . .
RUN dotnet build "EndPoint/EndPoint.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EndPoint/EndPoint.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EndPoint.dll"]
