namespace Captcha.Model
{
    /// <summary>
    /// 验证码验证输入
    /// </summary>
    public class CaptchaValidateInput
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 加密信息
        /// </summary>
        public string Payload { get; set; }
    }

    /// <summary>
    /// 验证信息内容
    /// </summary>
    public class CaptchaPayload
    {
        /// <summary>
        /// 滑动距离
        /// </summary>
        public float SwipeX { get; set; }

        /// <summary>
        /// 滑动轨迹（预留）
        /// </summary>
        public int[] Locus { get; set; }
    }
}
