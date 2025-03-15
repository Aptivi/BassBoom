FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /BB

# Initialize the project files
COPY . ./

# Attempt to build BassBoom
RUN dotnet build "BassBoom.sln" -p:Configuration=Release

# Run the ASP.NET image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /BB

# Copy the output files and start BassBoom
COPY --from=build-env /BB/private/BassBoom.Cli/bin/Release/net8.0 .
ENTRYPOINT ["dotnet", "BassBoom.Cli.dll"]
