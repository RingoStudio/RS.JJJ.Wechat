using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RS.Tools.Common.Utils;
using RS.Tools.Network;
using RS.Tools.Network.Client;

namespace RS.Snail.JJJ.Wechat.api
{
#if DEBUG
    public class WechatMethods
#else
    internal class WechatMethods
#endif
    {

        #region PRIVATE COMMON METHODS
        private static byte[] CommonPostBytes(APIs api, int port, dynamic data = null)
        {
            try
            {
                data = data ?? new JObject();
                var url = $"http://127.0.0.1:{port}/api/?type={(int)api}";
                return HTTPGet.Post(url, data.ToString(), null);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, $"WechatMethods.CommonPostBytes 发生错误 (port:{port}, api:{api}, data:{data})");
                return null;
            }
        }
        private static dynamic CommonPost(APIs api, int port, dynamic data = null)
        {
            try
            {
                var url = $"http://127.0.0.1:{port}/api/?type={(int)api}";
                byte[] resp = HTTPGet.Post(url, data?.ToString() ?? "", null);
                if (resp is null || resp.Length <= 0) return null;
                var json = System.Text.Encoding.UTF8.GetString(resp);
                if (string.IsNullOrEmpty(json) || json == "{}") return null;
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, $"WechatMethods.CommonPost 发生错误 (port:{port}, api:{api}, data:{data})");
                return null;
            }
        }
        private static byte[] CommonGetBytes(APIs api, int port)
        {
            try
            {
                var url = $"http://127.0.0.1:{port}/api/?type={(int)api}";
                return HTTPGet.Get(url);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, $"WechatMethods.CommonGetBytes 发生错误 (port:{port}, api:{api}");
                return null;
            }
        }
        private static dynamic CommonGet(APIs api, int port)
        {
            try
            {
                var url = $"http://127.0.0.1:{port}/api/?type={(int)api}";
                byte[] resp = HTTPGet.Get(url);
                if (resp is null || resp.Length <= 0) return null;
                var json = System.Text.Encoding.UTF8.GetString(resp);
                if (string.IsNullOrEmpty(json) || json == "{}") return null;
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, $"WechatMethods.CommonGet 发生错误 (port:{port}, api:{api}");
                return null;
            }
        }

        private static bool CheckReturnBool(dynamic data, string key = "msg")
        {
            if (data is null || data is not JObject || JSONHelper.GetCount(data) == 0) return false;
            return JSONHelper.ParseBool(data[key]);
        }

        private static bool CheckIsImageStream(byte[] data)
        {
            return true;
        }
        #endregion

        #region PUBLIC METHODS
        #region BASE INFO

        /// <summary>
        /// 检查该微信是否已经登陆
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CheckIsLogin(int port)
        {
            var ret = CommonPost(APIs.WECHAT_IS_LOGIN, port);
            // {is_login:(bool), result:OK}
            return CheckReturnBool(ret, "is_login");
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="port"></param>
        public static bool Logout(int port)
        {
            var ret = CommonPost(APIs.WECHAT_LOGOUT, port);
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }

        /// <summary>
        /// 获得登录的微信号信息
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static dynamic? GetSelfInfo(int port)
        {
            return CommonPost(APIs.WECHAT_GET_SELF_INFO, port)?["data"];

            //{
            //  wxId,
            //  wxNumber,
            //  wxNickName,
            //  Sex,
            //  wxSignature,
            //  wxBigAvatar,
            //  wxSmallAvatar,
            //  wxNation,
            //  wxProvince,
            //  wxCity,
            //  PhoneNumber,
            //  wxFilePath,
            //  uin
            //}
        }
        /// <summary>
        /// 获取当前账号的Wxid
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetSelfWxid(int port)
        {
            var info = GetSelfInfo(port);
            return JSONHelper.ParseString(info?["wxId"]);
        }
        /// <summary>
        /// 获取登录二维码
        /// </summary>
        /// <param name="port"></param>
        /// <returns>图片二进制流/空</returns>
        public static byte[] GetQRCodeImage(int port)
        {
            var bytes = CommonPostBytes(APIs.WECHAT_GET_QRCODE_IMAGE, port);
            // 成功： byte[]
            // 失败: {msg:"获取失败，微信已登录."/"获取失败", result:OK}
            if (bytes is null || bytes.Length <= 0) return null;
#if DEBUG
            System.IO.File.WriteAllBytes("qrcode", bytes);
#endif
            if (CheckIsImageStream(bytes)) return bytes;
            else return null;
        }
        #endregion

        #region SEND MESSAGE
        /// <summary>
        /// 发送文本（群/私）
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool MsgSendText(int port, string wxid, string msg)
        {
            if (string.IsNullOrEmpty(wxid) || string.IsNullOrEmpty(msg)) return false;
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_TEXT, port, JObject.FromObject(new
            {
                wxid = wxid,
                msg = msg
            }));
            //{msg:1, result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送at（群）
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <param name="wxids">要at的wxid列表</param>
        /// <param name="msg"></param>
        /// <param name="auto_nickname">是否在消息最前面自动填写at名字</param>
        /// <returns></returns>
        public static bool MsgSendAt(int port, string chatroom_id, IList<string> wxids, string msg, bool auto_nickname = false)
        {
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_AT, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
                wxids = string.Join(",", wxids),
                msg = msg,
                auto_nickname = auto_nickname ? 1 : 0,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送名片
        /// </summary>
        /// <param name="port"></param>
        /// <param name="receiver"></param>
        /// <param name="shared_wxid"></param>
        /// <param name="nickname"></param>
        /// <returns></returns>
        public static bool MsgSendCard(int port, string receiver, string shared_wxid, string nickname)
        {
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_CARD, port, JObject.FromObject(new
            {
                receiver = receiver,
                shared_wxid = shared_wxid,
                nickname = nickname,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="port"></param>
        /// <param name="receiver"></param>
        /// <param name="img_path"></param>
        /// <returns></returns>
        public static bool MsgSendImage(int port, string receiver, string img_path)
        {
            if (!System.IO.File.Exists(img_path)) return false;
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_IMAGE, port, JObject.FromObject(new
            {
                receiver = receiver,
                img_path = img_path,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="port"></param>
        /// <param name="receiver"></param>
        /// <param name="file_path"></param>
        /// <returns></returns>
        public static bool MsgSendFile(int port, string receiver, string file_path)
        {
            if (!System.IO.File.Exists(file_path)) return false;
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_FILE, port, JObject.FromObject(new
            {
                receiver = receiver,
                file_path = file_path,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送文章
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="title"></param>
        /// <param name="abstract"></param>
        /// <param name="url"></param>
        /// <param name="img_path"></param>
        /// <returns></returns>
        public static bool MsgSendArtical(int port, string wxid, string title, string @abstract, string url, string img_path = "")
        {
            if (string.IsNullOrEmpty(img_path) || !System.IO.File.Exists(img_path)) img_path = "";
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_ARTICLE, port, JObject.FromObject(new
            {
                wxid = wxid,
                title = title,
                @abstract = @abstract,
                url = url,
                img_path = img_path ?? "",
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送app
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static bool MsgSendApp(int port, string wxid, string appid)
        {
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_APP, port, JObject.FromObject(new
            {
                wxid = wxid,
                appid = appid,
            }));
            // { msg: (bool), result: OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 转发消息
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="msgid"></param>
        /// <returns></returns>
        public static bool MsgForwardMessage(int port, string wxid, ulong msgid = ulong.MaxValue)
        {
            var ret = CommonPost(APIs.WECHAT_MSG_FORWARD_MESSAGE, port, JObject.FromObject(new
            {
                wxid = wxid,
                msgid = msgid,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送xml消息
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="xml"></param>
        /// <param name="img_path"></param>
        /// <returns></returns>
        public static bool MsgSendXML(int port, string wxid, string xml, string img_path = "")
        {
            if (string.IsNullOrEmpty(img_path) || !System.IO.File.Exists(img_path)) img_path = "";
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_XML, port, JObject.FromObject(new
            {
                wxid = wxid,
                xml = xml,
                img_path = img_path,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 发送动态表情
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="img_path"></param>
        /// <returns></returns>
        public static bool MsgSendEmotion(int port, string wxid, string img_path)
        {
            if (string.IsNullOrEmpty(img_path) || !System.IO.File.Exists(img_path)) return false;
            var ret = CommonPost(APIs.WECHAT_MSG_SEND_EMOTION, port, JObject.FromObject(new
            {
                wxid = wxid,
                img_path = img_path,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        #endregion

        #region RECEIVE MESSAGE
        /// <summary>
        /// 开启接收消息HOOK，只支持socket监听
        /// </summary>
        /// <param name="port"></param>
        /// <param name="server_port"></param>
        public static void MsgStartHook(int port, int server_port = 10808)
        {
            CommonPostBytes(APIs.WECHAT_MSG_START_HOOK, port, JObject.FromObject(new
            {
                port = server_port,
            }));
        }
        /// <summary>
        /// 关闭接收消息HOOK
        /// </summary>
        /// <param name="port"></param>
        public static void MsgStopHook(int port)
        {
            CommonPostBytes(APIs.WECHAT_MSG_STOP_HOOK, port);
        }
        /// <summary>
        /// 开启图片消息HOOK
        /// {msg:(bool), result:OK}
        /// </summary>
        /// <param name="port"></param>
        /// <param name="save_path"></param>
        /// <returns></returns>
        public static bool MsgStartImageHook(int port, string save_path)
        {
            try
            {
                if (!System.IO.Directory.Exists(save_path)) { System.IO.Directory.CreateDirectory(save_path); }
                var ret = CommonPost(APIs.WECHAT_MSG_START_IMAGE_HOOK, port, JObject.FromObject(new
                {
                    save_path = save_path,
                }));
                // {msg:(bool), result:OK}
                return CheckReturnBool(ret);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, "WechatMethods.MsgStartImageHook");
                return false;
            }
        }
        /// <summary>
        /// 关闭图片消息HOOK
        /// </summary>
        /// <param name="port"></param>
        public static void MsgStopImageHook(int port)
        {
            CommonPostBytes(APIs.WECHAT_MSG_STOP_IMAGE_HOOK, port);
        }
        /// <summary>
        /// 开启语音消息HOOK
        /// {msg:(bool), result:OK}
        /// </summary>
        /// <param name="port"></param>
        /// <param name="save_path"></param>
        /// <returns></returns>
        public static bool MsgStartVoiceHook(int port, string save_path)
        {
            try
            {
                if (!System.IO.Directory.Exists(save_path)) { System.IO.Directory.CreateDirectory(save_path); }
                var ret = CommonPost(APIs.WECHAT_MSG_START_VOICE_HOOK, port, JObject.FromObject(new
                {
                    save_path = save_path,
                }));
                // {msg:(bool), result:OK}
                return CheckReturnBool(ret);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, "WechatMethods.MsgStartImageHook");
                return false;
            }
        }
        /// <summary>
        /// 关闭语音消息HOOK
        /// </summary>
        /// <param name="port"></param>
        public static void MsgStopVoiceHook(int port)
        {
            CommonPostBytes(APIs.WECHAT_MSG_STOP_VOICE_HOOK, port);
        }
        #endregion

        #region CONTACT
        /// <summary>
        /// 获取联系人列表
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static dynamic ContactGetList(int port)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_GET_LIST, port);
            if (ret is null || ret is not JObject) return null;
            return ret["data"];
            // {
            //  data:
            //      [
            //          {
            //              wxid,
            //              wxNumber,
            //              wxNickName,
            //              wxRemark,
            //              wxType,
            //              wxVerifyFlag
            //          }
            //      ],
            //  result:OK
            // }
        }
        /// <summary>
        /// 检查是否被好友删除
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <returns>status code / -1</returns>
        public static int ContactCheckStatus(int port, string wxid)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_CHECK_STATUS, port, JObject.FromObject(new
            {
                wxid = wxid,
            }));
            // {status:(int)好友状态码, result:OK}
            if (ret is null || ret is not JObject || JSONHelper.GetCount(ret) <= 0) return -1;
            return JSONHelper.ParseInt(ret?["status"], -1);
        }
        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <returns></returns>
        public static bool ContactDel(int port, string wxid)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_DEL, port, JObject.FromObject(new
            {
                wxid = wxid,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 从内存中获取好友信息
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <returns></returns>
        public static dynamic ContactSearchByCache(int port, string wxid)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_SEARCH_BY_CACHE, port, JObject.FromObject(new
            {
                wxid = wxid,
            }));
            if (ret is null || ret is not JObject) return null;
            return ret["data"];
            // {
            //  data:
            //      {
            //          wxid,
            //          wxNumber,
            //          wxV3,
            //          wxRemark,
            //          wxNickName,
            //          wxBigAvatar,
            //          wxSmallAvatar,
            //          wxSignature,
            //          wxNation,
            //          wxProvince,
            //          wxCity,
            //          wxBackground
            //      },
            //  result:OK
            // }
        }
        /// <summary>
        /// 网络搜索用户信息 (只有一个结果)
        /// </summary>
        /// <param name="port"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static dynamic ContactSearchByNet(int port, string keyword)
        {
            return CommonPost(APIs.WECHAT_CONTACT_SEARCH_BY_NET, port, JObject.FromObject(new
            {
                keyword = keyword,
            }));
            // {
            //  keyword,
            //  v3,
            //  NickName,
            //  Signature,
            //  Nation,
            //  Province,
            //  City,
            //  BigAvatar,
            //  SmallAvatar,
            //  Sex
            // }
        }
        /// <summary>
        /// wxid加好友
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool ContactAddByWxid(int port, string wxid, string msg)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_ADD_BY_WXID, port, JObject.FromObject(new
            {
                wxid = wxid,
                msg = msg,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// v3数据加好友
        /// </summary>
        /// <param name="port"></param>
        /// <param name="v3"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool ContactAddByV3(int port, string v3, string msg)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_ADD_BY_V3, port, JObject.FromObject(new
            {
                v3 = v3,
                msg = msg,
                add_type = 0x6,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 关注公众号
        /// </summary>
        /// <param name="port"></param>
        /// <param name="public_id"></param>
        /// <returns></returns>
        public static bool ContactAddByPublicID(int port, string public_id)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_ADD_BY_PUBLIC_ID, port, JObject.FromObject(new
            {
                public_id = public_id,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="port"></param>
        /// <param name="v3"></param>
        /// <param name="v4"></param>
        /// <returns></returns>
        public static bool ContactVerifyApply(int port, string v3, string v4)
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_VERIFY_APPLY, port, JObject.FromObject(new
            {
                v3 = v3,
                v4 = v4,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 修改备注
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool ContactEditRemark(int port, string wxid, string remark = "")
        {
            var ret = CommonPost(APIs.WECHAT_CONTACT_EDIT_REMARK, port, JObject.FromObject(new
            {
                wxid = wxid,
                remark = remark,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        #endregion

        #region CHATROOM
        /// <summary>
        /// 获取一个群内的成员列表
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <returns></returns>
        public static dynamic ChatroomGetMemberList(int port, string chatroom_id)
        {
            var ret = CommonPost(APIs.WECHAT_CHATROOM_GET_MEMBER_LIST, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
            }));
            // {members:[???], result:OK}
            if (ret is null || ret is not JObject) return "";
            return JSONHelper.ParseString(ret["members"]);
        }
        /// <summary>
        /// 获取一个群呢指定成员的昵称
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <param name="wxid"></param>
        /// <returns></returns>
        public static string ChatroomGetMemberNick(int port, string chatroom_id, string wxid)
        {
            var ret = CommonPost(APIs.WECHAT_CHATROOM_GET_MEMBER_NICKNAME, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
                wxid = wxid,
            }));
            // {nickname:(string), result:OK}
            if (ret is null || ret is not JObject) return "";
            return JSONHelper.ParseString(ret["nickname"]);
        }
        /// <summary>
        /// 从微信群内移除指定的成员
        /// 需要权限！！！
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <param name="wxids"></param>
        /// <returns></returns>
        public static bool ChatroomDelMember(int port, string chatroom_id, IList<string> wxids)
        {
            var ret = CommonPost(APIs.WECHAT_CHATROOM_DEL_MEMBER, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
                wxids = string.Join(",", wxids),
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 向微信群内增加指定的成员
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <param name="wxids"></param>
        /// <returns></returns>
        public static bool ChatroomAddMember(int port, string chatroom_id, IList<string> wxids)
        {
            var ret = CommonPost(APIs.WECHAT_CHATROOM_ADD_MEMBER, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
                wxids = string.Join(",", wxids),
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 设置群公告
        /// 需要权限！！！
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <param name="announcement"></param>
        /// <returns></returns>
        public static bool ChatroomSetAnnouncement(int port, string chatroom_id, string announcement)
        {
            var ret = CommonPost(APIs.WECHAT_CHATROOM_SET_ANNOUNCEMENT, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
                announcement = announcement,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 设置群名称
        /// 需要权限！！！
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <param name="chatroom_name"></param>
        /// <returns></returns>
        public static bool ChatroomSetChatroomName(int port, string chatroom_id, string chatroom_name)
        {
            var ret = CommonPost(APIs.WECHAT_CHATROOM_SET_CHATROOM_NAME, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
                chatroom_name = chatroom_name,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 设置群内个人昵称
        /// </summary>
        /// <param name="port"></param>
        /// <param name="chatroom_id"></param>
        /// <param name="nickname"></param>
        public static bool ChatroomSetSelfNickname(int port, string chatroom_id, string nickname)
        {
            var ret = CommonPost(APIs.WECHAT_CHATROOM_SET_SELF_NICKNAME, port, JObject.FromObject(new
            {
                chatroom_id = chatroom_id,
                chatroom_name = nickname,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        #endregion

        #region DATA BASE
        /// <summary>
        /// 获取数据库句柄
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static dynamic DatabaseGetHandles(int port)
        {
            var ret = CommonPost(APIs.WECHAT_DATABASE_GET_HANDLES, port);
            // {data:[{
            //         tables:[{name:(string 表名),
            //         tbl_name:(string 表名),
            //         sql:(string 建表语句),
            //         rootpage:(string 表编号)
            //       }],
            //     handle:(int 数据库句柄),
            //     db_name:(string 数据库名)}], 
            //  result:OK}
            if (ret is null || ret is not JObject) return null;
            return ret["data"];
        }
        /// <summary>
        /// 向指定路径备份数据库
        /// {msg:(bool), result:OK}
        /// </summary>
        /// <param name="port"></param>
        /// <param name="db_handle"></param>
        /// <param name="save_path"></param>
        /// <return></return>
        public static bool DatabaseBackup(int port, uint db_handle, string save_path)
        {
            var ret = CommonPost(APIs.WECHAT_DATABASE_BACKUP, port, JObject.FromObject(new
            {
                db_handle = db_handle,
                save_path = save_path,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        /// <summary>
        /// 从数据库执行SQL语句并返回结果
        /// </summary>
        /// <param name="port"></param>
        /// <param name="db_handle"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static dynamic DatabaseQuery(int port, uint db_handle, string sql)
        {
            var ret = CommonPost(APIs.WECHAT_DATABASE_QUERY, port, JObject.FromObject(new
            {
                db_handle = db_handle,
                sql = sql,
            }));
            // {data:[[]](string), result:OK}
            if (ret is null || ret is not JObject) return null;
            ret = ret["data"];
            return ret is JArray ? ret : null;
        }

        public static Dictionary<string, List<(string wxid, string displayName, string nickName)>> ContaceChatGroupMemberNames(int port, string chatroom_id = null)
        {
            var ret = new Dictionary<string, List<(string wxid, string displayName, string nickName)>>();
            try
            {
                var dbData = DatabaseGetHandles(port);
                if (dbData is null || dbData is not JArray || JSONHelper.GetCount(dbData) == 0) return ret;

                var handler = JSONHelper.ParseUInt(dbData?[0]?["handle"]);
                var sql = "";

                //从 ChatRoom 表中取 ChatRoomName(STRING)/UserNameList(STRING) 列
                sql = $"SELECT ChatRoomName,UserNameList FROM ChatRoom";
                var roomIDs = DatabaseQuery(port, handler, sql);
                if (roomIDs is not JArray || JSONHelper.GetCount(roomIDs) == 0) return ret;

                //从 ChatRoom 表中取 RoomData(BLOB) 列
                sql = $"SELECT RoomData FROM ChatRoom";
                var roomData = DatabaseQuery(port, handler, sql) ?? new JArray();

                //从 Contact 表中取 UserName(STRING)/NickName(STRING) 列
                sql = $"SELECT UserName, NickName FROM Contact";
                var contactData = DatabaseQuery(port, handler, sql) ?? new JArray();

                //建立 wxid - 微信昵称 字典
                var nickCache = new Dictionary<string, string>();
                string wxid, nick;
                for (int i = 1; i < JSONHelper.GetCount(contactData); i++)
                {
                    if (JSONHelper.GetCount(contactData[i]) < 2) continue;
                    wxid = JSONHelper.ParseString(contactData[i]?[0]);
                    if (string.IsNullOrEmpty(wxid)) continue;
                    nick = JSONHelper.ParseString(contactData[i]?[1]);
                    if (string.IsNullOrEmpty(nick)) nick = "";
                    if (!nickCache.ContainsKey(wxid)) nickCache.Add(wxid, nick);
                }

                string curRoomId, usersStr, namesStr;
                List<string> memberIDs;
                byte[] roomDataBuf;
                protobuf.ChatRoomData roomProtoModel;
                for (int i = 1; i < JSONHelper.GetCount(roomIDs); i++)
                {
                    if (JSONHelper.GetCount(roomIDs[i]) < 2) continue;
                    //取群ID
                    curRoomId = JSONHelper.ParseString(roomIDs[i]?[0]);
                    if (string.IsNullOrEmpty(curRoomId)) continue;
                    if (!string.IsNullOrEmpty(chatroom_id) && curRoomId != chatroom_id) continue;

                    //取群内wxid列表
                    usersStr = JSONHelper.ParseString(roomIDs[i]?[1]);
                    if (string.IsNullOrEmpty(usersStr)) continue;
                    memberIDs = usersStr.Split("^G").ToList().Where((a) => !string.IsNullOrEmpty(a)).ToList();

                    //取群内BLOG数据，进行Protobuf解析
                    try
                    {
                        roomProtoModel = null;
                        roomDataBuf = Convert.FromBase64String(JSONHelper.ParseString(roomData[i]?[0]));
                        if (roomDataBuf == null || roomDataBuf.Length == 0) continue;
                        roomProtoModel = protobuf.ChatRoomData.Parser.ParseFrom(roomDataBuf);
                        if (roomProtoModel == null || roomProtoModel.Members == null || roomProtoModel.Members.Count <= 0) continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (!ret.ContainsKey(curRoomId)) ret.Add(curRoomId, new List<(string wxid, string displayName, string nickName)>());
                    string displayName, nickName;
                    for (int j = 0; j < memberIDs.Count(); j++)
                    {
                        displayName = "";
                        nickName = nickCache.ContainsKey(memberIDs[j]) ? nickCache[memberIDs[j]] : "";
                        foreach (var member in roomProtoModel.Members)
                        {
                            if (member.WxID == memberIDs[j])
                            {
                                if (!string.IsNullOrEmpty(member.DisplayName))
                                {
                                    displayName = member.DisplayName;
                                }
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(displayName)) displayName = nickName;
                        ret[curRoomId].Add((memberIDs[j], displayName, nickName));
                    }

                    if (!string.IsNullOrEmpty(chatroom_id)) break;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, $"WechatMethods.ContaceChatGroupMemberNames 发生错误");
            }
            return ret;
        }
        #endregion

        #region VERSION
        /// <summary>
        /// 修改微信版本号
        /// {msg:(bool), result:OK}
        /// </summary>
        /// <param name="port"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool SetVersion(int port, string version = "3.7.0.30")
        {
            var ret = CommonPost(APIs.WECHAT_SET_VERSION, port, JObject.FromObject(new
            {
                version = version,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        #endregion

        #region LOG
        /// <summary>
        /// 开启日志信息HOOK
        /// </summary>
        /// <param name="port"></param>
        public static void LogStartHook(int port)
        {
            CommonPostBytes(APIs.WECHAT_LOG_START_HOOK, port);
        }
        /// <summary>
        /// 关闭日志信息HOOK
        /// </summary>
        /// <param name="port"></param>
        public static void LogStopHook(int port)
        {
            CommonPostBytes(APIs.WECHAT_LOG_STOP_HOOK, port);
        }
        #endregion

        #region BROWSER
        /// <summary>
        /// 打开微信内置浏览器
        /// </summary>
        /// <param name="port"></param>
        /// <param name="url"></param>
        public static void BrowserOpenWithUrl(int port, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            CommonPostBytes(APIs.WECHAT_BROWSER_OPEN_WITH_URL, port, JObject.FromObject(new
            {
                url = url,
            }));
        }
        #endregion

        #region 公众号
        /// <summary>
        /// 获取公众号历史消息
        /// </summary>
        /// <param name="port"></param>
        /// <param name="public_id"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetPublicMsg(int port, string public_id, string offset = "")
        {
            var ret = CommonPost(APIs.WECHAT_GET_PUBLIC_MSG, port, JObject.FromObject(new
            {
                public_id = public_id,
                offset = offset,
            }));
            // {msg:(string), result:OK}
            if (ret is null || ret is not JObject) return "";
            return JSONHelper.ParseString(ret["msg"]);
        }
        /// <summary>
        /// 获取公众号文章的A8Key
        /// </summary>
        /// <param name="port"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetA8Key(int port, string url)
        {
            var ret = CommonPost(APIs.WECHAT_GET_A8KEY, port, JObject.FromObject(new
            {
                url = url,
            }));
            // {msg:(string), result:OK}
            if (ret is null || ret is not JObject) return "";
            return JSONHelper.ParseString(ret["msg"]);
        }
        #endregion

        #region TRANSFER
        /// <summary>
        /// 收款
        /// </summary>
        /// <param name="port"></param>
        /// <param name="wxid"></param>
        /// <param name="transcationid"></param>
        /// <param name="transferid"></param>
        /// <returns></returns>
        public static bool GetTransfer(int port, string wxid, string transcationid, string transferid)
        {
            var ret = CommonPost(APIs.WECHAT_GET_TRANSFER, port, JObject.FromObject(new
            {
                wxid = wxid,
                transcationid = transcationid,
                transferid = transferid,
            }));
            // {msg:(bool), result:OK}
            return CheckReturnBool(ret);
        }
        #endregion

        #region CND
        /// <summary>
        /// 获取大文件的CDN地址
        /// </summary>
        /// <param name="port"></param>
        /// <param name="msgID"></param>
        /// <returns></returns>
        public static string GetCDN(int port, ulong msgID)
        {
            var ret = CommonPost(APIs.WECHAT_GET_CDN, port, JObject.FromObject(new
            {
                msgID = msgID,
            }));
            // {msg:(string), result:OK}
            if (ret is null || ret is not JObject) return "";
            return JSONHelper.ParseString(ret["path"]);
        }
        #endregion

        #endregion

    }
}
