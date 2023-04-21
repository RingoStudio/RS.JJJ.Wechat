using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RS.Tools.Common.Utils;

namespace RS.Snail.JJJ.Wechat.utils
{
    internal class ProcessHelper
    {
        #region WECHAT PROCESS INSPECTION
        private static Dictionary<int, Process> _processCache = new Dictionary<int, Process>();
        /// <summary>
        /// 获得WECHAT相关进程列表
        /// 并且刷新PID-PROCESS内部缓存
        /// </summary>
        /// <returns></returns>
        public static List<int> GetWechatProcesses()
        {
            _processCache.Clear();

            var ret = new List<int>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.ProcessName.ToLower() == "WeChat".ToLower())
                    {
                        ret.Add(process.Id);
                        _processCache.TryAdd(process.Id, process);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex, "ProcessHelper.GetWechatProcesses 在查找微信进程时发生错误");
                }
            }

            return ret;
        }
        /// <summary>
        /// 是否是微信PID
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static bool IsWechatProcess(int processId)
        {
            return GetProcessByPid(processId) is not null;
        }

        /// <summary>
        /// 检查微信进程是否已经被注入
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static bool CheckProcessInjected(int processId)
        {
            var process = GetProcessByPid(processId);
            if (process is null) return false;
            foreach (ProcessModule item in process.Modules)
            {
                if (item.ModuleName == "SWeChatRobot.dll") return true;
            }
            return false;
        }
        /// <summary>
        /// 获取进程id对应的进程对象
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static Process GetProcessByPid(int processId)
        {
            // 第一次没有，尝试刷新
            if (!_processCache.ContainsKey(processId)) GetWechatProcesses();
            // 第二次没有，失败
            if (!_processCache.ContainsKey(processId)) return null;

            return _processCache[processId];
        }

        /// <summary>
        /// 杀掉指定PID的进程
        /// (仅杀掉WECHAT相关)
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static bool KillWechatProcess(int processId)
        {
            try
            {
                var process = GetProcessByPid(processId);
                if (process is null) return false;
                process.Kill();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, "ProcessHelper.KillProcess 在杀掉微信进程时发生错误");
                return false;
            }

            return true;
        }
        #endregion

    }
}
