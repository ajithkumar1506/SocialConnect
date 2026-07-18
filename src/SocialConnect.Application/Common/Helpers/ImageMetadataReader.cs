namespace SocialConnect.Application.Common.Helpers;

public static class ImageMetadataReader
{
    public static (int Width, int Height)? GetDimensions(Stream stream, string contentType)
    {
        try
        {
            var type = contentType.ToLowerInvariant();
            if (type == "image/png")
            {
                return GetPngDimensions(stream);
            }
            if (type == "image/jpeg" || type == "image/jpg")
            {
                return GetJpegDimensions(stream);
            }
            if (type == "image/gif")
            {
                return GetGifDimensions(stream);
            }
        }
        catch
        {
            // Ignore and return null if parsing fails
        }
        return null;
    }

    private static (int Width, int Height)? GetPngDimensions(Stream stream)
    {
        var buffer = new byte[8];
        stream.Position = 16; // Width starts at byte 16
        if (stream.Read(buffer, 0, 8) == 8)
        {
            int width = (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
            int height = (buffer[4] << 24) | (buffer[5] << 16) | (buffer[6] << 8) | buffer[7];
            return (width, height);
        }
        return null;
    }

    private static (int Width, int Height)? GetGifDimensions(Stream stream)
    {
        var buffer = new byte[4];
        stream.Position = 6; // Width starts at byte 6
        if (stream.Read(buffer, 0, 4) == 4)
        {
            int width = buffer[0] | (buffer[1] << 8);
            int height = buffer[2] | (buffer[3] << 8);
            return (width, height);
        }
        return null;
    }

    private static (int Width, int Height)? GetJpegDimensions(Stream stream)
    {
        int b;
        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 0xFF)
            {
                int marker = stream.ReadByte();
                if (marker == -1) break;

                if ((marker >= 0xC0 && marker <= 0xC3) || (marker >= 0xC5 && marker <= 0xC7) ||
                    (marker >= 0xC9 && marker <= 0xCB) || (marker >= 0xCD && marker <= 0xCF))
                {
                    stream.Position += 3;
                    var heightBuf = new byte[2];
                    var widthBuf = new byte[2];
                    if (stream.Read(heightBuf, 0, 2) == 2 && stream.Read(widthBuf, 0, 2) == 2)
                    {
                        int height = (heightBuf[0] << 8) | heightBuf[1];
                        int width = (widthBuf[0] << 8) | widthBuf[1];
                        return (width, height);
                    }
                    break;
                }
                else if (marker == 0xD9 || marker == 0xDA)
                {
                    break;
                }
                else if (marker != 0xD8 && marker != 0x01)
                {
                    int lenHigh = stream.ReadByte();
                    int lenLow = stream.ReadByte();
                    if (lenHigh == -1 || lenLow == -1) break;
                    int len = (lenHigh << 8) | lenLow;
                    stream.Position += (len - 2);
                }
            }
        }
        return null;
    }
}
