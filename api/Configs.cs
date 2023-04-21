using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.api
{
    internal class Configs
    {
        public static bool KillOtherWechatDuringBooting = true;

        #region NETWORK
        public static int SocketServerPort = 0;
        #endregion

        #region MESSAGES
        public static int MaxMessageTextLength = 500;
        public static string MessageCombiner = "\n————————————————\n";
        public static int MinMessageSendInterval = 3000;
        public static int MaxMessageSendInterval = 6000;
        #endregion
    }
}
