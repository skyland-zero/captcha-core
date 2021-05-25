namespace Captcha.Model
{
    /// <summary>
    /// 验证码返回
    /// </summary>
    public class CaptchaOutput
    {
        /// <summary>
        /// 验证码唯一值
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 加密Key
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// 加密IV
        /// </summary>
        public string SecretIV { get; set; }

        /// <summary>
        /// 背景图片
        /// </summary>
        public string BgImage { get; set; }

        /// <summary>
        /// 滑动拼图块
        /// </summary>
        public string SpImage { get; set; }

    }
}
