dotnet test --collect:"XPlat Code Coverage"
reportgenerator "-reports:TestResults\*\*.xml" -targetdir:coveragereport
