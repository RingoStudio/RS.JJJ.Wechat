using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.native
{
    internal class DriverMethods
    {
        const string dllPath = "lib\\http\\wxDriver64.dll";

        /// <summary>
        /// 启动一个新Wechat实例
        /// 该接口可绕过微信防多开机制
        /// ref: DLLEXPORT DWORD new_wechat();
        /// </summary>
        /// <returns></returns>
        [DllImport(dllPath, EntryPoint = "new_wechat")]
        public static extern int NewWechat();

        /// <summary>
        /// 注入并启动服务
        /// ref: DLLEXPORT BOOL start_listen(DWORD pid, int port);
        /// </summary>
        /// <param name="pid">微信实例pid</param>
        /// <param name="port">http端口</param>
        /// <returns></returns>
        [DllImport(dllPath, EntryPoint = "start_listen")]
        public static extern bool StartListen(int pid, int port);

        /// <summary>
        /// 停止并解除注入
        /// ref: DLLEXPORT BOOL stop_listen(DWORD pid);
        /// </summary>
        /// <param name="pid">微信实例pid</param>
        /// <returns></returns>
        [DllImport(dllPath, EntryPoint = "stop_listen")]
        public static extern bool StopListen(int pid);

        
    }
}
