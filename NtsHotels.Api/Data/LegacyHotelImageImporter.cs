using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using NtsHotels.Api.Services;

namespace NtsHotels.Api.Data;

public static class LegacyHotelImageImporter
{
    public static async Task ImportAsync(ApplicationDbContext db, HotelImageProcessor processor)
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            var connection = db.Database.GetDbConnection();
            var pending = new List<(string Id, string Url)>();
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT "Id", "ImageUrl" FROM "Hotels"
                    WHERE "ImageData" IS NULL AND "ImageUrl" IS NOT NULL AND trim("ImageUrl") <> ''
                    """;
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    pending.Add((reader.GetString(0), reader.GetString(1)));
            }

            if (pending.Count == 0) return;
            using var client = CreateClient();
            await using var transaction = await connection.BeginTransactionAsync();
            foreach (var (id, url) in pending)
            {
                try
                {
                    var bytes = await DownloadAsync(client, url);
                    await using var stream = new MemoryStream(bytes);
                    var file = new FormFile(stream, 0, bytes.Length, "image", "legacy");
                    var compressed = await processor.CompressAsync(file, 50_000_000);
                    await using var update = connection.CreateCommand();
                    update.Transaction = transaction;
                    update.CommandText = """
                        UPDATE "Hotels" SET "ImageData" = @image
                        WHERE "Id" = @id AND "ImageData" IS NULL AND "ImageUrl" = @url
                        """;
                    AddParameter(update, "@image", compressed);
                    AddParameter(update, "@id", id);
                    AddParameter(update, "@url", url);
                    if (await update.ExecuteNonQueryAsync() != 1)
                        throw new InvalidOperationException("El hotel cambió durante la conversión.");
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidImageException or InvalidOperationException or UriFormatException)
                {
                    throw new InvalidOperationException($"No se pudo convertir la imagen del hotel {id}. La migración se ha detenido; las URL originales se conservan.", ex);
                }
            }
            await transaction.CommitAsync();
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static HttpClient CreateClient()
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            ConnectCallback = async (context, cancellationToken) =>
            {
                var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
                foreach (var address in addresses)
                {
                    if (!IsPublic(address)) continue;
                    var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch (SocketException) { socket.Dispose(); }
                    catch { socket.Dispose(); throw; }
                }
                throw new HttpRequestException("No se pudo conectar a una dirección pública.");
            }
        };
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
    }

    private static bool IsPublic(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var b = address.GetAddressBytes();
        if (b.Length == 16)
            return (b[0] & 0xe0) == 0x20 && !(b[0] == 0x20 && b[1] == 0x01 && b[2] == 0x0d && b[3] == 0xb8);
        return b[0] != 0 && b[0] != 10 && b[0] != 127 && b[0] < 224
            && !(b[0] == 169 && b[1] == 254)
            && !(b[0] == 172 && b[1] >= 16 && b[1] <= 31)
            && !(b[0] == 192 && b[1] == 168)
            && !(b[0] == 100 && b[1] >= 64 && b[1] <= 127)
            && !(b[0] == 198 && (b[1] == 18 || b[1] == 19));
    }

    private static async Task<byte[]> DownloadAsync(HttpClient client, string url)
    {
        var uri = new Uri(url, UriKind.Absolute);
        for (var redirects = 0; redirects <= 3; redirects++)
        {
            if (uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
                throw new InvalidOperationException("Solo se permiten direcciones HTTPS públicas sin credenciales.");
            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            if ((int)response.StatusCode is >= 300 and < 400)
            {
                var location = response.Headers.Location ?? throw new InvalidOperationException("Redirección sin destino.");
                uri = new Uri(uri, location);
                continue;
            }
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > HotelImageProcessor.MaxFileBytes)
                throw new InvalidOperationException("La imagen supera 10 MB.");
            await using var source = await response.Content.ReadAsStreamAsync();
            await using var target = new MemoryStream();
            var buffer = new byte[81920];
            int count;
            while ((count = await source.ReadAsync(buffer)) > 0)
            {
                if (target.Length + count > HotelImageProcessor.MaxFileBytes)
                    throw new InvalidOperationException("La imagen supera 10 MB.");
                await target.WriteAsync(buffer.AsMemory(0, count));
            }
            return target.ToArray();
        }
        throw new InvalidOperationException("Demasiadas redirecciones.");
    }
}
