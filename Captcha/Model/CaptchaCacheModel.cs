namespace Captcha.Model
{
    public class CaptchaCacheModel
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
        /// X坐标
        /// </summary>
        public int SwipeX { get; set; }
    }
}
