using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;

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
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            // Create bitmap QR code
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(10);

            return qrBytes;
        }
    }
}
