
if not exist bin\ (
  dotnet build >nul
)
dotnet run --no-build >nul

