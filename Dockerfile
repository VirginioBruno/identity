FROM mcr.microsoft.com/dotnet/sdk:7.0

WORKDIR /app

# Install Entity Framework Core CLI
RUN dotnet tool install --global dotnet-ef

# Add the installed tools to the PATH
ENV PATH="${PATH}:/root/.dotnet/tools"