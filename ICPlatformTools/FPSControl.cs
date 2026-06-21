using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class FPSControl
    {
        private object lockObj = new object();

        private double m_displayInterval = 0;
        private double m_timestampInterval = 0;
        private Int64 m_firstFrameTime = 0;
        private Int64 m_lastFrameTime = 0;
        private volatile bool m_forceDisplay = true;


        public void SetDisplayFPS(int fps)
        {
            lock (lockObj)
            {
                m_displayInterval = (fps > 0 ? 1000.0 / fps : 0);
            }

            m_forceDisplay = (fps < 0);
        }

        public void SetTimestampInterval(double interval)
        {
            lock (lockObj)
            {
                m_timestampInterval = interval;
            }
        }

        public void SetTimestampFrequency(Int64 freq)
        {
            SetTimestampInterval(freq <= 0 ? 0 : 1000.0 / freq);
        }

        public bool IsTimeToDisplay(Int64 timestamp)
        {
            ///
            /// 算法图示:
            /// 0       500         1000        1500        |2000   显示时间轴, 显示帧率2, 显示间隔500ms
            /// 0     400       800       1200     1600     |2000   帧时间轴, 每隔400ms采集一帧, 2秒内采集5帧
            ///    (400,100) (300,200) (200,300) (100,400)  |       每一帧距离前后显示时间点的时间差
            /// s      s         h         s        s       |       显示状态, s:显示, h:不显示
            ///
            /// 说明:
            /// 假设显示帧率为2, 显示间隔就是500ms, 2秒内显示4帧
            /// 假设采集帧率为2.5(为了显示特殊情况, 实际不存在浮点数的帧率), 采集间隔为400ms, 2秒内采集5帧
            /// 采集5显示4, 所以需要丢弃1帧, 判断步骤如下:
            /// 1) 当收到第1帧时, 直接显示
            /// 2) 之后的每1帧, 计算它与第1帧的时间差a, a%=500(500是显示间隔), 得到它在一个显示限时段内的位置
            ///    也就是它在上一个显示时间点的距离b1和下一个显示时间点的距离b2, 图中记为(b1, b2)
            ///    比如第3帧与第1帧的时间差是800, 800%500=300,
            ///    所以它与上一个显示时间点距离是300, 与下一个显示时间点的距离是500-300=200, 即(300,200)
            /// 3) 将b1,b2与采集帧率c的一半比较
            ///    显示条件: b1 <= c/2 || b2 < c/2
            ///    比如示例中c=400, c/2=200
            ///    第2帧 b1=400, b2=100, 满足b2 < c/2, 显示
            ///    第3帧 b1=300, b2=200, 不满足条件, 不显示
            ///    第4帧 b1=200, b2=300, 满足b1 <= c/2, 显示
            ///

            if (m_forceDisplay)
            {
                return true;
            }

            double displayInterval = 0;
            double timestampInterval = 0;

            lock (lockObj)
            {
                displayInterval = m_displayInterval;
                timestampInterval = m_timestampInterval;
            }

            // 不显示
            if (displayInterval <= 0)
            {
                return false;
            }

            // 时间戳频率获取失败, 默认全显示. 这种情况理论上不会出现
            if (timestampInterval <= 0)
            {
                return true;
            }

            // 第一帧必须显示
            if (m_firstFrameTime == 0 || m_lastFrameTime == 0)
            {
                m_firstFrameTime = timestamp;
                m_lastFrameTime = timestamp;
                return true;
            }

            // 当前时间戳比之前保存的小
            if (timestamp < m_firstFrameTime)
            {
                m_firstFrameTime = timestamp;
                m_lastFrameTime = timestamp;
                return true;
            }

            // 当前帧和上一帧的时间间隔, 即采集间隔
            double deltaToLastFrame = (timestamp - m_lastFrameTime) * timestampInterval;

            // 两帧间隔超过显示间隔, 显示
            if (deltaToLastFrame >= displayInterval)
            {
                // 保存最后一帧的时间戳
                m_lastFrameTime = timestamp;
                return true;
            }

            // 当前帧相对于第一帧的时间间隔
            double deltaToFirstFrame = (timestamp - m_firstFrameTime) * timestampInterval;

            // 每隔一段时间更新起始时间
            if (deltaToFirstFrame > 1000 * 60 * 30)
            {
                m_firstFrameTime = timestamp;
            }

            // 保存最后一帧的时间戳
            m_lastFrameTime = timestamp;

            // 当前帧时间点落在显示间隔的位置
            double timeInDisplayInterval =  deltaToFirstFrame % displayInterval;

            if ((timeInDisplayInterval * 2 <= deltaToLastFrame)
                || ((displayInterval - timeInDisplayInterval) * 2 < deltaToLastFrame))
            {
                return true;
            }
            return false;
        }
    }
}
