using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace ProcessManagement.Services.QRCodes
{
    public class QRCodeServices
    {
        private byte[]? qrcodeimage = null;
        public byte[]? QRCodeImage
        {
            get { return qrcodeimage; }
            set
            {
                qrcodeimage = value;
            }
        }

        public string GenerateQRCode(string qrCodeString, int imgsize)
        {
            if (qrCodeString != null)
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeString, QRCodeGenerator.ECCLevel.Q);

                // Calculate the module size based on the desired QR code size
                int moduleSize = imgsize / qrCodeData.ModuleMatrix.Count;

                Base64QRCode qrCode = new(qrCodeData);
                string qrCodeImageAsBase64 = qrCode.GetGraphic(moduleSize);

                return qrCodeImageAsBase64;
            }
            else
            {
                return string.Empty;
            }
        }

        public byte[] GenerateQRCodeImage(string content)
        {   
            // Create QR code generator
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.L);

            // Create bitmap QR code
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(25);

            return qrBytes;
        }

        public byte[] GenerateQrCode(string text, int height, int width)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCode = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L))
                {
                    // Create QR code bitmap
                    var qrCodeBmp = new BitmapByteQRCode(qrCode);
                    byte[] qrCodeImage = qrCodeBmp.GetGraphic(20); // 20 is pixel size per module

                    // Convert byte array to Bitmap
                    using (var ms = new MemoryStream(qrCodeImage))
                    {
                        var originalBitmap = new Bitmap(ms);

                        // Create new bitmap with desired dimensions
                        var resizedBitmap = new Bitmap(width, height);
                        using (var graphics = Graphics.FromImage(resizedBitmap))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(originalBitmap, 0, 0, width, height);
                        }

                        // Convert Bitmap to PNG bytes
                        resizedBitmap.Save(ms, ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();

                        return imageBytes;
                    }
                }
            }
        }
    }
}
