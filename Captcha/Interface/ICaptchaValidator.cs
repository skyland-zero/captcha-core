using Captcha.Model;
using System.Threading.Tasks;

namespace Captcha.Interface
{
    /// <summary>
    /// 验证码
    /// </summary>
    public interface ICaptchaValidator
    {
        /// <summary>
        /// 验证码判断
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<CaptchaValidateResult> Validate(CaptchaValidateInput input);

        /// <summary>
        /// 业务提交时验证
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<CaptchaValidateResult> ConfirmAsync(CaptchaValidateInput input);

        /// <summary>
        /// 业务提交时验证（二次，一般用不到）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<CaptchaValidateResult> SecondConfirmAsync(CaptchaValidateInput input);

        /// <summary>
        /// 业务提交时验证（三次，一般用不到）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<CaptchaValidateResult> ThirdConfirmAsync(CaptchaValidateInput input);
    }
}
