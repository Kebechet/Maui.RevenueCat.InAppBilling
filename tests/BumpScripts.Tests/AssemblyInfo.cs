using Xunit;

// Every test runs `dotnet run .github/scripts/bump.cs`, and concurrent runs race on the
// script's shared build output ("The application to execute does not exist").
[assembly: CollectionBehavior(DisableTestParallelization = true)]
