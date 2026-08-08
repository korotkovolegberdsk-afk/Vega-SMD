using Microsoft.Data.Sqlite;
using Vega.QualityAnalytics.Models;

namespace Vega.QualityAnalytics.Data;

public class QualityAnalyticsRepository
{
    private readonly string _databasePath;
    public QualityAnalyticsRepository(string? databasePath = null) { _databasePath = databasePath ?? Path.Combine(AppContext.BaseDirectory, "QualityAnalytics.db"); EnsureSchema(); }

    public int AddMetric(QualityMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric); using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO QualityMetrics (ProductionLotId, MetricType, Value, Unit, Date) VALUES ($lotId, $metricType, $value, $unit, $date); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$lotId", metric.ProductionLotId); command.Parameters.AddWithValue("$metricType", metric.MetricType.ToString()); command.Parameters.AddWithValue("$value", metric.Value); command.Parameters.AddWithValue("$unit", metric.Unit); command.Parameters.AddWithValue("$date", metric.Date.ToString("O")); return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<QualityMetric> GetMetrics(int productionLotId)
    {
        using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "SELECT Id, ProductionLotId, MetricType, Value, Unit, Date FROM QualityMetrics WHERE ProductionLotId = $lotId ORDER BY Date, Id;"; command.Parameters.AddWithValue("$lotId", productionLotId); var result = new List<QualityMetric>(); using var reader = command.ExecuteReader(); while (reader.Read()) result.Add(new QualityMetric { Id = reader.GetInt32(0), ProductionLotId = reader.GetInt32(1), MetricType = Enum.Parse<QualityMetricType>(reader.GetString(2)), Value = reader.GetDouble(3), Unit = reader.GetString(4), Date = DateTime.Parse(reader.GetString(5)) }); return result;
    }

    private SqliteConnection Open() { var directory = Path.GetDirectoryName(Path.GetFullPath(_databasePath)); Directory.CreateDirectory(directory!); var connection = new SqliteConnection($"Data Source={_databasePath};Pooling=False"); connection.Open(); return connection; }
    private void EnsureSchema() { using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "CREATE TABLE IF NOT EXISTS QualityMetrics (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductionLotId INTEGER NOT NULL, MetricType TEXT NOT NULL, Value REAL NOT NULL, Unit TEXT NOT NULL, Date TEXT NOT NULL); CREATE INDEX IF NOT EXISTS IX_QualityMetrics_LotDate ON QualityMetrics(ProductionLotId, Date);"; command.ExecuteNonQuery(); }
}