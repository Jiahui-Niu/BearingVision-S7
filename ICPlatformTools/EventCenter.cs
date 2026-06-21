using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class EventCenter
    {
        public delegate void EventCenterHandler(EventData data);

        public enum EventID
        {
            None,
            SDKConfigChanged,           // sdk配置修改事件
            AppConfigChanged,           // 上层配置修复事件
            CodeCamerasChanged,         // 扫码相机修改事件
            PanoCameraChanged,          // 全景相机修改事件
            LayoutChanged,              // 
            Logger,                     // 
            BeforeStartProject,         // 工程启动前事件, 点击启动按钮后马上触发
            AfterStartProject,          // 工程启动后事件, 在sdk和上层启动过程全完成后触发
            DeviceStatusChanged,        // 设备状态改变事件, 如相机断线后触发
            UpdateHistoryValue,         // 更新历史记录数据库
            UpdateUploadStatus,         // 更新历史记录上传标志
            UpdateUploadSortCode,       // 更新格口号
            UploadComponmentCode,       // 补码更新界面条码
            CameraOffline,              // 相机离线
            ErrorProduced,              // 错误信息
            NotificationRegistering,    // 通知注册
            NotificationPushing,        // 通知推送          
            NotificationUnRegistering,  // 通知反注册
            UpdateKejieConfig,          // 科捷模式切换
            uploadKejieIpcImage,        // 科捷上传图片 
            BeltStateChanged,           // 皮带状态改变
            UploadYZJGImageInfo,        // 邮政金关上传url
			SetGridShowMessage,         // 设置界面包裹信息列表显示的信息
			//UpdateRecvHttpinfo,       // 接收极兔客户端的启动停止信息
            UpdateAppStatus,            // 用于控制界面启停
            CfgDataChanged,             // 用于定制配置文件修改的更新
		    DatabaseMalformed,          // 数据库损坏
            Shutdown,                   // 关机事件
            BeltNotify,                 // 称体PLC响应信息，例如：复位按钮
        }

        private static EventCenter m_instance;

        private readonly Dictionary<EventID, List<EventCenterHandler>> m_eventDict = new Dictionary<EventID, List<EventCenterHandler>>();

        private EventCenter()
        {
        }

        public static EventCenter Instance
        {
            get { return m_instance ?? (m_instance = new EventCenter()); }
        }

        public void Attatch(EventID id, EventCenterHandler handler)
        {
            List<EventCenterHandler> handlerCollection;
            if (!m_eventDict.TryGetValue(id, out handlerCollection))
            {
                handlerCollection = new List<EventCenterHandler>();
                m_eventDict.Add(id, handlerCollection);
            }
            handlerCollection.Add(handler);
            LogHelper.Log.DebugFormat("[EventCenter][Attatch] type: [{0}], handleCount: [{1}].", id, handlerCollection.Count);
        }

        public void Dettach(EventID id, EventCenterHandler handler)
        {
            List<EventCenterHandler> handlerCollection;
            if (m_eventDict.TryGetValue(id, out handlerCollection))
            {
                LogHelper.Log.DebugFormat("[EventCenter][Dettach] type: [{0}], handleCount: [{1}].", id, handlerCollection.Count);
                handlerCollection.Remove(handler);
            }
        }

        public void Notify(EventID id, EventData data = null)
        {
            List<EventCenterHandler> handlerCollection;
            if (m_eventDict.TryGetValue(id, out handlerCollection))
            {
                foreach (var handler in handlerCollection)
                {
                    handler(data ?? EventData.Empty);
                    if (data != null) LogHelper.Log.DebugFormat("[EventCenter][Notify] type: [{0}].", id);
                }
            }
        }
    }

    public class EventData
    {
        private readonly object m_sender;
        private readonly object m_message;

        public EventData(object sender, object message)
        {
            m_sender = sender;
            m_message = message;
        }

        public object Sender
        {
            get { return m_sender; }
        }

        public object Message
        {
            get { return m_message; }
        }

        public static EventData Empty
        {
            get
            {
                return new EventData(null, null);
            }
        }
    }
}
