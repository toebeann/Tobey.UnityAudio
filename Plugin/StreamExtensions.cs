using System.IO;

namespace Tobey.UnityAudio;
internal static class StreamExtensions
{
    public static void CopyTo(this Stream stream, Stream destination, int bufferSize = 16 * 1024)
    {
        byte[] buffer = new byte[bufferSize];
        int bytesRead;
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            destination.Write(buffer, 0, bytesRead);
        }
    }

    public static void SaveToFile(this Stream stream, string path)
    {
        using var fileStream = File.OpenWrite(path);
        stream.Seek(0, SeekOrigin.Begin);
        fileStream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
    }
}
