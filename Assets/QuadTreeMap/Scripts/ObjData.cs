using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    [System.Serializable]
    public class ObjData
    {
        public int uid; //独一无二的id Game用
        [SerializeField]
        public string resPath;  //prefab路径
        [SerializeField]
        public Vector3 pos; //位置
        [SerializeField]
        public Quaternion rot; //旋转
        [SerializeField]
        public Vector3 scale;   //缩放
        [SerializeField]
        public Vector3 size;   //模型尺寸
        public ObjData(string resPath, Vector3 position, Quaternion rotation, Vector3 localScale, Vector3 modelSize)
        {
            this.resPath = resPath;
            this.pos = position;
            this.rot = rotation;
            this.scale = localScale;
            this.size = modelSize;
        }

        public Bounds GetObjBounds()
        {
            return new Bounds(pos, new Vector3(scale.x * size.x, scale.y * size.y, scale.z * size.z));
        }
    }

    [System.Serializable]
    public class ObjDataContainer
    {
        [SerializeField]
        public ObjData[] objDatas;
    }
}