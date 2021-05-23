namespace Captcha.Internal.Model.SwipeCaptcha
{
    /// <summary>
    /// 贝塞尔区县信息
    /// </summary>
    public class BezierLineInfo
    {
        /// <summary>
        /// 滑动验证码拼图贝塞尔曲线所在边
        /// </summary>
        public BezierLineSide LineSide { get; set; }

        /// <summary>
        /// 贝塞尔曲线朝向
        /// </summary>
        public BezierLineTowards Towards { get; set; }
    }
}
