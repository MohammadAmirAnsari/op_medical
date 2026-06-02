using BarcodeStandard;
using MudBlazor;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;

namespace OP.PORTAL.Services
{
    public class BarcodeService
    {
        public string GenerateBase64(string text)
        {            
            var b = new Barcode
            {
                IncludeLabel = true
            };
            int baseBarWidth = 2; // pixels per bar
            int length = text.Length;
            int width = Math.Max(250, length * baseBarWidth * 10); // adjust multiplier

            var img = b.Encode(BarcodeStandard.Type.Code128, text, SKColors.Black, SKColors.White, width, 120);

           // var img = b.Encode(BarcodeStandard.Type.Code128, text, SKColors.Black, SKColors.White, 290, 120);
            // Convert SKImage to base64 PNG
            using var ms = new MemoryStream();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(ms);

            return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
        }
    }
}
