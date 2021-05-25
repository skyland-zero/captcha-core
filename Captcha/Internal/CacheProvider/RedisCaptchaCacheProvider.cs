using Captcha.Interface;
using Captcha.Model;
using FreeRedis;
using System;
using System.Threading.Tasks;

namespace Captcha.Internal.CacheProvider
{
    public class RedisCaptchaCacheProvider : ICaptchaCacheProvider
    {
        /// <summary>
        /// init
        /// </summary>
        /// <param name="cache"></param>
        public RedisCaptchaCacheProvider(RedisClient cache)
        {
            _cache = cache;
        }
        private readonly RedisClient _cache;

        /// <summary>
        /// 保存验证码数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="model"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, CaptchaCacheModel model, TimeSpan expire)
        {
            _cache.Set(key, model, expire);
            return await Task.FromResult(true);
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
            _cache.Set(key, num, expire);
            return await Task.FromResult(true);
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
