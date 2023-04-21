using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.api
{
    /// <summary>
    /// HTTP API 功能定义
    /// </summary>
    internal enum APIs : int
    {
        //logincheck
        WECHAT_IS_LOGIN = 0,                    //登录检查

        //selfinfo
        WECHAT_GET_SELF_INFO = 1,               //获取个人信息

        //sendmessage
        WECHAT_MSG_SEND_TEXT = 2,               //发送文本
        WECHAT_MSG_SEND_AT = 3,                 //发送群艾特
        WECHAT_MSG_SEND_CARD = 4,               //分享好友名片
        WECHAT_MSG_SEND_IMAGE = 5,              //发送图片
        WECHAT_MSG_SEND_FILE = 6,               //发送文件
        WECHAT_MSG_SEND_ARTICLE = 7,            //发送xml文章
        WECHAT_MSG_SEND_APP = 8,                //发送小程序

        //receivemessage
        WECHAT_MSG_START_HOOK = 9,              //开启接收消息HOOK，只支持socket监听
        WECHAT_MSG_STOP_HOOK = 10,              //关闭接收消息HOOK
        WECHAT_MSG_START_IMAGE_HOOK = 11,       //开启图片消息HOOK
        WECHAT_MSG_STOP_IMAGE_HOOK = 12,        //关闭图片消息HOOK
        WECHAT_MSG_START_VOICE_HOOK = 13,       //开启语音消息HOOK
        WECHAT_MSG_STOP_VOICE_HOOK = 14,        //关闭语音消息HOOK

        //contact
        WECHAT_CONTACT_GET_LIST = 15,           //获取联系人列表
        WECHAT_CONTACT_CHECK_STATUS = 16,       //检查是否被好友删除
        WECHAT_CONTACT_DEL = 17,                //删除好友
        WECHAT_CONTACT_SEARCH_BY_CACHE = 18,    //从内存中获取好友信息
        WECHAT_CONTACT_SEARCH_BY_NET = 19,      //网络搜索用户信息
        WECHAT_CONTACT_ADD_BY_WXID = 20,        //wxid加好友
        WECHAT_CONTACT_ADD_BY_V3 = 21,          //v3数据加好友
        WECHAT_CONTACT_ADD_BY_PUBLIC_ID = 22,   //关注公众号
        WECHAT_CONTACT_VERIFY_APPLY = 23,       //通过好友请求
        WECHAT_CONTACT_EDIT_REMARK = 24,        //修改备注

        //chatroom
        WECHAT_CHATROOM_GET_MEMBER_LIST = 25,   //获取群成员列表
        WECHAT_CHATROOM_GET_MEMBER_NICKNAME = 26,//获取指定群成员昵称
        WECHAT_CHATROOM_DEL_MEMBER = 27,        //删除群成员
        WECHAT_CHATROOM_ADD_MEMBER = 28,        //添加群成员
        WECHAT_CHATROOM_SET_ANNOUNCEMENT = 29,  //设置群公告
        WECHAT_CHATROOM_SET_CHATROOM_NAME = 30, //设置群聊名称
        WECHAT_CHATROOM_SET_SELF_NICKNAME = 31, //设置群内个人昵称

        //database
        WECHAT_DATABASE_GET_HANDLES = 32,       //获取数据库句柄
        WECHAT_DATABASE_BACKUP = 33,            //备份数据库
        WECHAT_DATABASE_QUERY = 34,             //数据库查询

        //version
        WECHAT_SET_VERSION = 35,                //修改微信版本号

        //log
        WECHAT_LOG_START_HOOK = 36,             //开启日志信息HOOK
        WECHAT_LOG_STOP_HOOK = 37,              //关闭日志信息HOOK

        //browser
        WECHAT_BROWSER_OPEN_WITH_URL = 38,      //打开微信内置浏览器
        WECHAT_GET_PUBLIC_MSG = 39,             //获取公众号历史消息

        WECHAT_MSG_FORWARD_MESSAGE = 40,        //转发消息
        WECHAT_GET_QRCODE_IMAGE = 41,           //获取二维码
        WECHAT_GET_A8KEY = 42,                  //获取A8Key
        WECHAT_MSG_SEND_XML = 43,               //发送xml消息
        WECHAT_LOGOUT = 44,                     //退出登录
        WECHAT_GET_TRANSFER = 45,               //收款
        WECHAT_MSG_SEND_EMOTION = 46,           //发送表情
        WECHAT_GET_CDN = 47,                    //视频/大文件等CDN
    }
}
