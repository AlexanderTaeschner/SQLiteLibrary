dotnet test --collect:"XPlat Code Coverage"
rem dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator "-reports:TestResults\*\*.xml" -targetdir:coveragereport
