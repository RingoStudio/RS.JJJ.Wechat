## 描述
### 本项目是 @github/ljc545w 大佬的[ComWechatRobot](https://github.com/ljc545w/ComWeChatRobot) HTTP接口 C#版实现

### 目前实现以下功能：
- 获取通讯录
- 获取所有群成员基础信息(含wxid，群昵称，微信昵称）
- 发送文本、图片、文件、xml文章、名片、群艾特消息
- 根据wxid查询好友信息
- 根据群ID获取所有群成员WXID / 同时获取一个微信群内所有成员的群昵称
- 检测好友状态（是否好友、被删除、被拉黑）
- 接收各类消息，可写回调函数进行处理
- 群管理
- 微信多开
- 自动合并短的文本、艾特信息（可设定单条信息最大长度）
- 自动分割过长的单条信息


## 可用版本
微信3.7.0.30 [下载地址](https://aichunjing.lanzoui.com/b00dd197e)


## 编译环境
Visual Studio 2022 + .Net 6.0


## 使用前准备
- 不需要注册COM组件
- 自行准备一个SOCKET SERVER类，在RS.Snail.JJJ.Wechat.api.Context._messageServer处使用


## 初始化一个机器人实例

```c#
    var wechat = new RS.JJJ.Wechat.Service();
    bool v = wechat.Init(
                        new List<string> { "wxid_1234_" }, 
                        msg => Console.Write(msg)
                        );
    wechat.StartReceive();
```


## 请注意
- 本项目目前仍未正式投入使用，正在持续完善中


## 更新记录
#### 2023.04.21
- 首次发布


## 免责声明
代码仅供交流学习使用，请勿用于非法用途和商业用途！如因此产生任何法律纠纷，均与作者无关！