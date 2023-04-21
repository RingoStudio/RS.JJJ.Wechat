using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.api
{
    /// <summary>
    /// 微信状态
    /// </summary>
    public enum WechatStat
    {
        /// <summary>
        /// 未启动
        /// </summary>
        NotStart = 0,
        /// <summary>
        /// 在线
        /// </summary>
        Online = 1,
        /// <summary>
        /// 已注入
        /// </summary>
        Injected = 2,
        /// <summary>
        /// 下线（或未登录）
        /// </summary>
        Offline = 3,
        /// <summary>
        /// 异常
        /// </summary>
        Abnormal = 4,
    }
}
