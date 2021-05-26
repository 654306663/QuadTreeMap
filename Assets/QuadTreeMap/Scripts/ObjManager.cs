using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    public enum SceneObjStatus
    {
        Loading,    //加载中
        Loaded  //加载完毕
    }


    public class SceneObjData
    {
        public ObjData objData;
        public SceneObjStatus status;
        public GameObject obj;

        public SceneObjData(ObjData objData)
        {
            this.objData = objData;
            this.obj = null;
        }
    }

    public class ObjManager : MonoBehaviour
    {
        public static ObjManager Instance;
        private Dictionary<int, SceneObjData> activeSceneObjDatas = new Dictionary<int, SceneObjData>();
        private List<int> unloadUids = new List<int>();
        private void Awake()
        {
            Instance = this;
        }

        public void LoadAsync(ObjData objData)
        {
            if (activeSceneObjDatas.ContainsKey(objData.uid))
                return;
            StartCoroutine(LoadObj(objData));
        }

        public void Unload(int uid)
        {
            if (activeSceneObjDatas.ContainsKey(uid) && unloadUids.Contains(uid) == false)
            {
                unloadUids.Add(uid);
            }
            for (int i = 0; i < unloadUids.Count; i++)
            {
                if (activeSceneObjDatas[unloadUids[i]].status == SceneObjStatus.Loaded)
                {
                    Destroy(activeSceneObjDatas[unloadUids[i]].obj);
                    activeSceneObjDatas.Remove(unloadUids[i]);
                    unloadUids.RemoveAt(i--);
                }
            }
        }

        private IEnumerator LoadObj(ObjData obj)
        {
            SceneObjData sceneObjData = new SceneObjData(obj);
            sceneObjData.status = SceneObjStatus.Loading;
            activeSceneObjDatas.Add(obj.uid, sceneObjData);
            GameObject resObj = null;
            ResourceRequest request = Resources.LoadAsync<GameObject>(obj.resPath);
            yield return request;
            resObj = request.asset as GameObject;
            yield return new WaitUntil(() => resObj != null);

            sceneObjData.status = SceneObjStatus.Loaded;
            SetObjTransfrom(resObj, sceneObjData);
        }

        private void SetObjTransfrom(GameObject prefab, SceneObjData sceneObj)
        {
            sceneObj.obj = Instantiate(prefab);
            sceneObj.obj.transform.position = sceneObj.objData.pos;
            sceneObj.obj.transform.rotation = sceneObj.objData.rot;
            sceneObj.obj.transform.localScale = sceneObj.objData.scale;
        }
    }

}