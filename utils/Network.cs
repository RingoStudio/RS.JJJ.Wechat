using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.utils
{
    internal class Network
    {
        /// <summary>
        /// 获取一个随机的未占用端口号
        /// </summary>
        /// <param name="maxRetryTimes"></param>
        /// <returns></returns>
        public static int GetRandomPort(int maxRetryTimes = 1000)
        {
            var used = PortIsUsed();
            var rand = new Random();
            int times = 0;
            while (true)
            {
                var port = rand.Next(1024, 65535);
                if (!used.Contains(port)) return port;
                times++;
                if (times > maxRetryTimes) return -1;
            }

        }
        public static IList PortIsUsed()

        {

            //获取本地计算机的网络连接和通信统计数据的信息

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            //返回本地计算机上的所有Tcp监听程序

            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

            //返回本地计算机上的所有UDP监听程序

            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。

            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            IList allPorts = new ArrayList();

            foreach (IPEndPoint ep in ipsTCP)

            {

                allPorts.Add(ep.Port);

            }

            foreach (IPEndPoint ep in ipsUDP)

            {

                allPorts.Add(ep.Port);

            }

            foreach (TcpConnectionInformation conn in tcpConnInfoArray)

            {

                allPorts.Add(conn.LocalEndPoint.Port);

            }

            return allPorts;

        }
    }
}
