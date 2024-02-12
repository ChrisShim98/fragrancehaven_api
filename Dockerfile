# Use the .NET Core SDK as a base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining source code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Use a lightweight image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app/out ./

# Expose the port the application will listen on
EXPOSE 80

# Specify the command to run when the container starts
ENTRYPOINT ["dotnet", "api.dll"]
