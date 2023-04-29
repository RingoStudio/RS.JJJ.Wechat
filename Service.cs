using RS.Snail.JJJ.Wechat.api;
using System.Diagnostics.SymbolStore;

namespace RS.Snail.JJJ.Wechat
{
    public class Service
    {
        #region FIELDS
        private Context _context;
        private Func<dynamic, Task>? _msgCallback = null;
        private Func<dynamic, Task>? _recallCallback = null;
        private Func<string, bool, Task>? _wechatStatCallback = null;
        #endregion

        #region INIT
        public Service(bool isRestart = false, bool isTest = false)
        {
            _context = new Context(isRestart, isTest);
        }
        /// <summary>
        /// 初始化所有微信功能
        /// </summary>
        /// <param name="wxids">需要登录的微信号列表</param>
        /// <param name="msgCallback">微信消息回调</param>
        /// <param name="recallCallback">撤回消息回调</param>
        /// <param name="wechatStatCallback">微信号状态变更回调</param>
        /// <returns></returns>
        public bool Init(IList<string> wxids, Func<dynamic, Task> msgCallback, Func<dynamic, Task> recallCallback = null, Func<string, bool, Task> wechatStatCallback = null)
        {
            _msgCallback = msgCallback;
            _wechatStatCallback = wechatStatCallback;
            return _context.Init(wxids, _msgCallback, recallCallback);
        }
        #endregion

        #region METHODS 
        #region OPERATIONS 控制功能
        /// <summary>
        /// 清除消息队列
        /// </summary>
        /// <param name="via"></param>
        /// <returns></returns>
        public bool ClearMessageQueue(string via = "") => _context.ClearMessageQueue(via);
        /// <summary>
        /// 开始从微信接收消息 (必须先初始化！)
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <returns></returns>
        public bool StartReceive(string via = "") => _context.MsgStartReceive(via);
        /// <summary>
        /// 停止从微信接收消息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        public void StopReceive(string via = "", bool deInject = false) => _context.MsgStopReceive(via: via, deInject: deInject);
        /// <summary>
        /// 开启图片消息HOOK
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="savePath">图片存储的路径</param>
        /// <returns></returns>
        public bool MsgStartImageHook(string via, string savePath) => _context.MsgStartImageHook(via, savePath);
        /// <summary>
        /// 关闭图片消息HOOK
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        public void MsgStopImageHook(string via) => _context.MsgStopImageHook(via);
        /// <summary>
        /// 开启语音消息HOOK
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="savePath">语音存储的路径</param>
        /// <returns></returns>
        public bool MsgStartVoiceHook(string via, string savePath) => _context.MsgStartVoiceHook(via, savePath);
        /// <summary>
        /// 关闭语音消息HOOK
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        public void MsgStopVoiceHook(string via) => _context.MsgStopVoiceHook(via);
        /// <summary>
        /// 开启日志信息HOOK
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        public void LogStartHook(string via) => _context.LogStartHook(via);
        /// <summary>
        /// 关闭日志信息HOOK
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        public void LogStopHook(string via) => _context?.LogStopHook(via);
        #endregion

        #region BASE INFO 基础信息获取
        /// <summary>
        /// 检查该微信号是否登录
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <returns></returns>
        public bool CheckIsLogin(string via) => _context.CheckIsLogin(via);
        /// <summary>
        /// 退出微信号登录
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <returns></returns>
        public bool Logout(string via) => _context.Logout(via);
        /// <summary>
        /// 获得微信号个人信息
        /// </summary>
        /// <param name="vis"></param>
        /// <returns>/{
        ///     "wxId":(string),
        ///     "wxNumber"(string 微信号):,
        ///     "wxNickName":(string 微信昵称),
        ///     "Sex":(int 性别 0 - 未知, 1 - 男, 2 - 女),
        ///     "wxSignature":(string 签名),
        ///     "wxBigAvatar":(string 微信昵称),
        ///     "wxSmallAvatar":,
        ///     "wxNation":,
        ///     "wxProvince":,
        ///     "wxCity":,
        ///     "PhoneNumber":,
        ///     "wxFilePath":(string 微信文件存放路径),
        ///     "uin":(int ref:https://blog.csdn.net/weinierbian/article/details/126700288),
        /// }</returns>
        public dynamic GetSelfInfo(string vis) => _context.GetSelfInfo(vis);
        /// <summary>
        /// 获取微信号登录二维码
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <returns>图像二进制数组, 或null</returns>
        public byte[] GetQRCodeImage(string via) => _context.GetQRCodeImage(via);
        #endregion

