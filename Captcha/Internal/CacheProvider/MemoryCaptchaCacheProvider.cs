using Captcha.Interface;
using Captcha.Model;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Captcha.Internal.CacheProvider
{
    /// <summary>
    /// 内存缓存验证码缓冲器
    /// </summary>
    public class MemoryCaptchaCacheProvider : ICaptchaCacheProvider
    {
        /// <summary>
        /// init
        /// </summary>
        /// <param name="cache"></param>
        public MemoryCaptchaCacheProvider(IMemoryCache cache)
        {
            _cache = cache;
        }
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 保存验证码数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="model"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, CaptchaCacheModel model, TimeSpan expire)
        {
            var res = _cache.Set(key, model, expire);
            if (res != null)
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// 获取验证码数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<CaptchaCacheModel> GetAsync(string key)
        {
            var res = _cache.Get<CaptchaCacheModel>(key);
            return await Task.FromResult(res);
        }

        /// <summary>
        /// 保存验证码验证次数缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="num"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task<bool> SetNumAsync(string key, int num, TimeSpan expire)
        {
            var res = _cache.Set(key, num, expire);
            if (res != 0)
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// 获取验证码验证次数缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<int> GetNumAsync(string key)
        {
            var res = _cache.Get<int>(key);
            return await Task.FromResult(res);
        }
    }
}
