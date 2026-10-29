namespace FourPrep.Modules.Storage.Infrastructure.VirusScanning;

using System.Text;
using System.Net.Sockets;
using System.Buffers.Binary;
using Microsoft.Extensions.Options;
using FourPrep.Modules.Storage.Application.Abstractions.VirusScanning;

internal sealed class ClamAvScanner(IOptions<ClamAvOptions> options) : IVirusScanner
{
    private const int ChunkSize = 64 * 1024;
    private const int LengthPrefixSize = 4;
    private const int MaxReplyLength = 1024;
    private const string ReplyPrefix = "stream: ";
    private const string CleanReply = "stream: OK";
    private const string FoundSuffix = " FOUND";

    private static readonly byte[] InstreamCommand = "zINSTREAM\0"u8.ToArray();

    private readonly ClamAvOptions _options = options.Value;

    public async Task<VirusScanResult> ScanAsync(Stream content, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        using var client = new TcpClient();
        await client.ConnectAsync(_options.Host, _options.Port, timeout.Token);
        await using var network = client.GetStream();

        await network.WriteAsync(InstreamCommand, timeout.Token);
        await SendChunksAsync(content, network, timeout.Token);

        return Parse(await ReadReplyAsync(network, timeout.Token));
    }

    private static async Task SendChunksAsync(Stream content, NetworkStream network, CancellationToken cancellationToken)
    {
        var buffer = new byte[ChunkSize];
        int read;

        while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await network.WriteAsync(LengthPrefix(read), cancellationToken);
            await network.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        await network.WriteAsync(LengthPrefix(0), cancellationToken);
    }

    private static async Task<string> ReadReplyAsync(NetworkStream network, CancellationToken cancellationToken)
    {
        using var reply = new MemoryStream();
        var single = new byte[1];

        while (reply.Length < MaxReplyLength && await network.ReadAsync(single, cancellationToken) > 0 && single[0] != 0)
        {
            reply.WriteByte(single[0]);
        }

        return Encoding.ASCII.GetString(reply.ToArray()).Trim();
    }

    private static VirusScanResult Parse(string reply)
    {
        if (reply == CleanReply)
        {
            return VirusScanResult.Clean;
        }

        if (reply.StartsWith(ReplyPrefix, StringComparison.Ordinal) && reply.EndsWith(FoundSuffix, StringComparison.Ordinal))
        {
            return VirusScanResult.Infected(reply[ReplyPrefix.Length..^FoundSuffix.Length]);
        }

        throw new InvalidOperationException($"ClamAV did not scan the file: {reply}");
    }

    private static byte[] LengthPrefix(int length)
    {
        var prefix = new byte[LengthPrefixSize];
        BinaryPrimitives.WriteInt32BigEndian(prefix, length);
        return prefix;
    }
}
