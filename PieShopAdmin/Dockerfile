FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PieShopAdmin/PieShopAdmin.csproj", "PieShopAdmin/"]
RUN dotnet restore "PieShopAdmin/PieShopAdmin.csproj"
COPY . .
WORKDIR "/src/PieShopAdmin"
RUN dotnet build "PieShopAdmin.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PieShopAdmin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN apt-get -q update && apt-get -qy install netcat-traditional
COPY ./ShellScripts/wait-for.sh .
COPY ./ShellScripts/start.sh .
RUN chmod +x wait-for.sh
RUN chmod +x start.sh

ENTRYPOINT ["dotnet", "PieShopAdmin.dll"]
