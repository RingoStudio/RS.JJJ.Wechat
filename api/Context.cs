using Newtonsoft.Json.Linq;
using RS.Snail.JJJ.Wechat.native;
using RS.Snail.JJJ.Wechat.utils;
using RS.Tools.Common.Utils;
// using RS.Tools.Network.Sockets;
using System;
using System.Collections.Concurrent;
using System.Text;
using RS.Tools.Network.Sockets;

namespace RS.Snail.JJJ.Wechat.api
{
    /// <summary>
    /// 上下文类
    /// </summary>
    internal class Context
    {
        #region FIELDS
        private WechatInstanceMgr _instanceMgr;
        private Func<dynamic, Task> _receivedCallback;
        private Func<dynamic, Task> _recallCallback;
        private bool _testFlag;
        #endregion

        #region INIT
        public Context(bool isRestart, bool testFlag = false)
        {
            _instanceMgr = new WechatInstanceMgr(isRestart);
            _testFlag = testFlag;
        }

        public bool Init(IList<string> wxids, Func<dynamic, Task> receivedCallback, Func<dynamic, Task> recallCallback)
        {
            if (!_testFlag)
            {
                if (!_instanceMgr.Init(wxids)) return false;
            }
            _receivedCallback = receivedCallback;
            _recallCallback = recallCallback;
            _msgNeedCache = _recallCallback is not null;
            InitMessageQueue(wxids);
            return true;
        }
        #endregion

        #region BASE INFO METHODS
        /// <summary>
        /// 清除消息队列
        /// </summary>
        /// <param name="via"></param>
        /// <returns></returns>
        public bool ClearMessageQueue(string via = "")
        {
            try
            {
                if (string.IsNullOrEmpty(via))
                {
                    foreach (var key in _messageQueue.Keys)
                    {
                        if (_messageQueue[key] is not null && _messageQueue[key].Count > 0) _messageQueue[key].Clear();
                    }
                    return true;
                }
                else
                {
                    if (!_messageQueue.ContainsKey(via)) return false;
                    if (_messageQueue[via] is null || _messageQueue[via].Count == 0) return true;
                    _messageQueue[via].Clear();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, "ClearMessageQueue");
                return false;
            }
        }

        public bool CheckIsLogin(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.CheckIsLogin(port);
        }

        public bool Logout(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.Logout(port);
        }
        public dynamic GetSelfInfo(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.GetSelfInfo(port);
        }

        public byte[] GetQRCodeImage(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.GetQRCodeImage(port);
        }
        #endregion

