using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RS.Tools.Common.Utils;
using RS.Snail.JJJ.Wechat.utils;
using Newtonsoft.Json.Linq;

namespace RS.Snail.JJJ.Wechat.api
{
    /// <summary>
    /// 微信实例管理
    /// </summary>
    internal class WechatInstanceMgr
    {
        #region FIELDS
        private Context _context;
        private ConcurrentDictionary<string, WechatInstance> _instances;
        private List<WechatInstance> _pendingLoginInstances;
        private List<string> _loginWxids;
        private bool _isRestart;
        public ConcurrentDictionary<string, WechatInstance> Instances { get { return _instances; } }
        #endregion

        #region INIT
        /// <summary>
        /// 从给定的wxid名单初始化微信实例列表
        /// </summary>
        /// <param name="wxids"></param>
        public WechatInstanceMgr(bool isRestart)
        {
            _isRestart = isRestart;
            LoadCache();
        }

        public bool Init(IList<string> wxids)
        {
            _loginWxids = wxids.ToList();
            // remove not exist wxids
            var removeWxids = new List<string>();
            foreach (var item in _instances) { if (!_loginWxids.Contains(item.Key)) removeWxids.Add(item.Key); }
            foreach (var item in removeWxids) { _instances.TryRemove(item, out _); }

            // add new wxids
            foreach (var item in _loginWxids) { if (!_instances.ContainsKey(item)) _instances.TryAdd(item, new WechatInstance() { Wxid = item }); }

            // check imm
            this.CheckInstanceStatForBooting()
                .ManualLoginWechatsInConsole();
            SaveCache();

            if (_pendingLoginInstances.Count > 0) return false;

            return true;
        }
        /// <summary>
        /// 启动时检查并更新所有微信实例状态
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private WechatInstanceMgr CheckInstanceStatForBooting()
        {
            var allWxPids = ProcessHelper.GetWechatProcesses();

            // 待登录列表
            _pendingLoginInstances = new List<WechatInstance>();

            foreach (var item in _instances.Values)
            {
                if (item.IsInspected) continue;

                // 如果存档中的pid存在，并且端口大于零
                if (item.Pid > 0 && item.Port > 0 && ProcessHelper.IsWechatProcess(item.Pid) && WechatMethods.CheckIsLogin(item.Port))
                {
                    // 尝试重新注入
                    try
                    {
                        native.DriverMethods.StopListen(item.Pid);
                        var newPort = utils.Network.GetRandomPort();
                        var isStarted = native.DriverMethods.StartListen(item.Pid, newPort);
                        if (isStarted) item.Port = newPort;
                        else continue;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    var myInfo = WechatMethods.GetSelfInfo(item.Port);
                    var curWxid = "";
                    if (myInfo is not null && JSONHelper.GetCount(myInfo) > 0)
                    {
                        curWxid = JSONHelper.ParseString(myInfo?["wxId"]);
                    }

                    if (curWxid == item.Wxid)
                    {
                        item.IsInspected = true;
                        continue;
                    }

                    // 这个微信登录了其他的微信号（几率很小）
                    if (!string.IsNullOrEmpty(curWxid) && _instances.ContainsKey(curWxid))
                    {
                        _instances[curWxid].Pid = item.Pid;
                        _instances[curWxid].Port = item.Port;
                        _instances[curWxid].IsInspected = true;
                    }
                }

                item.ClearCache();
                _pendingLoginInstances.Add(item);
            }

            // 按照配置，杀掉检查失效的微信进程
            if (Configs.KillOtherWechatDuringBooting)
            {
                foreach (var pid in allWxPids)
                {
                    if (!IsPidAvailable(pid)) ProcessHelper.KillWechatProcess(pid);
                }
            }

            return this;
        }

        private WechatInstanceMgr ManualLoginWechatsInConsole()
        {
            if (_pendingLoginInstances.Count <= 0) return this;
            // 通过控制台手动确认微信登录情况
            // 若需要通过winform实现，修改此部分
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"有 {_pendingLoginInstances.Count} 个微信号需要重新登录！请按照下面的提示，完成登录操作后，输入Y。");
            int curIndex = 1;
            foreach (var item in _pendingLoginInstances)
            {
                int tryTimes = 0;
                while (!item.IsInspected)
                {
                    tryTimes++;

                    // 多开微信
                    var curPid = native.DriverMethods.NewWechat();
                    int step = 0;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"现在请在刚刚打开的微信中登录wxid为 {item.Wxid} 的账号" +
                                      $"（进度{curIndex}/{_pendingLoginInstances.Count},第{tryTimes}次）\n" +
                                      $"完成后，请按下 Y （按 N 将直接退出, 按 R 将重开一个新微信窗口）\n" +
                                      $"请勿自行关闭微信窗口，否则会崩掉");
                    while (true)
                    {
                        var key = Console.ReadKey();

                        if (key.KeyChar == 'Y') { }
                        else if (key.KeyChar == 'N')
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            return this;
                        }
                        else if (key.KeyChar == 'R')
                        {
                            ProcessHelper.KillWechatProcess(curPid);
                            break;
                        }
                        else continue;

                        try
                        {
                            int port = -1;

                            // STEP 1 启动监听
                            if (step <= 0)
                            {
                                port = utils.Network.GetRandomPort();
                                if (!native.DriverMethods.StartListen(curPid, port))
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"注入微信进程失败，将尝试重新开启一个新的微信窗口！");
                                    ProcessHelper.KillWechatProcess(curPid);
                                    break;
                                }
                                item.Pid = curPid;
                                step++;
                            }

                            // STEP 2 检查是否登录到聊天界面
                            if (step <= 1)
                            {
                                if (!WechatMethods.CheckIsLogin(port))
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"你还没有完成登录！请登录到微信的聊天界面，再按下 Y");
                                    continue;
                                }
                                step++;
                            }

