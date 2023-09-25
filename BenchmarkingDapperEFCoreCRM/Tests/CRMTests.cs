using Microsoft.Data.SqlClient;
using BenchmarkDotNet.Attributes;
using Bogus.DataSets;
using Bogus.Extensions.Brazil;
using Dapper.Contrib.Extensions;
using BenchmarkingDapperEFCoreCRM.EFCore;

namespace BenchmarkingDapperEFCoreCRM.Tests;

[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, launchCount: 5)]
public class CRMTests
{
    private const int NumeroContatosPorCompanhia = 1;

    private int GetNumeroContatosPorCompanhia()
    {
        var envNumeroContatosPorCompanhia =
            Environment.GetEnvironmentVariable("NumeroContatosPorCompanhia");
        if (!String.IsNullOrWhiteSpace(envNumeroContatosPorCompanhia) &&
            int.TryParse(envNumeroContatosPorCompanhia, out int result))
            return result;
        return NumeroContatosPorCompanhia;
    }

    #region EFCore Tests

    private CRMContext? _context;
    private Name? _namesDataSetEF;
    private PhoneNumbers? _phonesDataSetEF;
    private Address? _addressesDataSetEF;
    private Company? _companiesDataSetEF;
    private int _numeroContatosPorCompanhiaEF;

    [IterationSetup(Target = nameof(InputDataWithEntityFrameworkCore))]
    public void SetupEntityFrameworkCore()
    {
        _context = new CRMContext();
        _namesDataSetEF = new Name("pt_BR");
        _phonesDataSetEF = new PhoneNumbers("pt_BR");
        _addressesDataSetEF = new Address("pt_BR");
        _companiesDataSetEF = new Company("pt_BR");
        _numeroContatosPorCompanhiaEF = GetNumeroContatosPorCompanhia();
    }

    [Benchmark]
    public EFCore.Empresa InputDataWithEntityFrameworkCore()
    {
        var empresa = new EFCore.Empresa()
        {
            Nome = _companiesDataSetEF!.CompanyName(),
            CNPJ = _companiesDataSetEF!.Cnpj(includeFormatSymbols: false),
            Cidade = _addressesDataSetEF!.City(),
            Contatos = new ()
        };
        for (int i = 0; i < _numeroContatosPorCompanhiaEF; i++)
        {
            empresa.Contatos.Add(new ()
            {
                Nome = _namesDataSetEF!.FullName(),
                Telefone = _phonesDataSetEF!.PhoneNumber()
            });
        }

        _context!.Add(empresa);
        _context!.SaveChanges();

        return empresa;        
    }

    [IterationCleanup(Target = nameof(InputDataWithEntityFrameworkCore))]
    public void CleanupEntityFrameworkCore()
    {
        _context = null;
    }

    #endregion

    #region Dapper Tests

    private SqlConnection? _connection;
    private Name? _namesDataSetDapper;
    private PhoneNumbers? _phonesDataSetDapper;
    private Address? _addressesDataSetDapper;
    private Company? _companiesDataSetDapper;
    private int _numeroContatosPorCompanhiaDapper;

    [IterationSetup(Target = nameof(InputDataWithDapper))]
    public void SetupDapper()
    {
        _connection = new SqlConnection(Configurations.BaseDapper);
        _namesDataSetDapper = new Name("pt_BR");
        _phonesDataSetDapper = new PhoneNumbers("pt_BR");
        _addressesDataSetDapper = new Address("pt_BR");
        _companiesDataSetDapper = new Company("pt_BR");
        _numeroContatosPorCompanhiaDapper = GetNumeroContatosPorCompanhia();
    }

    [Benchmark]
    public Dapper.Empresa InputDataWithDapper()
    {
        var empresa = new Dapper.Empresa()
        {
            Nome = _companiesDataSetDapper!.CompanyName(),
            CNPJ = _companiesDataSetDapper!.Cnpj(includeFormatSymbols: false),
            Cidade = _addressesDataSetDapper!.City()
        };
        
        _connection!.Open();
        var transaction = _connection.BeginTransaction();

        _connection.Insert<Dapper.Empresa>(empresa, transaction);

        empresa.Contatos = new();
        for (int i = 0; i < _numeroContatosPorCompanhiaDapper; i++)
        {
            var contato = new Dapper.Contato()
            {
                IdEmpresa = empresa.IdEmpresa,
                Nome = _namesDataSetDapper!.FullName(),
                Telefone = _phonesDataSetDapper!.PhoneNumber()
            };
            _connection.Insert<Dapper.Contato>(contato, transaction);
            empresa.Contatos.Add(contato);
        }

        transaction.Commit();
        _connection.Close();
        
        return empresa;
    }

    [IterationCleanup(Target = nameof(InputDataWithDapper))]
    public void CleanupDapper()
    {
        _connection = null;
    }

    #endregion
}