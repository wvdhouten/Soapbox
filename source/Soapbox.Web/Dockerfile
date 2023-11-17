#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Soapbox.Web/Soapbox.Web.csproj", "Soapbox.Web/"]
RUN dotnet restore "Soapbox.Web/Soapbox.Web.csproj"
COPY . .
WORKDIR "/src/Soapbox.Web"
RUN dotnet build "Soapbox.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Soapbox.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Soapbox.Web.dll"]