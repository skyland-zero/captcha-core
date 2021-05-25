namespace Captcha.Model
{
    /// <summary>
    /// 验证码验证结果返回参数
    /// </summary>
    public class CaptchaValidateResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Msg { get; set; }
    }
}
