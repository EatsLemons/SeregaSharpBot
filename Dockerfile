FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview8-alpine3.9 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -r alpine.3.9-x64 -o out /p:PublishTrimmed=true

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-preview8-alpine3.9 as runtime
WORKDIR /app
COPY --from=build-env /app/out ./
RUN ls
ENTRYPOINT ["dotnet", "SeregaBot.Application.dll"]