using System.Buffers.Binary;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Vega.CAD;

public sealed record GlbTriangle(Vector3 A, Vector3 B, Vector3 C, Vector4 Color);

/// <summary>Reads the mesh produced by the in-process STEP backend.</summary>
public sealed class GlbMeshReader
{
    public IReadOnlyList<GlbTriangle> ReadTriangles(string glbPath)
    {
        using var stream = File.OpenRead(glbPath);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
        if (reader.ReadUInt32() != 0x46546C67 || reader.ReadUInt32() != 2)
            throw new InvalidDataException("The file is not a GLB 2.0 model.");
        var length = reader.ReadUInt32();
        if (length != stream.Length) throw new InvalidDataException("The GLB length is invalid.");
        var json = Array.Empty<byte>();
        var binary = Array.Empty<byte>();
        while (stream.Position < stream.Length)
        {
            var chunkLength = reader.ReadUInt32();
            var chunkType = reader.ReadUInt32();
            var bytes = reader.ReadBytes(checked((int)chunkLength));
            if (chunkType == 0x4E4F534A) json = bytes;
            else if (chunkType == 0x004E4942) binary = bytes;
        }
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var bufferViews = root.GetProperty("bufferViews").EnumerateArray().ToArray();
        var accessors = root.GetProperty("accessors").EnumerateArray().ToArray();
        var materials = root.TryGetProperty("materials", out var materialArray) ? materialArray.EnumerateArray().ToArray() : [];
        var triangles = new List<GlbTriangle>();
        foreach (var mesh in root.GetProperty("meshes").EnumerateArray())
        foreach (var primitive in mesh.GetProperty("primitives").EnumerateArray())
        {
            var attributes = primitive.GetProperty("attributes");
            var positions = ReadVec3(accessors[attributes.GetProperty("POSITION").GetInt32()], bufferViews, binary);
            var indices = primitive.TryGetProperty("indices", out var indexProperty)
                ? ReadIndices(accessors[indexProperty.GetInt32()], bufferViews, binary)
                : Enumerable.Range(0, positions.Length).ToArray();
            var color = ReadColor(primitive, materials);
            for (var i = 0; i + 2 < indices.Length; i += 3)
                triangles.Add(new GlbTriangle(positions[indices[i]], positions[indices[i + 1]], positions[indices[i + 2]], color));
        }
        if (triangles.Count == 0) throw new InvalidDataException("The GLB contains no triangles.");
        return triangles;
    }

    private static Vector3[] ReadVec3(JsonElement accessor, JsonElement[] views, byte[] data)
    {
        if (accessor.GetProperty("componentType").GetInt32() != 5126 || accessor.GetProperty("type").GetString() != "VEC3")
            throw new InvalidDataException("Only float VEC3 positions are supported.");
        var view = views[accessor.GetProperty("bufferView").GetInt32()];
        var offset = view.TryGetProperty("byteOffset", out var viewOffset) ? viewOffset.GetInt32() : 0;
        offset += accessor.TryGetProperty("byteOffset", out var accessorOffset) ? accessorOffset.GetInt32() : 0;
        var stride = view.TryGetProperty("byteStride", out var byteStride) ? byteStride.GetInt32() : 12;
        var result = new Vector3[accessor.GetProperty("count").GetInt32()];
        for (var i = 0; i < result.Length; i++)
        {
            var p = offset + i * stride;
            result[i] = new Vector3(ReadFloat(data, p), ReadFloat(data, p + 4), ReadFloat(data, p + 8));
        }
        return result;
    }

    private static int[] ReadIndices(JsonElement accessor, JsonElement[] views, byte[] data)
    {
        var componentType = accessor.GetProperty("componentType").GetInt32();
        var view = views[accessor.GetProperty("bufferView").GetInt32()];
        var offset = (view.TryGetProperty("byteOffset", out var viewOffset) ? viewOffset.GetInt32() : 0) + (accessor.TryGetProperty("byteOffset", out var accessorOffset) ? accessorOffset.GetInt32() : 0);
        var count = accessor.GetProperty("count").GetInt32();
        var size = componentType == 5121 ? 1 : componentType == 5123 ? 2 : componentType == 5125 ? 4 : 0;
        if (size == 0) throw new InvalidDataException("Unsupported GLB index type.");
        var result = new int[count];
        for (var i = 0; i < count; i++) result[i] = size switch { 1 => data[offset + i], 2 => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset + i * 2, 2)), _ => checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + i * 4, 4))) };
        return result;
    }

    private static Vector4 ReadColor(JsonElement primitive, JsonElement[] materials)
    {
        if (!primitive.TryGetProperty("material", out var materialIndex) || materials.Length == 0) return Vector4.One;
        var material = materials[materialIndex.GetInt32()];
        if (!material.TryGetProperty("pbrMetallicRoughness", out var pbr) || !pbr.TryGetProperty("baseColorFactor", out var factor)) return Vector4.One;
        var values = factor.EnumerateArray().Select(x => x.GetSingle()).ToArray();
        return new Vector4(values.ElementAtOrDefault(0), values.ElementAtOrDefault(1), values.ElementAtOrDefault(2), values.ElementAtOrDefault(3) == 0 ? 1 : values[3]);
    }

    private static float ReadFloat(byte[] data, int offset) => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset, 4)));
}