        #region SEND MESSAGES 发送消息
        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="msg">消息内容</param>
        /// <returns></returns>
        public bool MsgSendText(string via, string target, string msg) => _context.MsgSendText(via, target, msg);
        /// <summary>
        /// 发送群艾特消息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群</param>
        /// <param name="ats">要at的wxid列表</param>
        /// <param name="msg">消息内容</param>
        /// <param name="autoNickName">是否在消息内容最上方自动填写被at的群成员昵称</param>
        /// <returns></returns>
        public bool MsgSendAt(string via, string chatroom, IList<string> ats, string msg, bool autoNickName = false) => _context.MsgSendAt(via, chatroom, ats, msg, autoNickName);
        /// <summary>
        /// 分享好友名片
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="sharedWxid">名片所属好友wxid</param>
        /// <param name="nickName">名片所属好友昵称</param>
        /// <returns></returns>
        public bool MsgSendCard(string via, string target, string sharedWxid, string nickName) => _context.MsgSendCard(via, target, sharedWxid, nickName);
        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="path">图片本地路径</param>
        /// <returns></returns>
        public bool MsgSendImage(string via, string target, string path) => _context.MsgSendImage(via, target, path);
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="path">图片本地路径</param>
        /// <returns></returns>
        public bool MsgSendFile(string via, string target, string path) => _context.MsgSendFile(via, target, path);
        /// <summary>
        /// 发送文章
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="title">文章标题</param>
        /// <param name="abstract">文章摘要</param>
        /// <param name="url">点击跳转链接</param>
        /// <param name="imgPath">文章封面图片的本地路径</param>
        /// <returns></returns>
        public bool MsgSendArtical(string via, string target, string title, string @abstract, string url, string imgPath = "") => _context.MsgSendArtical(via, target, title, @abstract, url, imgPath);
        /// <summary>
        /// 发送小程序
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="appid">小程序id</param>
        /// <returns></returns>
        public bool MsgSendApp(string via, string target, string appid) => _context.MsgSendApp(via, target, appid);
        /// <summary>
        /// 发送xml消息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="xml">xml内容</param>
        /// <param name="imgPath">封面图片的本地路径</param>
        /// <returns></returns>
        public bool MsgSendXML(string via, string target, string xml, string imgPath = "") => _context.MsgSendXML(via, target, xml, imgPath);
        /// <summary>
        /// 发送动态表情
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="imgPath">表情文件的本地路径</param>
        /// <returns></returns>
        public bool MsgSendEmotion(string via, string target, string imgPath) => _context.MsgSendEmotion(via, target, imgPath);
        /// <summary>
        /// 转发消息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="msgid">要转发的消息id</param>
        /// <returns></returns>
        public bool MsgForwardMessage(string via, string target, ulong msgid = ulong.MaxValue) => _context.MsgForwardMessage(via, target, msgid);
        #endregion

