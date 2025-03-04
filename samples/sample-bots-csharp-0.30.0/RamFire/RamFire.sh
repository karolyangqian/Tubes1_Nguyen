#!/bin/sh
if [ -d "bin" ]; then
  dotnet build
fi
dotnet run --no-build

