using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    public interface INode
    {
        Bounds bound { get; set; }
        /// <summary>
        /// 初始化插入一个场景物体
        /// </summary>
        /// <param name="obj"></param>
        void InsertObjData(ObjData obj);
        /// <summary>
        /// 当触发者（主角）在该节点里时显示物体
        /// </summary>
        /// <param name="camera"></param>
        void Inside(Camera camera);
        /// <summary>
        /// 当触发者（主角）不在该节点里时隐藏物体
        /// </summary>
        /// <param name="camera"></param>
        void Outside(Camera camera);
        void DrawBound();
    }
}