using System.Diagnostics;
using System.Text;

namespace Vega.CAD;

public sealed record KiCadProjectionSet(string TopPath, string BottomPath, string SidePath, string IsometricPath, string ModelPath);

public sealed class KiCadModelRenderService
{
    private readonly string _kicadCli;
    private readonly string _kicadViewer;

    public KiCadModelRenderService(string? kicadCli = null)
    {
        _kicadCli = kicadCli ?? @"C:\Program Files\KiCad\10.0\bin\kicad-cli.exe";
        _kicadViewer = Path.Combine(Path.GetDirectoryName(_kicadCli) ?? string.Empty, "pcbnew.exe");
    }

    public static string? ResolveModelPath(string? packageName, string? configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            var candidates = new[] { configuredPath, Path.Combine(AppContext.BaseDirectory, configuredPath) };
            var configured = candidates.FirstOrDefault(File.Exists);
            if (configured is not null) return configured;
        }
        var key = (packageName ?? string.Empty).Trim().ToUpperInvariant();
        var model = key switch
        {
            "C01005" => @"Capacitor_SMD.3dshapes\C_01005_0402Metric.step",
            "C0201" => @"Capacitor_SMD.3dshapes\C_0201_0603Metric.step",
            "C0402" or "0402" => @"Capacitor_SMD.3dshapes\C_0402_1005Metric.step",
            "C0603" => @"Capacitor_SMD.3dshapes\C_0603_1608Metric.step",
            "R0201" => @"Resistor_SMD.3dshapes\R_0201_0603Metric.step",
            "R0402" => @"Resistor_SMD.3dshapes\R_0402_1005Metric.step",
            "R1206" => @"Resistor_SMD.3dshapes\R_1206_3216Metric.step",
            "L0201" => @"Inductor_SMD.3dshapes\L_0201_0603Metric.step",
            "L0603" => @"Inductor_SMD.3dshapes\L_0603_1608Metric.step",
            _ => null
        };
        if (model is null) return null;
        var path = Path.Combine(@"C:\Program Files\KiCad\10.0\share\kicad\3dmodels", model);
        return File.Exists(path) ? path : null;
    }

    public async Task<KiCadProjectionSet> RenderAsync(string modelPath, string outputDirectory, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_kicadCli)) throw new FileNotFoundException("KiCad CLI was not found.", _kicadCli);
        if (!File.Exists(modelPath)) throw new FileNotFoundException("3D model was not found.", modelPath);
        Directory.CreateDirectory(outputDirectory);
        var normalizedModel = modelPath.Replace('\\', '/');
        var boardPath = Path.Combine(outputDirectory, "model-render.kicad_pcb");
        await File.WriteAllTextAsync(boardPath, CreateBoard(normalizedModel), new UTF8Encoding(false), cancellationToken);
        var top = Path.Combine(outputDirectory, "top.png");
        var bottom = Path.Combine(outputDirectory, "bottom.png");
        var side = Path.Combine(outputDirectory, "side.png");
        var iso = Path.Combine(outputDirectory, "isometric.png");
        await RenderOneAsync(boardPath, top, "top", null, cancellationToken);
        await RenderOneAsync(boardPath, bottom, "bottom", null, cancellationToken);
        await RenderOneAsync(boardPath, side, "left", null, cancellationToken);
        await RenderOneAsync(boardPath, iso, null, "-35,0,20", cancellationToken);
        return new KiCadProjectionSet(top, bottom, side, iso, modelPath);
    }

    public void OpenInteractiveViewer(string boardPath)
    {
        if (!File.Exists(_kicadViewer)) throw new FileNotFoundException("KiCad PCB Editor was not found.", _kicadViewer);
        Process.Start(new ProcessStartInfo { FileName = _kicadViewer, Arguments = $"\"{boardPath}\"", UseShellExecute = true });
    }

    private async Task RenderOneAsync(string boardPath, string outputPath, string? side, string? rotation, CancellationToken cancellationToken)
    {
        var args = new List<string> { "pcb", "render", "--output", outputPath, "--width", "512", "--height", "512", "--background", "transparent", "--quality", "basic", "--zoom", "0.25" };
        if (side is not null) { args.Add("--side"); args.Add(side); }
        if (rotation is not null) { args.Add("--rotate"); args.Add(rotation); }
        args.Add(boardPath);
        var psi = new ProcessStartInfo { FileName = _kicadCli, UseShellExecute = false, CreateNoWindow = true };
        foreach (var arg in args) psi.ArgumentList.Add(arg);
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start KiCad CLI.");
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0 || !File.Exists(outputPath)) throw new InvalidOperationException($"KiCad render failed for {Path.GetFileName(outputPath)}.");
    }

    private static string CreateBoard(string modelPath) => $"""
(kicad_pcb (version 20240108) (generator pcbnew)
  (general (thickness 0))
  (paper "A4")
  (layers (0 "F.Cu" signal) (31 "B.Cu" signal) (36 "B.SilkS" user "b.silkscreen") (37 "F.SilkS" user "f.silkscreen") (44 "Edge.Cuts" user))
  (setup (pad_to_mask_clearance 0))
  (footprint "Vega:KiCadModel" (layer "F.Cu") (at 0 0) (attr smd)
    (model "{modelPath}" (offset (xyz 0 0 0)) (scale (xyz 1 1 1)) (rotate (xyz 0 0 0)))
  )
)
""";
}
