using LDFCore.Common.Captcha.Enums;

namespace LDFCore.Common.Captcha.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class CaptchaEarInfo
    {
        public CaptchaBezierLineSide LineSide { get; set; }

        public CaptchaBezierTowards Towards { get; set; }
    }
}
