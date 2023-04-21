using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.api
{
    /// <summary>
    /// 存储一个待发送的消息
    /// </summary>
    internal class SendMessage
    {
        public string Via;
        /// <summary>
        /// 消息类型
        /// </summary>
        public SendMessageType MessageType;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message;
        /// <summary>
        /// 发送目标
        /// </summary>
        public string Target;
        /// <summary>
        /// At名单
        /// </summary>
        public IList<string> Ats;

        public ulong msgId;

        public Dictionary<string, string> Params = null;
        /// <summary>
        /// 对接成功并产生新消息内容时，+1
        /// </summary>
        public int AppendTimes = 0;

        public bool AtNeedAutoNickName;

        public SendMessage(SendMessageType messageType,
            string via,
            string target,
            string message,
            IList<string> ats = null,
            bool atNeedAutoNickName = false,
            ulong msgId = 0,
            Dictionary<string, string> @params = null)
        {
            Via = via;
            MessageType = messageType;
            Message = message;
            Target = target;
            Ats = ats;
            this.msgId = msgId;
            Params = @params;
            AtNeedAutoNickName = atNeedAutoNickName;
        }

        public string GetParam(string key)
        {
            if (this.Params is null) return "";
            return this.Params.ContainsKey(key) ? this.Params[key] : "";
        }

        public bool IsNeedSplit()
        {
            switch (this.MessageType)
            {
                case SendMessageType.PRIVATE_TEXT:
                case SendMessageType.GROUP_TEXT:
                    if (this.Message.Length > Configs.MaxMessageTextLength) return true;
                    break;
                case SendMessageType.GROUP_AT:
                    if (AtNeedAutoNickName) return true;
                    break;
                default: return false;
            }
            return false;
        }
        /// <summary>
        /// 将本消息进行分割
        /// </summary>
        /// <returns></returns>
        public List<SendMessage> Split()
        {
            var ret = new List<SendMessage>();
            var texts = new List<string>();
            var raw = this.Message;
            int startPos = 0;
            while (true)
            {
                var text = raw.Substring(startPos, Math.Min(Configs.MaxMessageTextLength, raw.Length - startPos));
                ret.Add(new SendMessage(this.MessageType, this.Via, text, this.Target, ats: this.Ats, atNeedAutoNickName: this.AtNeedAutoNickName));
                startPos += Configs.MaxMessageTextLength;
                if (startPos >= raw.Length) break;
            }

            int index = 1;
            foreach (var message in ret)
            {
                message.Message += $"\n(长消息 {index}/{ret.Count})";
                index++;
            }

            this.Ats = null;

            return ret;
        }


        /// <summary>
        /// 判断另一个消息是否可以和本消息合并
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsMatch(SendMessage other)
        {
            if (other.MessageType != this.MessageType) return false;
            switch (other.MessageType)
            {
                case SendMessageType.PRIVATE_TEXT:
                case SendMessageType.GROUP_TEXT:
                case SendMessageType.GROUP_AT:
                    if (this.Via != other.Via || this.Target != other.Target) return false;
                    if (this.Message.Length + other.Message.Length + 10 > Configs.MaxMessageTextLength) return false;
                    if (other.MessageType == SendMessageType.GROUP_AT && AtNeedAutoNickName != other.AtNeedAutoNickName) return false;
                    break;
                default: return false;
            }
            return true;
        }

        public void AppendOne(SendMessage other)
        {

            this.Message += Configs.MessageCombiner;
            this.Message += other.Message;
            if (other.Ats != null)
            {
                if (this.Ats is null) this.Ats = other.Ats;
                else
                {
                    foreach (var item in other.Ats) this.Ats.Add(item);
                    other.Ats = null;
                }
            }
        }
        /// <summary>
        /// 打印消息
        /// </summary>
        public void Print()
        {
            var desc = new List<string>
            {
                $">> 发送消息：" + this.MessageType switch
                {
                    SendMessageType.PRIVATE_TEXT => "私聊文本",
                    SendMessageType.GROUP_TEXT => "群组文本",
                    SendMessageType.GROUP_AT => "群组AT",
                    SendMessageType.CARD => "分享名片",
                    SendMessageType.IMAGE => "图片",
                    SendMessageType.FILE => "文件",
                    SendMessageType.ARTICAL => "文章",
                    SendMessageType.APP => "APP",
                    SendMessageType.FORWARD => "转发",
                    SendMessageType.XML => "XML",
                    SendMessageType.EMOTION => "表情",
                    _=> "其他消息",
                },
            };

            if (Ats is not null && Ats.Count > 0) desc.Add("   AT: " + string.Join(", ", Ats.Select(a => $"@{a}")));
            desc.Add("   TO: " + Target);
            if (!string.IsNullOrEmpty(this.Message)) desc.Add("   MSG:\n" + this.Message);
            if (this.Params is not null && this.Params.Count > 0)
            {
                foreach (var param in this.Params)
                {
                    desc.Add($"   {param.Key}: {param.Value}");
                }
            }
            desc.Add("");

            Console.WriteLine(string.Join("\n", desc));
        }
    }
}
