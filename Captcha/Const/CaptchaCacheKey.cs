namespace Captcha.Const
{
    /// <summary>
    /// 验证码缓存Key
    /// </summary>
    public static class CaptchaCacheKey
    {
        /// <summary>
        /// 主要信息存储Key
        /// </summary>
        public const string MainKey = "captcha:main:{0}";

        /// <summary>
        /// 验证次数信息存储Key
        /// </summary>
        public const string NumKey = "captcha:num:{0}";
    }
}
