using Captcha.Model;
using System;
using System.Threading.Tasks;

namespace Captcha.Interface
{
    /// <summary>
    /// 验证码缓存提供器
    /// </summary>
    public interface ICaptchaCacheProvider
    {
        /// <summary>
        /// 保存验证码数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="model"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        Task<bool> SetAsync(string key, CaptchaCacheModel model, TimeSpan expire);

        /// <summary>
        /// 获取验证码数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<CaptchaCacheModel> GetAsync(string key);

        /// <summary>
        /// 保存验证码验证次数缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="num"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        Task<bool> SetNumAsync(string key, int num, TimeSpan expire);

        /// <summary>
        /// 获取验证码验证次数缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<int> GetNumAsync(string key);
    }
}
