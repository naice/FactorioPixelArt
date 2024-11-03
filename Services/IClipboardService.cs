namespace FactorioPixelArt.Services;

public interface IClipboardService
{
    Task CopyToClipboard(string text);
}
