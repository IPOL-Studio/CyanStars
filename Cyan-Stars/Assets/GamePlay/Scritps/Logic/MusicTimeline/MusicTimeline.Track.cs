using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MusicTimeline
{
    /// <summary>
    /// 轨道类型
    /// </summary>
    public enum TrackType
    {
        Lrc,
    }
    
    /// <summary>
    /// 轨道
    /// </summary>
    public class Track
    {
        public Track(TrackType type)
        {
            Type = type;
        }

        private List<BaseNode> nodes = new List<BaseNode>();

        /// <summary>
        /// 当前索引
        /// </summary>
        private int curIndex;
        
        /// <summary>
        /// 轨道类型
        /// </summary>
        public TrackType Type
        {
            get;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(BaseNode node)
        {
            nodes.Add(node);
        }

        
        public void OnUpdate(float curTime,float deltaTime)
        {
            if (curIndex == nodes.Count)
            {
                return;
            }
            
            BaseNode curNode = nodes[curIndex];
            if (curTime >= curNode.StartTime)
            {
                if (!curNode.IsEnd)
                {
                    curNode.OnUpdate(curTime,deltaTime);
                }

                if (curNode.IsEnd)
                {
                    //当前节点执行完毕
                    curIndex++;
                }
            }
        }


    }

}
