using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FactorioPixelArt.Services;

public class PixelartBlueprintGenerator
{
    private const string BP_HEADER = @"{
    ""blueprint"": {
        ""icons"": [
            {
                ""signal"": {
                    ""name"": ""small-lamp""
                },
                ""index"": 1
            }
        ],
        ""entities"":
			";
private const string BP_FOOTER = @",
        ""item"": ""blueprint"",
        ""version"": 562949954273281
    }
}";
public record Color(
	[property: JsonPropertyName("r")] int R,
	[property: JsonPropertyName("g")] int G,
	[property: JsonPropertyName("b")] int B,
	[property: JsonPropertyName("a")] int A
);

public record Position(
	[property: JsonPropertyName("x")] double X,
	[property: JsonPropertyName("y")] double Y
);

public record Entity(
	[property: JsonPropertyName("entity_number")] int EntityNumber,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("position")] Position Position,
	[property: JsonPropertyName("color")] Color Color,
	[property: JsonPropertyName("always_on")] bool AlwaysOn
); 

public static string GeneratePixelartBlueprint(Stream imageStream)
{
	List<Entity> entities = new List<Entity>();
	using var img = Image.Load<Rgba32>(imageStream);

	if (img.Height > 128 || img.Width > 128) throw new Exception("Image too big, max 128x128 pixels.");
	
	for (int i = 0; i < img.Width; i++)
	{
		for (int j = 0; j < img.Height; j++)
		{
			var pixel = img[i, j];
			
			var position = new Position(i, j);
			var color = new Color(pixel.R, pixel.G, pixel.B, pixel.A);
			
			if (pixel.A <= 0) continue;
			
			var lamp = new Entity(entities.Count+1,
				"small-lamp", position, color, true);
			entities.Add(lamp);
		}
	}

	var blueprint = BP_HEADER +
		JsonSerializer.Serialize(entities) + BP_FOOTER;
	return EncodeFactorioBlueprint(blueprint);
}

private static string DecodeFactorioBlueprint(string blueprint)
{
	using var memstr = new MemoryStream();
	blueprint = blueprint.Substring(1);
	byte[] data = Convert.FromBase64String(blueprint);
	memstr.Write(data, 0, data.Length);
	memstr.Seek(0, SeekOrigin.Begin);
	using ZLibStream zlibstr = new System.IO.Compression.ZLibStream(memstr, CompressionMode.Decompress);
	using StreamReader reader = new StreamReader(zlibstr);
	return reader.ReadToEnd();
}
private static string EncodeFactorioBlueprint(string blueprint)
{
	using var ms = new MemoryStream();
	// This is the ZLib header to match the compression DeflateStream does
	// https://blogs.msdn.microsoft.com/bclteam/2007/05/16/system-io-compression-capabilities-kim-hamilton/
	ms.WriteByte(0x58);
	ms.WriteByte(0x85);
	UInt16 AddlerA = 1;
	UInt16 AddlerB = 0;
	using (var sw = new DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true))
	{
		var byteprint = Encoding.UTF8.GetBytes(blueprint);
		for (int i = 0; i < byteprint.Length; i++)
		{
			AddlerA = (UInt16)((AddlerA + byteprint[i]) % 65521);
			AddlerB = (UInt16)((AddlerB + AddlerA) % 65521);

			sw.WriteByte(byteprint[i]);
		}
	}
	ms.WriteByte((byte)(AddlerB >> 8));
	ms.WriteByte((byte)(AddlerB & 0xFF));
	ms.WriteByte((byte)(AddlerA >> 8));
	ms.WriteByte((byte)(AddlerA & 0xFF));
	ms.Seek(0, SeekOrigin.Begin);
	var bytes = ms.ToArray();
	return "0" + Convert.ToBase64String(bytes);
}
}