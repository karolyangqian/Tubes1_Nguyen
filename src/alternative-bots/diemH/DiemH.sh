#!/bin/sh
# This script runs the bot in development mode by default.
# Development Mode (default): Always cleans, rebuilds, and runs.
# Release Mode (commented out): Runs without rebuilding.

# Development mode: always rebuild
rm -rf bin obj
dotnet build
dotnet run --no-build

# Uncomment below for release mode (runs without rebuilding)
# if [ -d "bin" ]; then
#   dotnet run --no-build
# else
#   dotnet build
#   dotnet run --no-build
# fi
