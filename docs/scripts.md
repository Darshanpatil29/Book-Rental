# Create solution
dotnet new sln -n ECommerce.Microservice

# Create layer projects
dotnet new classlib -n OrderApi.Application -f net8.0
dotnet new classlib -n OrderApi.Domain -f net8.0
dotnet new classlib -n OrderApi.Infrastructure -f net8.0
dotnet new webapi -n OrderApi.Presentation -f net8.0

# Create test projects
dotnet new xunit -n OrderApi.Tests -f net8.0

# Create shared library
dotnet new classlib -n eCommerce.SharedLibrary -f net8.0

# Add projects to solution
dotnet sln add OrderApi.Application/OrderApi.Application.csproj
dotnet sln add OrderApi.Domain/OrderApi.Domain.csproj
dotnet sln add OrderApi.Infrastructure/OrderApi.Infrastructure.csproj
dotnet sln add OrderApi.Presentation/OrderApi.Presentation.csproj
dotnet sln add OrderApi.Tests/OrderApi.Tests.csproj
dotnet sln add eCommerce.SharedLibrary/eCommerce.SharedLibrary.csproj