                            // STEP 3 检查是否登录正确的WXID
                            if (step <= 2)
                            {
                                var curWxid = WechatMethods.GetSelfWxid(port);
                                if (string.IsNullOrEmpty(curWxid) || curWxid != item.Wxid)
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"你登陆了错误的微信号！请退出账号后重试！");
                                    continue;
                                }
                                step++;
                            }

                            item.Port = port;
                            item.Pid = curPid;
                            item.IsInspected = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"操作过程中出现了未知的错误！即将重新开始！");
                            Logger.Instance.Write(ex, "WechatInstanceMgr.ManualLoginWechatsInConsole");
                            ProcessHelper.KillWechatProcess(curPid);
                            break;
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"你已经成功登录了 {item.Wxid} ");
                curIndex++;
            }
            Console.ForegroundColor = ConsoleColor.White;
            return this;
        }

        private void LoadCache()
        {
            _instances = new();
            var data = RS.Tools.Common.Utils.IOHelper.GetCSV(RS.Tools.Common.Enums.CSVType.RobotData, "wechat_instances");
            if (data is null || JSONHelper.GetCount(data) == 0) return;
            foreach (var item in data)
            {
                _instances.TryAdd(item.Name, JsonConvert.DeserializeObject<WechatInstance>(item.Value.ToString()));
            }

        }

        private void SaveCache()
        {
            try
            {
                var data = JObject.FromObject(_instances);
                IOHelper.SaveCSV(Tools.Common.Enums.CSVType.RobotData, data, "wechat_instances");
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex, "WechatInstanceMgr.SaveCache");
            }

        }
        #endregion

        #region QUERY
        public int GetWechatPidByWxid(string wxid)
        {
            if (!_instances.ContainsKey(wxid)) return -1;
            return _instances[wxid].Pid;
        }
        public int GetRemotePortByWxid(string wxid)
        {
            if (!_instances.ContainsKey(wxid)) return -1;
            return _instances[wxid].Port;
        }
        #endregion

        #region UTILS
        private bool IsPidAvailable(int pid)
        {
            foreach (var item in _instances.Values)
            {
                if (item.Pid > 0 && item.Pid == pid) return true;
            }
            return false;
        }
        #endregion
    }

    [Serializable]
    internal class WechatInstance
    {
        public int Pid;

        public int Port;

        public string Wxid = "";

        public WechatStat Stat;

        [Newtonsoft.Json.JsonIgnore]
        public bool IsInspected = false;

        public void ClearCache()
        {
            Port = 0;
            Pid = 0;
            Stat = WechatStat.NotStart;
            IsInspected = false;
        }
    }



}
