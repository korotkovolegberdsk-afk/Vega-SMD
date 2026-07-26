using Vega.Data.Migration;
using Vega.Data.SQLite;
using Vega.Data.Repositories;
using Vega.Models.Packages;

namespace Vega.Services.Library;

public class PackageService
{
    private readonly PackageRepository _repository;


    public PackageService()
    {
        var database = new SMTDatabase();

        database.Initialize();


        var seeder = new PackageSeeder();

        seeder.AddDefaultPackages();


        var cleanup = new DatabaseCleanup();

        cleanup.RemovePackageDuplicates();


        _repository = new PackageRepository();
    }



    public List<PackageSearchResult> GetPackages()
    {
        return _repository.GetAll();
    }
}