using System.Globalization;
using Vega.Mirtec;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class MirtecMmdFormatTests
{
    private static string Sample => Path.Combine(AppContext.BaseDirectory, "TestData", "Mirtec", "ATMEGA_3000_14_V3.mmd");

    [Fact]
    public void ReadRealMmdTemplate()
    {
        var template = new MirtecMmdTemplateReader().Read(Sample);

        Assert.Equal("0", template.Fid1X);
        Assert.Equal("0", template.Fid1Y);
        Assert.Equal("0", template.Fid2X);
        Assert.Equal("0", template.Fid2Y);
        Assert.Equal("NO", template.CoordinateTransform);
        Assert.Equal(68, template.Records.Count);

        AssertRecord(template.Records[0], 1, "16487", "51462", "90", "CA-IF1051HS", "U3", "SOP08");
        AssertRecord(template.Records[17], 18, "24663", "20199", "270", "ATmega168A-AU", "U5", "QFP32_08_9x9");
        AssertRecord(template.Records[67], 68, "18404", "8536", "180", "UNKNOWN", "_R31", null);
    }

    [Fact]
    public void RoundTripPreservesRealMmdSemantics()
    {
        var reader = new MirtecMmdTemplateReader();
        var template = reader.Read(Sample);
        var project = new MirtecMmdProject { ProjectName = "ATMEGA_3000_14_V3" };
        foreach (var record in template.Records)
        {
            project.Parts.Add(new MirtecMmdPart
            {
                RefDes = record.RefDes,
                PartNumber = record.PartNumber,
                X = double.Parse(record.X, CultureInfo.InvariantCulture),
                Y = double.Parse(record.Y, CultureInfo.InvariantCulture),
                Rotation = double.Parse(record.Rotation, CultureInfo.InvariantCulture),
                PackageName = record.PackageName ?? string.Empty
            });
        }

        var path = Path.Combine(Path.GetTempPath(), $"GENERATED_ATMEGA_3000_14_V3_{Guid.NewGuid():N}.mmd");
        var reportPath = Path.Combine(Path.GetDirectoryName(path)!, "PROJECT_MMD_REPORT.txt");
        try
        {
            new MirtecMmdWriter().Write(project, path, template);
            var output = reader.Read(path);
            var lines = File.ReadAllLines(path);
            var recordLines = lines.Where(line => line.StartsWith('#')).ToArray();

            Assert.Contains("[Fiducial]", lines);
            Assert.Contains("[Part Info]", lines);
            Assert.Contains("Coordinate Transform=NO", lines);
            Assert.Contains("Part Count=68", lines);
            Assert.Equal(68, output.Records.Count);
            Assert.Equal(68, recordLines.Length);
            Assert.Equal("0", output.Fid1X);
            Assert.Equal("0", output.Fid1Y);
            Assert.Equal("0", output.Fid2X);
            Assert.Equal("0", output.Fid2Y);
            Assert.Equal("NO", output.CoordinateTransform);

            for (var index = 0; index < recordLines.Length; index++)
            {
                Assert.StartsWith($"#{index + 1:00000000}=", recordLines[index]);
                Assert.Equal(6, recordLines[index].Split('\t').Length);
            }

            AssertRecord(output.Records[0], 1, "16487", "51462", "90", "CA-IF1051HS", "U3", "SOP08");
            AssertRecord(output.Records[17], 18, "24663", "20199", "270", "ATmega168A-AU", "U5", "QFP32_08_9x9");
            AssertRecord(output.Records[67], 68, "18404", "8536", "180", "UNKNOWN", "_R31", null);
            Assert.DoesNotContain(lines, line => line.Contains("DatabaseNo", StringComparison.Ordinal));
            Assert.DoesNotContain(lines, line => line.Contains("LibraryUse", StringComparison.Ordinal));
            Assert.DoesNotContain(lines, line => line.Contains("LibraryPath", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(path);
            File.Delete(reportPath);
        }
    }

    private static void AssertRecord(MirtecMmdRecord record, int index, string x, string y, string rotation, string partNumber, string refDes, string? packageName)
    {
        Assert.Equal(index, record.Index);
        Assert.Equal(x, record.X);
        Assert.Equal(y, record.Y);
        Assert.Equal(rotation, record.Rotation);
        Assert.Equal(partNumber, record.PartNumber);
        Assert.Equal(refDes, record.RefDes);
        Assert.Equal(packageName, record.PackageName);
    }
}