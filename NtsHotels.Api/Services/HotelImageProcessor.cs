using ImageMagick;

namespace NtsHotels.Api.Services;

public class HotelImageProcessor
{
    public const long MaxFileBytes = 10 * 1024 * 1024;
    private const long MaxPixels = 25_000_000;

    public async Task<byte[]> CompressAsync(IFormFile file, long maxPixels = MaxPixels)
    {
        if (file.Length <= 0 || file.Length > MaxFileBytes)
            throw new InvalidImageException("La imagen debe tener un tamaño máximo de 10 MB.");

        try
        {
            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var bytes = stream.ToArray();
            var jpeg = bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff;
            var png = bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            var webp = bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8);
            if (!jpeg && !png && !webp)
                throw new InvalidImageException("Solo se admiten imágenes JPEG, PNG y WebP.");

            var info = new MagickImageInfo(bytes);
            if (info.Width == 0 || info.Height == 0 || (long)info.Width * info.Height > maxPixels)
                throw new InvalidImageException($"La imagen supera el límite de {maxPixels / 1_000_000} megapíxeles.");
            using var image = new MagickImage(bytes);
            image.AutoOrient();
            if (image.Width > 800)
                image.Resize(800, 0);
            image.BackgroundColor = MagickColors.White;
            image.Alpha(AlphaAction.Remove);
            image.Strip();
            image.Quality = 80;
            return image.ToByteArray(MagickFormat.Jpeg);
        }
        catch (MagickException)
        {
            throw new InvalidImageException("El archivo contiene una imagen dañada.");
        }
    }
}

public class InvalidImageException(string message) : Exception(message);
