$env:NumeroContatosPorCompanhia = "1"
$env:BaseEFCoreConnectionString= "Server=localhost;Database=BaseCRMEF;User Id=sa;Password=SqlServer2019!;TrustServerCertificate=True;"
$env:BaseDapperConnectionString= "Server=localhost;Database=BaseCRMDapper;User Id=sa;Password=SqlServer2019!;TrustServerCertificate=True;"
dotnet run --filter * -c Release