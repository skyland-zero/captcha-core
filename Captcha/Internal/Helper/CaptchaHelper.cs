using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Captcha.Internal.Helper
{
    public static class CaptchaHelper
    {
        private static PngEncoder _pngEncoder => new PngEncoder
        {
            ColorType = PngColorType.RgbWithAlpha,
            TransparentColorMode = PngTransparentColorMode.Clear
        };

        /// <summary>
        /// 拼图图片先保存到文件流再转成base64
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static async Task<string> ConvertPngImageToBase64(Image image)
        {
            //滑块图片
            using var spStream = new MemoryStream();
            await image.SaveAsPngAsync(spStream, _pngEncoder);
            if (image != null)
            {
                image.Dispose();
            }
            spStream.Seek(0, SeekOrigin.Begin);
            var bytes = spStream.ToArray();
            var base64 = Convert.ToBase64String(bytes);
            var spBase64 = "data:image/png;base64," + base64;
            return spBase64;
        }

        /// <summary>
        /// 生成Key和IV
        /// </summary>
        /// <returns></returns>
        public static (string, string) GenerateSecret()
        {
            using Aes aes = Aes.Create();
            return (Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
        }
    }
}