        #region CONTACTS 通讯录
        /// <summary>
        /// 获取联系人列表
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <returns>{
        ///     "wxid":,
        ///     "wxNumber":,
        ///     "wxNickName":,
        ///     "wxRemark":,
        ///     "wxType":(int ),
        ///     "wxVerifyFlag":,
        /// }</returns>
        public dynamic ContactGetList(string via) => _context.ContactGetList(via);
        /// <summary>
        /// 获取好友状态码
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友wxid</param>
        /// <returns></returns>
        public int ContactCheckStatus(string via, string target) => _context.ContactCheckStatus(via, target);
        /// <summary>
        /// 从本地存储中搜索好友信息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <returns>{
        ///     "wxid":,
        ///     "wxNumber":,
        ///     "wxV3":,
        ///     "wxRemark":,
        ///     "wxNickName":,
        ///     "wxBigAvatar":,
        ///     "wxSmallAvatar":,
        ///     "wxSignature":,
        ///     "wxNation":,
        ///     "wxProvince":,
        ///     "wxCity":,
        ///     "wxBackground":,
        /// }</returns>
        public dynamic ContactSearchByCache(string via, string target) => _context.ContactSearchByCache(via, target);
        /// <summary>
        /// 从网络搜索用户信息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>{
        ///     "keyword":,
        ///     "v3":,
        ///     "NickName":,
        ///     "Signature":,
        ///     "Nation":,
        ///     "Province":,
        ///     "City":,
        ///     "BigAvatar":,
        ///     "SmallAvatar":,
        ///     "Sex":,
        /// }</returns>
        public dynamic ContactSearchByNet(string via, string keyword) => _context.ContactSearchByNet(via, keyword);
        /// <summary>
        /// 通过对方wxid加好友
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">对方wxid</param>
        /// <param name="msg">打招呼内容</param>
        /// <returns></returns>
        public bool ContactAddByWxid(string via, string target, string msg) => _context.ContactAddByWxid(via, target, msg);
        /// <summary>
        /// 通过对方v3加好友
        /// v3格式示例如下，是对wxid的加密，v3/v4可通过"附近的人"等方式获取
        /// v3_020b3826fd0301000000000020d52147493461000000501ea9a3dba12f95f6b60a0536a1adb6483caf43bb183ea39a14bdfa3504303bbed35e312933432143d2c0487ba8bb7366eccce1af4fbeb3cc572a20df3a47f8ed1da9f65ab2f327228d3838@stranger
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="v3">对方v3</param>
        /// <param name="msg">打招呼内容</param>
        /// <returns></returns>
        public bool ContactAddByV3(string via, string v3, string msg) => _context.ContactAddByV3(via, v3, msg);
        /// <summary>
        /// 关注公众号
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="publicId">公众号ID（格式 gh_XXX）</param>
        /// <returns></returns>
        public bool ContactAddByPublicID(string via, string publicId) => _context.ContactAddByPublicID(via, publicId);
        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="v3">对方v3</param>
        /// <param name="v4">对方v4</param>
        /// <returns></returns>
        public bool ContactVerifyApply(string via, string v3, string v4) => _context.ContactVerifyApply(via, v3, v4);
        /// <summary>
        /// 修改好友/群备注
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target">目标好友/群wxid</param>
        /// <param name="remark">备注内容</param>
        /// <returns></returns>
        public bool ContactEditRemark(string via, string target, string remark) => _context.ContactEditRemark(via, target, remark);
        /// <summary>
        /// 微信群成员及昵称
        /// </summary>
        /// <param name="via"></param>
        /// <param name="chatroom"></param>
        /// <returns></returns>
        public Dictionary<string, List<(string wxid, string displayName, string nickName)>> ContaceChatGroupMemberNames(string via, string chatroom = "") => _context.ContaceChatGroupMemberNames(via, chatroom);

        #endregion

        #region CHATROOM 群
        /// <summary>
        /// 获取群成员列表(建议使用<see>ContaceChatGroupMemberNames方法</see>)
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群wxid</param>
        /// <returns>{
        ///     ???
        /// }</returns>
        public dynamic ChatroomGetMemberList(string via, string chatroom) => _context.ChatroomGetMemberList(via, chatroom);
        /// <summary>
        /// 获取指定群成员昵称
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群wxid</param>
        /// <param name="target">目标成员wxid</param>
        /// <returns></returns>
        public string ChatroomGetMemberNick(string via, string chatroom, string target) => _context.ChatroomGetMemberNick(via, chatroom, target);
        /// <summary>
        /// 删除群成员(批量)
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群wxid</param>
        /// <param name="targets">要删除的成员wxid列表</param>
        /// <returns></returns>
        public bool ChatroomDelMember(string via, string chatroom, IList<string> targets) => _context.ChatroomDelMember(via, chatroom, targets);
        /// <summary>
        /// 添加群成员(批量)
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群wxid</param>
        /// <param name="targets">要邀请的成员wxid列表</param>
        /// <returns></returns>
        public bool ChatroomAddMember(string via, string chatroom, IList<string> targets) => _context.ChatroomAddMember(via, chatroom, targets);
        /// <summary>
        /// 设置群公告
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群wxid</param>
        /// <param name="announcement">公告内容</param>
        /// <returns></returns>
        public bool ChatroomSetAnnouncement(string via, string chatroom, string announcement) => _context.ChatroomSetAnnouncement(via, chatroom, announcement);
        /// <summary>
        /// 设置群聊名称
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群wxid</param>
        /// <param name="name">群名称内容</param>
        /// <returns></returns>
        public bool ChatroomSetChatroomName(string via, string chatroom, string name) => _context.ChatroomSetChatroomName(via, chatroom, name);
        /// <summary>
        /// 设置群内个人昵称
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="chatroom">目标群wxid</param>
        /// <param name="nickname">昵称内容</param>
        /// <returns></returns>
        public bool ChatroomSetSelfNickname(string via, string chatroom, string nickname) => _context.ChatroomSetSelfNickname(via, chatroom, nickname);
        #endregion

        #region DATA BASE 数据库
        /// <summary>
        /// 获取数据库句柄
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <returns>[
        ///     "tables":[{
        ///         "name":(string 表名),
        ///         "tbl_name":(string 表名),
        ///         "sql":(string 建表语句),
        ///         "rootpage":(string 表编号),
        ///     }],
        ///     "handle":(uint, 数据库句柄),
        ///     "db_name":(string, 数据库名),
        /// ]</returns>
        public dynamic DatabaseGetHandles(string via) => _context.DatabaseGetHandles(via);
        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="handler">数据库句柄</param>
        /// <param name="savePath">保存到本地路径</param>
        /// <returns></returns>
        public bool DatabaseBackup(string via, uint handler, string savePath) => _context.DatabaseBackup(via, handler, savePath);
        /// <summary>
        /// 数据库查询
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="handler">数据库句柄</param>
        /// <param name="sql">要执行的sql内容</param>
        /// <returns>[[(string 字段),...](行),...](Jarray 2D)</returns>
        public dynamic DatabaseQuery(string via, uint handler, string sql) => _context.DatabaseQuery(via, handler, sql);
        #endregion

        #region BROWSER 内置浏览器
        /// <summary>
        /// 打开微信内置浏览器
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="url">打开的url</param>
        public void BrowserOpenWithUrl(string via, string url) => _context.BrowserOpenWithUrl(via, url);
        #endregion

        #region PUBLIC ACCOUNT 公众号
        /// <summary>
        /// 获取公众号历史消息
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="publicId">公众号id</param>
        /// <param name="offset">未知</param>
        /// <returns></returns>
        public string GetPublicMsg(string via, string publicId, string offset = "") => _context.GetPublicMsg(via, publicId, offset);
        /// <summary>
        /// 获取公众号A8Key
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="url">公众号文章链接</param>
        /// <returns></returns>
        public string GetA8Key(string via, string url) => _context.GetA8Key(via, url);
        #endregion

        #region TRANSFERS 转账
        /// <summary>
        /// 收款(具体不详)
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="target"></param>
        /// <param name="transcationid"></param>
        /// <param name="transferid"></param>
        /// <returns></returns>
        public bool GetTransfer(string via, string target, string transcationid, string transferid) => _context.GetTransfer(via, target, transcationid, transferid);
        #endregion

        #region CDN 
        /// <summary>
        /// 视频/大文件等CDN
        /// </summary>
        /// <param name="via">执行操作的微信号</param>
        /// <param name="msgID">消息id</param>
        /// <returns></returns>
        public string GetCDN(string via, ulong msgID) => _context.GetCDN(via, msgID);
        #endregion


        #endregion

    }
}