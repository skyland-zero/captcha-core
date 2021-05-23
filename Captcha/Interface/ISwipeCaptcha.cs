using SixLabors.ImageSharp;
using System.Threading.Tasks;

namespace Captcha.Interface
{
    /// <summary>
    /// 滑动验证码接口
    /// </summary>
    public interface ISwipeCaptcha
    {
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="bgFilePath"></param>
        /// <returns></returns>
        Task<(Image, Image, int)> GenerateCaptchaAsync(string bgFilePath);
    }
}