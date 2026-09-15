using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Reflection;
using OCCTProxyLib;

namespace Vega.CAD;

/// <summary>
/// In-process STEP access. No KiCad or FreeCAD process is started.
/// OpenCASCADE is loaded as an application library and the resulting GLB
/// is the single mesh source for the preview and orthogonal projections.
/// </summary>
public sealed class OcctStepModelService
{
    private static readonly object NativeLock = new();
    private static int _initialized;
    private static nint _proxyHandle;
    private static Assembly? _proxyAssembly;

    public string ConvertStepToGlb(string stepPath, string outputDirectory)
    {
        if (!File.Exists(stepPath)) throw new FileNotFoundException("STEP model was not found.", stepPath);
        Directory.CreateDirectory(outputDirectory);
        InitializeNativeRuntime();
        var glbPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(stepPath) + ".glb");
        using var converter = new CadConverter();
        var result = converter.ConvertStepToGlb(stepPath, glbPath, MeshQuality.High, null);
        if (!result.Success || !File.Exists(glbPath))
            throw new InvalidOperationException($"STEP import failed: {result.ErrorMessage}");
        return glbPath;
    }

    private static void InitializeNativeRuntime()
    {
        if (Volatile.Read(ref _initialized) != 0) return;
        lock (NativeLock)
        {
            if (_initialized != 0) return;
            InitializeNativeRuntimeCore();
            Volatile.Write(ref _initialized, 1);
        }
    }

    private static void InitializeNativeRuntimeCore()
    {
        var nativePath = Path.Combine(AppContext.BaseDirectory, "OpenCascade", "x64");
        var appBase = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Directory.Exists(nativePath))
            throw new DirectoryNotFoundException($"OpenCASCADE runtime was not found: {nativePath}");
        if (!OperatingSystem.IsWindows() || !SetDllDirectory(appBase))
            throw new PlatformNotSupportedException("The in-process STEP backend requires Windows x64.");
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var pathEntries = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var requiredEntries = new[] { nativePath, appBase }
            .Where(entry => !pathEntries.Contains(entry, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        if (requiredEntries.Length > 0)
            Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, requiredEntries.Append(path)));

        // Load the managed/native bridge explicitly from the application
        // directory before CadConverter resolves its P/Invoke entry points.
        // This removes ambiguity when the process has multiple native DLL
        // locations on PATH.
        var proxyPath = Path.Combine(appBase, "OCCTProxy.dll");
        if (!File.Exists(proxyPath))
            throw new FileNotFoundException("OCCTProxy.dll не найден.", proxyPath);
        _proxyHandle = LoadLibrary(proxyPath);
        if (_proxyHandle == nint.Zero)
            throw new InvalidOperationException($"Не удалось загрузить OCCTProxy.dll (Win32 {Marshal.GetLastWin32Error()}).");
        _proxyAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(proxyPath);

        // CadConverter declares the bridge with P/Invoke. In a WPF host the
        // normal probing rules can still miss a native module that was loaded
        // explicitly above, so bind that import to the verified module handle.
        NativeLibrary.SetDllImportResolver(
            typeof(CadConverter).Assembly,
            static (libraryName, _, _) =>
                string.Equals(libraryName, "OCCTProxy", StringComparison.OrdinalIgnoreCase)
                    ? _proxyHandle
                    : nint.Zero);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string path);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint LoadLibrary(string path);
}
