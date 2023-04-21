using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.api
{
    internal enum SendMessageType
    {
        PRIVATE_TEXT = 0,
        GROUP_TEXT = 1,
        GROUP_AT = 2,
        CARD = 3,
        IMAGE = 4,
        FILE = 5,
        ARTICAL = 6,
        APP = 7,
        FORWARD = 8,
        XML = 9,
        EMOTION = 10,
    }
}
