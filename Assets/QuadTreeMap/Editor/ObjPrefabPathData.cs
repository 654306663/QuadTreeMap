using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    public class ObjPrefabPathData
    {
        public GameObject obj;
        private ObjPrefabData quadTreeObjPrefabPath;
        public string objPrefabPath
        {
            get
            {
                return quadTreeObjPrefabPath.prefabPath;
            }
            set
            {
                quadTreeObjPrefabPath.prefabPath = value;
            }
        }
        public Vector3 objModelSize
        {
            get
            {
                return quadTreeObjPrefabPath.modelSize;
            }
            set
            {
                quadTreeObjPrefabPath.modelSize = value;
            }
        }
        public string objPrefabPathSimple;
        public ObjPrefabPathData(GameObject obj)
        {
            this.obj = obj;
            quadTreeObjPrefabPath = obj.GetComponent<ObjPrefabData>();
            if (quadTreeObjPrefabPath == null)
                quadTreeObjPrefabPath = obj.AddComponent<ObjPrefabData>();
            if (quadTreeObjPrefabPath.prefabPath == null)
                quadTreeObjPrefabPath.prefabPath = "";
            if (quadTreeObjPrefabPath.prefabPath.IndexOf('/') != -1 && quadTreeObjPrefabPath.prefabPath.IndexOf('.') != -1)
            {
                string[] temp = quadTreeObjPrefabPath.prefabPath.Split('/');
                string[] temp2 = temp[temp.Length - 1].Split('.');
                objPrefabPathSimple = temp2[temp2.Length - 2];
            }
        }
    }
}