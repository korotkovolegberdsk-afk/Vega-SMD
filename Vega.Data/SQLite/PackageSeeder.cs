using Microsoft.Data.Sqlite;

namespace Vega.Data.SQLite;

public class PackageSeeder
{
    private readonly string _connectionString;


    public PackageSeeder()
    {
        _connectionString = "Data Source=SMT.db";
    }



    public void AddDefaultPackages()
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        AddPackage(connection,
            "0402",
            "Chip 0402 (1005 Metric)",
            1.0,
            0.5,
            0.35,
            "IPC-7351",
            "Проверка мелких чип-компонентов");


        AddPackage(connection,
            "0603",
            "Chip 0603 (1608 Metric)",
            1.6,
            0.8,
            0.45,
            "IPC-7351",
            "Проверка корпуса и качества пайки");


        AddPackage(connection,
            "0805",
            "Chip 0805 (2012 Metric)",
            2.0,
            1.25,
            0.55,
            "IPC-7351",
            "Контроль смещения и объема припоя");


        AddPackage(connection,
            "1206",
            "Chip 1206 (3216 Metric)",
            3.2,
            1.6,
            0.55,
            "IPC-7351",
            "Контроль пайки больших чипов");


        AddPackage(connection,
            "SOT23",
            "SOT23 Transistor Package",
            3.0,
            1.75,
            1.3,
            "JEDEC TO-236",
            "Контроль выводов и полярности");


        AddPackage(connection,
            "SO8",
            "SOIC-8 IC Package",
            4.9,
            3.9,
            1.75,
            "JEDEC MS-012",
            "Проверка выводов и качества пайки");


        AddPackage(connection,
            "QFN32",
            "QFN 32 Lead Package",
            5.0,
            5.0,
            0.9,
            "IPC-7351",
            "Контроль центральной площадки и выводов");
    }



    private void AddPackage(
        SqliteConnection connection,
        string packageName,
        string displayName,
        double length,
        double width,
        double height,
        string ipc,
        string aoi)
    {

        var command = connection.CreateCommand();


        command.CommandText =
        """
        INSERT OR IGNORE INTO Packages
        (
            PackageName,
            DisplayName,
            Length,
            Width,
            Height,
            IPCName,
            AOIRecommendations
        )
        VALUES
        (
            $name,
            $display,
            $length,
            $width,
            $height,
            $ipc,
            $aoi
        );
        """;


        command.Parameters.AddWithValue("$name", packageName);
        command.Parameters.AddWithValue("$display", displayName);
        command.Parameters.AddWithValue("$length", length);
        command.Parameters.AddWithValue("$width", width);
        command.Parameters.AddWithValue("$height", height);
        command.Parameters.AddWithValue("$ipc", ipc);
        command.Parameters.AddWithValue("$aoi", aoi);


        command.ExecuteNonQuery();
    }
}