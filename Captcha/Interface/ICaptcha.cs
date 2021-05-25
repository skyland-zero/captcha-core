using Captcha.Model;
using System.Threading.Tasks;

namespace Captcha.Interface
{
    public interface ICaptcha
    {
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        Task<CaptchaOutput> GenerateAsync();
    }
}
