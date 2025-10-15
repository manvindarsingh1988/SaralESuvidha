using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SaralESuvidha.Models;

public static class BitmapExtension
{
    public static byte[] BitmapToByteArray(this Bitmap bitmap)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}