        #region SEND MESSAGE 要进行消息队列/消息合并
        public bool MsgSendText(string via, string target, string msg, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.PRIVATE_TEXT, via, target, msg));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendText(port, target, msg);
        }

        public bool MsgSendAt(string via, string chatroomID, IList<string> targets, string msg, bool autoNickName = false, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.GROUP_AT, via, chatroomID, msg, ats: targets, atNeedAutoNickName: autoNickName));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendAt(port, chatroomID, targets, msg, autoNickName);
        }

        public bool MsgSendCard(string via, string target, string sharedWxid, string nickName, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.CARD, via, target, "", @params: new Dictionary<string, string> {
                    { "shard_wxid", sharedWxid },
                    { "nickname", nickName },
                }));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendCard(port, target, sharedWxid, nickName);
        }

        public bool MsgSendImage(string via, string target, string path, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.IMAGE, via, target, "", @params: new Dictionary<string, string> {
                    { "path", path },
                }));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendImage(port, target, path);
        }

        public bool MsgSendFile(string via, string target, string path, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.FILE, via, target, "", @params: new Dictionary<string, string> {
                    { "path", path },
                }));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendFile(port, target, path);
        }

        public bool MsgSendArtical(string via, string target, string title, string @abstract, string url, string imgPath = "", bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.ARTICAL, via, target, "", @params: new Dictionary<string, string> {
                    { "title", title },
                    { "abstract", @abstract },
                    { "url", url },
                    { "path", imgPath ?? "" },
                }));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendArtical(port, target, title, @abstract, url, imgPath);
        }

        public bool MsgSendApp(string via, string target, string appid, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.APP, via, target, "", @params: new Dictionary<string, string> {
                    { "appid", appid },
                }));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendApp(port, target, appid);
        }
        public bool MsgSendXML(string via, string target, string xml, string imgPath = "", bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.XML, via, target, "", @params: new Dictionary<string, string> {
                    { "xml", xml },
                    { "path", imgPath },
                }));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendXML(port, target, xml, imgPath);
        }

        public bool MsgSendEmotion(string via, string target, string imgPath, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.EMOTION, via, target, "", @params: new Dictionary<string, string> {
                    { "path", imgPath },
                }));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgSendEmotion(port, target, imgPath);
        }

        public bool MsgForwardMessage(string via, string target, ulong msgid = ulong.MaxValue, bool immediately = false)
        {
            if (!immediately)
            {
                MessageEnqueue(new SendMessage(SendMessageType.EMOTION, via, target, "", msgId: msgid));
                return true;
            }
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgForwardMessage(port, target, msgid);
        }
        #endregion

        #region RECEIVE MESSGAE
        public bool MsgStartImageHook(string via, string savePath)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgStartImageHook(port, savePath);
        }

        public void MsgStopImageHook(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return;
            WechatMethods.MsgStopImageHook(port);
        }

        public bool MsgStartVoiceHook(string via, string savePath)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.MsgStartVoiceHook(port, savePath);
        }
        public void MsgStopVoiceHook(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return;
            WechatMethods.MsgStopVoiceHook(port);
        }
        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="via">要启动的微信号，留空为全部启动</param>
        /// <returns></returns>
        public bool MsgStartReceive(string via = "")
        {
            int successCnt = 0;
            if (!InitServer()) return false;

            foreach (var item in _instanceMgr.Instances)
            {
                if (!string.IsNullOrEmpty(via) && item.Key != via) continue;
                if (!item.Value.IsInspected || item.Value.Pid <= 0 || item.Value.Port <= 0) continue;

                try
                {
                    if (!DriverMethods.StartListen(item.Value.Pid, item.Value.Port)) continue;
#if DEBUG
                    //  LogStartHook(item.Key);
#endif
                    WechatMethods.MsgStartHook(item.Value.Port, _messageServerPort);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex, $"Context.MsgStartReceive, WXID:{item.Key}");
                    continue;
                }

                successCnt++;
            }

            return successCnt > 0;
        }

        /// 停止监听
        /// </summary>
        /// <param name="deInject">是否解除注入</param>
        /// <param name="via">要停止的微信号，留空为全部停止</param>
        /// <returns></returns>
        public bool MsgStopReceive(bool deInject = false, string via = "")
        {
            int successCnt = 0;
            foreach (var item in _instanceMgr.Instances)
            {
                if (!string.IsNullOrEmpty(via) && item.Key != via) continue;
                if (!item.Value.IsInspected || item.Value.Pid <= 0 || item.Value.Port <= 0) continue;

                try
                {
                    if (deInject)
                    {
                        if (!DriverMethods.StopListen(item.Value.Pid)) continue;
                    }
#if DEBUG
                    LogStopHook(item.Key);
#endif
                    WechatMethods.MsgStopHook(item.Value.Port);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex, $"Context.MsgStopReceive, WXID:{item.Key}");
                    continue;
                }

                successCnt++;
            }

            if (string.IsNullOrEmpty(via)) StopServer();

            return successCnt > 0;
        }
        #endregion

        #region CONTACT
        public dynamic ContactGetList(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.ContactGetList(port);
        }

        public int ContactCheckStatus(string via, string target)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return -1;
            return WechatMethods.ContactCheckStatus(port, target);
        }
        public bool ContactDel(string via, string target)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ContactDel(port, target);
        }
        public dynamic ContactSearchByCache(string via, string target)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.ContactSearchByCache(port, target);
        }
        public dynamic ContactSearchByNet(string via, string keyword)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.ContactSearchByNet(port, keyword);
        }
        public bool ContactAddByWxid(string via, string target, string msg)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ContactAddByWxid(port, target, msg);
        }
        public bool ContactAddByV3(string via, string v3, string msg)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ContactAddByV3(port, v3, msg);
        }
        public bool ContactAddByPublicID(string via, string public_id)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ContactAddByPublicID(port, public_id);
        }
        public bool ContactVerifyApply(string via, string v3, string v4)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ContactVerifyApply(port, v3, v4);
        }
        public bool ContactEditRemark(string via, string target, string remark)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ContactEditRemark(port, target, remark);
        }

        public Dictionary<string, List<(string wxid, string displayName, string nickName)>> ContaceChatGroupMemberNames(string via, string chatroomId = "")
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return new Dictionary<string, List<(string wxid, string displayName, string nickName)>>();
            return WechatMethods.ContaceChatGroupMemberNames(port, chatroomId);
        }


        #endregion

        #region CHATROOM
        public dynamic ChatroomGetMemberList(string via, string target)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.ChatroomGetMemberList(port, target);
        }
        public string ChatroomGetMemberNick(string via, string chatroomID, string target)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return "";
            return WechatMethods.ChatroomGetMemberNick(port, chatroomID, target);
        }
        public bool ChatroomDelMember(string via, string chatroomID, IList<string> wxids)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ChatroomDelMember(port, chatroomID, wxids);
        }
        public bool ChatroomAddMember(string via, string chatroomID, IList<string> wxids)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ChatroomAddMember(port, chatroomID, wxids);
        }
        public bool ChatroomSetAnnouncement(string via, string chatroomID, string announcement)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ChatroomSetAnnouncement(port, chatroomID, announcement);
        }
        public bool ChatroomSetChatroomName(string via, string chatroomID, string chatroom_name)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ChatroomSetAnnouncement(port, chatroomID, chatroom_name);
        }
        public bool ChatroomSetSelfNickname(string via, string chatroomID, string nickname)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.ChatroomSetSelfNickname(port, chatroomID, nickname);
        }
        #endregion

        #region DATA BASE
        public dynamic DatabaseGetHandles(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.DatabaseGetHandles(port);
        }
        public bool DatabaseBackup(string via, uint dbHandle, string savePath)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.DatabaseBackup(port, dbHandle, savePath);
        }
        public dynamic DatabaseQuery(string via, uint dbHandle, string sql)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return null;
            return WechatMethods.DatabaseQuery(port, dbHandle, sql);
        }
        #endregion

        #region LOG
        public void LogStartHook(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return;
            WechatMethods.LogStartHook(port);
        }
        public void LogStopHook(string via)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return;
            WechatMethods.LogStopHook(port);
        }
        #endregion

        #region BROWSER
        public void BrowserOpenWithUrl(string via, string url)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return;
            WechatMethods.BrowserOpenWithUrl(port, url);
        }
        #endregion

        #region PUBLIC ACCOUNT
        public string GetPublicMsg(string via, string public_id, string offset = "")
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return "";
            return WechatMethods.GetPublicMsg(port, public_id, offset);
        }
        public string GetA8Key(string via, string url)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return "";
            return WechatMethods.GetA8Key(port, url);
        }
        #endregion

        #region TRANSFERS
        public bool GetTransfer(string via, string target, string transcationid, string transferid)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return false;
            return WechatMethods.GetTransfer(port, target, transcationid, transferid);
        }
        #endregion

        #region CDN
        public string GetCDN(string via, ulong msgID)
        {
            var port = _instanceMgr.GetRemotePortByWxid(via);
            if (port < 0) return "";
            return WechatMethods.GetCDN(port, msgID);
        }
        #endregion

        #region MESSAGE QUEUE
        private ConcurrentDictionary<string, ConcurrentQueue<SendMessage>> _messageQueue;
        private ConcurrentDictionary<string, long> _lastSendLog;
        // private bool _isSending = false;
        private int _curInterval = Configs.MinMessageSendInterval;
        private Random _randInterval = new Random();
        private Task _sendingTask;
        private void InitMessageQueue(IList<string> wxids)
        {
            _messageQueue = new ConcurrentDictionary<string, ConcurrentQueue<SendMessage>>();
            _lastSendLog = new ConcurrentDictionary<string, long>();
            foreach (var wxid in wxids)
            {
                _messageQueue.TryAdd(wxid, new ConcurrentQueue<SendMessage>());
                _lastSendLog.TryAdd(wxid, 0);
            }
        }



        private void MessageEnqueue(SendMessage message)
        {
            var targets = _messageQueue.ContainsKey(message.Via) ? _messageQueue[message.Via] : null;
            if (targets is null) return;

            if (_testFlag)
            {
                message.Print();
                return;
            }

            // 长消息 切割
            if (message.IsNeedSplit())
            {
                foreach (var item in message.Split())
                {
                    targets.Enqueue(item);
                }
                return;
            }

            // 短消息 合并
            foreach (var item in targets)
            {
                if (item.IsMatch(message))
                {
                    item.AppendOne(message);
                    return;
                }
            }

            targets.Enqueue(message);

            if (_sendingTask is null || _sendingTask.IsCompleted)
            {
                _sendingTask = Task.Run(() => SendingMessage());
            }

        }

        private void SendingMessage()
        {
            do
            {
                // 检查上一次发送的时间间隔
                foreach (var item in _messageQueue)
                {
                    if (_lastSendLog[item.Key] + _curInterval < RS.Tools.Common.Utils.TimeHelper.ToTimeStampMills())
                    {
                        item.Value.TryDequeue(out var message);
                        if (message is null) continue;
                        SendMessageImm(message);
                    }
                }

                // 检查队列里面是否有待发送的消息
                foreach (var item in _messageQueue)
                {
                    if (item.Value.Count > 0) continue;
                }

                // 队列空，结束循环
                // _isSending = false;
                return;
            } while (true);
        }
        //CARD = 3,
        //IMAGE = 4,
        //FILE = 5,
        //ARTICAL = 6,
        //APP = 7,
        //FORWARD = 8,
        //XML = 9,
        //EMOTION = 10,
        private void SendMessageImm(SendMessage message)
        {
            switch (message.MessageType)
            {
                case SendMessageType.PRIVATE_TEXT:
                case SendMessageType.GROUP_TEXT:
                    MsgSendText(message.Via, message.Target, message.Message, true);
                    break;
                case SendMessageType.GROUP_AT:
                    MsgSendAt(message.Via, message.Target, message.Ats, message.Message, message.AtNeedAutoNickName, true);
                    break;
                case SendMessageType.CARD:
                    MsgSendCard(message.Via, message.Target, message.GetParam("shared_wxid"), message.GetParam("nickname"), true);
                    break;
                case SendMessageType.IMAGE:
                    MsgSendImage(message.Via, message.Target, message.GetParam("path"), true);
                    break;
                case SendMessageType.FILE:
                    MsgSendFile(message.Via, message.Target, message.GetParam("path"), true);
                    break;
                case SendMessageType.ARTICAL:
                    MsgSendArtical(message.Via, message.Target, message.GetParam("title"), message.GetParam("abstract"), message.GetParam("url"), message.GetParam("path"), true);
                    break;
                case SendMessageType.APP:
                    MsgSendApp(message.Via, message.Target, message.GetParam("appid"), true);
                    break;
                case SendMessageType.FORWARD:
                    MsgForwardMessage(message.Via, message.Target, message.msgId, true);
                    break;
                case SendMessageType.XML:
                    MsgSendXML(message.Via, message.Target, message.GetParam("xml"), message.GetParam("path"), true);
                    break;
                case SendMessageType.EMOTION:
                    MsgSendEmotion(message.Via, message.Target, message.GetParam("path"), true);
                    break;
            }
            UpdateSendMessageInterval();
        }

        private void UpdateSendMessageInterval()
        {
            _curInterval = _randInterval.Next(Configs.MinMessageSendInterval, Configs.MaxMessageSendInterval);
        }
        #endregion

        #region MESSAGE SERVICE
        /// <summary>
        /// 消息接收服务器
        /// </summary>
        //private RSTCPServer _messageServer;
        private RSTCPServer _messageServer;
        /// <summary>
        /// 消息接收服务器端口
        /// </summary>
        private int _messageServerPort;
        /// <summary>
        /// 消息缓存
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<ulong, dynamic>> _msgCache = new();
        /// <summary>
        /// 是否需要缓存消息
        /// </summary>
        private bool _msgNeedCache = false;
        /// <summary>
        /// 可接收的消息类型
        /// </summary>
        private static List<RS.Tools.Common.Enums.WechatMessageType> _receiveMessageTypes = new()
        {
            RS.Tools.Common.Enums.WechatMessageType.Text,
            RS.Tools.Common.Enums.WechatMessageType.Image,
            RS.Tools.Common.Enums.WechatMessageType.File,
        };

        public bool IsMessageServerWorking
        {
            get => _messageServer is not null && _messageServer.Running;
        }

        /// <summary>
        /// 初始化并开始监听
        /// </summary>
        /// <returns></returns>
        private bool InitServer()
        {
            if (IsMessageServerWorking) return true;
            _messageServerPort = Network.GetRandomPort();

            try
            {
                _messageServer = new RSTCPServer(_messageServerPort)
                {
                    OnReceived = (c, d) => OnMessageReceived(d),

#if DEBUG
                    //OnConnected = c => Console.WriteLine("连接 " + c.ToString()),
                    //OnDisconnected = c => Console.WriteLine("断开 " + c.ToString()),
#endif
                };

                _messageServer.Start();

                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// 停止监听
        /// </summary>
        /// <returns></returns>
        private bool StopServer()
        {
            try
            {
                if (_messageServer is null) return false;
                _messageServer?.Stop();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, "StopSocktServer");
            }
            return false;
        }

        private Task _receiveTask;
        private ConcurrentQueue<byte[]> _receiveQueue;
        /// <summary>
        /// 消息到达时触发
        /// </summary>
        /// <param name="raw"></param>
        private async void OnMessageReceived(byte[] raw)
        {
            try
            {
                if (raw is null || raw.Length <= 0) return;
                _receiveQueue.Enqueue(raw);

                if (_receiveTask is null || _receiveTask.IsCompleted)
                {
                    _receiveTask = TreatReceiveMessage();
                }


            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, "Wechat.OnMesageReceived");
            }
        }

        private async Task TreatReceiveMessage()
        {
            try
            {
                do
                {
                    if (_receiveQueue.Count == 0) return;
                    var flag = _receiveQueue.TryDequeue(out var raw);
                    if (!flag || raw is null || raw.Length <= 0) return;

                    var data = Encoding.UTF8.GetString(raw);
                    var jo = JObject.Parse(data);
                    // Console.WriteLine(jo);
                    if (jo is not JObject) return;

                    // 过滤掉自己发送的消息
                    if (JSONHelper.ParseBool(jo["isSendMsg"])) return;

                    var type = (RS.Tools.Common.Enums.WechatMessageType)JSONHelper.ParseInt(jo["type"]);
                    if (type == Tools.Common.Enums.WechatMessageType.Recall && _msgNeedCache)
                    {
                        // 撤回消息类型，检索消息缓存并回调
                        var msgID = JSONHelper.ParseULong(jo["msgid"]);
                        var self = JSONHelper.ParseString(jo["self"]);
                        var recall = QueryCachedMessage(self, msgID);
                        if (recall is not null) await _recallCallback?.Invoke(recall);
                    }
                    else
                    {
                        // 过滤掉不关注的消息类型
                        if (!_receiveMessageTypes.Contains(type)) return;

                        // type49(File) 过滤掉不是文件的情况
                        if (type == Tools.Common.Enums.WechatMessageType.File)
                        {
                            var path = JSONHelper.ParseString(jo["filepath"]);
                            if (!path.Contains("\\FileStorage\\MsgAttach\\")) return;
                        }

                        // 缓存消息
                        if (_msgNeedCache) CacheMessage(jo);

                        if (_receivedCallback is not null) await _receivedCallback.Invoke(jo);
                    }
                } while (true);

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void CacheMessage(dynamic msg)
        {
            ulong msgid = JSONHelper.ParseULong(msg.msgid);
            long time = JSONHelper.ParseLong(msg.timestamp);
            string self = JSONHelper.ParseString(msg.self);
            if (!_msgCache.ContainsKey(self)) _msgCache.TryAdd(self, new());
            _msgCache[self][msgid] = new { time = time, msg = msg };

            if (_msgCache[self].Count > 100)
            {
                var now = TimeHelper.ToTimeStamp();
                _msgCache[self] = _msgCache[self].Where(x => now - x.Value.time >= 300).ToDictionary(k => k.Key, k => k.Value);
            }
        }

        private dynamic? QueryCachedMessage(string self, ulong msgid)
        {
            if (!_msgCache.ContainsKey(self) || !_msgCache[self].ContainsKey(msgid)) return null;
            return _msgCache[self][msgid]?.msg;
        }

        #endregion
    }
}
