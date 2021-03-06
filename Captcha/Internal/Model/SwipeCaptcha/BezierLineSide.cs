namespace Captcha.Internal.Model.SwipeCaptcha
{
    /// <summary>
    /// 滑动验证码拼图贝塞尔曲线所在边
    /// </summary>
    public enum BezierLineSide
    {
        /// <summary>
        /// 左边
        /// </summary>
        Left = 1,

        /// <summary>
        /// 下边
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// 右边
        /// </summary>
        Right = 3,

        /// <summary>
        ///  上边
        /// </summary>
        Top = 4
    }
}
