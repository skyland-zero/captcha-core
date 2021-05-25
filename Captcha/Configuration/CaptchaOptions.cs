using Microsoft.Extensions.Options;

namespace Captcha.Configuration
{
    /// <summary>
    /// 验证码配置
    /// </summary>
    public class CaptchaOptions : IOptions<CaptchaOptions>
    {
        /// <summary>
        /// 值
        /// </summary>
        public CaptchaOptions Value => this;

        /// <summary>
        /// 验证码类型（默认使用滑动验证码）
        /// </summary>
        public CaptchaType Type { get; set; } = CaptchaType.Swipe;

        /// <summary>
        /// 验证码有效时间（分钟，默认为5分钟）
        /// </summary>
        public int Expiration { get; set; } = 5;

        /// <summary>
        /// 滑动验证码配置
        /// </summary>
        public SwipeCaptchaOptions Swipe { get; set; } = new SwipeCaptchaOptions();
    }

    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum CaptchaType
    {
        Random = 1,

        Swipe = 2,
    }

    /// <summary>
    /// 滑动验证码配置
    /// </summary>
    public class SwipeCaptchaOptions
    {
        /// <summary>
        /// 滑动允许偏移量（像素）
        /// </summary>
        public int AllowOffest { get; set; } = 4;

        /// <summary>
        /// 是否生成混淆拼图
        /// </summary>
        public bool GenerateMix { get; set; } = true;

        /// <summary>
        /// 混淆拼图是否随机旋转
        /// </summary>
        public bool MixRandomRota { get; set; } = true;

        /// <summary>
        /// 混淆拼图是否随机缩放
        /// </summary>
        public bool MixRandomScaled { get; set; } = true;

        /// <summary>
        /// 背景拼图是否随机旋转
        /// </summary>
        public bool PuzzleRandomRota { get; set; } = true;

        /// <summary>
        /// 背景拼图是否随机缩放
        /// </summary>
        public bool PuzzleRandomScaled { get; set; } = true;

        /// <summary>
        /// 背景拼图阴影颜色
        /// </summary>
        public string BgPuzzleColor { get; set; } = "#000000";

        /// <summary>
        /// 背景拼图阴影颜色
        /// </summary>
        public float BgPuzzleAlpha { get; set; } = 0.5F;

        /// <summary>
        /// 背景拼图边框颜色
        /// </summary>
        public string BgPuzzlePathColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// 背景拼图边框宽度
        /// </summary>
        public float BgPuzzlePathWidth { get; set; } = 1.5F;

        /// <summary>
        /// 背景拼图透明度
        /// </summary>
        public float BgPuzzlePathAlpha { get; set; } = 0.5F;

        /// <summary>
        /// 拼图滑块边框颜色
        /// </summary>
        public string SpPuzzlePathColor { get; set; } = "#000000";

        /// <summary>
        /// 拼图滑块边框宽度
        /// </summary>
        public float SpPuzzlePathWidth { get; set; } = 1.5F;

        /// <summary>
        /// 拼图滑块边框透明度
        /// </summary>
        public float SpPuzzlePathAlpha { get; set; } = 0.5F;
    }
}