FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/Fintex.Web.Host/Fintex.Web.Host.csproj", "src/Fintex.Web.Host/"]
COPY ["src/Fintex.Web.Core/Fintex.Web.Core.csproj", "src/Fintex.Web.Core/"]
COPY ["src/Fintex.Application/Fintex.Application.csproj", "src/Fintex.Application/"]
COPY ["src/Fintex.Core/Fintex.Core.csproj", "src/Fintex.Core/"]
COPY ["src/Fintex.EntityFrameworkCore/Fintex.EntityFrameworkCore.csproj", "src/Fintex.EntityFrameworkCore/"]
WORKDIR "/src/src/Fintex.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/Fintex.Web.Host", "src/Fintex.Web.Host"]
COPY ["src/Fintex.Web.Core", "src/Fintex.Web.Core"]
COPY ["src/Fintex.Application", "src/Fintex.Application"]
COPY ["src/Fintex.Core", "src/Fintex.Core"]
COPY ["src/Fintex.EntityFrameworkCore", "src/Fintex.EntityFrameworkCore"]
WORKDIR "/src/src/Fintex.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Fintex.Web.Host.dll"]
