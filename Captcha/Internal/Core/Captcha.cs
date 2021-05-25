using Captcha.Configuration;
using Captcha.Const;
using Captcha.Interface;
using Captcha.Internal.Helper;
using Captcha.Model;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Threading.Tasks;

namespace Captcha.Internal.Core
{
    public class Captcha : ICaptcha
    {
        /// <summary>
        /// init
        /// </summary>
        /// <param name="options"></param>
        /// <param name="captchaImage"></param>
        /// <param name="cache"></param>
        public Captcha(IOptionsMonitor<CaptchaOptions> options, ISwipeCaptcha swipeCaptcha, ICaptchaCacheProvider cache)
        {
            _options = options.CurrentValue;
            _swipeCaptcha = swipeCaptcha;
            _cache = cache;
        }
        private readonly CaptchaOptions _options;
        private readonly ISwipeCaptcha _swipeCaptcha;
        private readonly ICaptchaCacheProvider _cache;

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        public async Task<CaptchaOutput> GenerateAsync()
        {
            //获取验证码类型

            //根据验证码类型获取验证码
            return await GenerateSwipeAsync();
        }

        /// <summary>
        /// 生成滑动验证码
        /// </summary>
        /// <returns></returns>
        private async Task<CaptchaOutput> GenerateSwipeAsync()
        {
            var bgPath = string.Empty;//GetBgImagePath();
            if (bgPath == null)
            {
                return null;
            }

            var captchas = await _swipeCaptcha.GenerateCaptchaAsync(bgPath);
            var bgBase64 = captchas.Item1.ToBase64String(PngFormat.Instance);
            if (captchas.Item1 != null)
            {
                captchas.Item1.Dispose();
            }
            var spBase64 = await CaptchaHelper.ConvertPngImageToBase64(captchas.Item2);

            var swipeX = captchas.Item3;
            var secret = CaptchaHelper.GenerateSecret();
            var output = new CaptchaOutput()
            {
                Token = Guid.NewGuid().ToString("N"),
                SecretKey = secret.Item1,
                SecretIV = secret.Item2,
                BgImage = bgBase64,
                SpImage = spBase64
            };
            var cache = new CaptchaCacheModel()
            {
                Token = output.Token,
                SecretKey = output.SecretKey,
                SecretIV = output.SecretIV,
                SwipeX = swipeX
            };
            //缓存验证码信息
            var mainKey = string.Format(CaptchaCacheKey.MainKey, output.Token);
            var numKey = string.Format(CaptchaCacheKey.NumKey, output.Token);
            await _cache.SetAsync(mainKey, cache, TimeSpan.FromMinutes(_options.Expiration));
            await _cache.SetNumAsync(numKey, 1, TimeSpan.FromMinutes(_options.Expiration));

            return output;
        }

    }
}
