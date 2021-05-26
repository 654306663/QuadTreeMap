using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorSettings;

namespace WCC.QuadTree
{
    public class CreateMapDataEditor : EditorWindow
    {
        [MenuItem("Tools/Assets/四叉树地图数据生成工具")]
        public static void Open()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(CreateMapDataEditor), false, "四叉树地图数据生成工具");
            window.minSize = new Vector2(900, 500);
        }

        private Color defaultGUIColor;
        private TextAnchor defaultTextAnchor;
        private int stepNum = 1;
        private void OnGUI()
        {
            defaultGUIColor = GUI.color;
            defaultTextAnchor = GUI.skin.label.alignment;

            switch (stepNum)
            {
                case 1:
                    GUILayout.BeginHorizontal("HelpBox");
                    GUILayout.Label("四叉树地图数据生成工具，用于批量将地图中物体提取预设体，并生成对应地图数据\n教程：1.在Hierarchy面板选择物体 2.补全未填写的模型原始尺寸（一般为Bounds.size，并非LocalScale）、预设体路径 3.保证每个物体都有预设体 4.点击下一步");
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    SelectGameObjectsView();
                    break;
                case 2:
                    EditorGUILayout.Space();
                    CreateMapDataView();
                    break;
            }
            GUI.color = defaultGUIColor;
            GUI.skin.label.alignment = defaultTextAnchor;
        }

        #region 选择要生成地图数据的物体
        private string searchObjName = "";
        private Vector2 allGameObjectScrollPosition;
        private List<ObjPrefabPathData> selectGameObjectDatas = new List<ObjPrefabPathData>();
        private List<ObjPrefabPathData> searchGameObjectDatas = new List<ObjPrefabPathData>();
        private string prefabPathHead = "Assets/";
        private bool allDoForceOverride = false;    //执行全部补全路径时 是否强制覆盖
        private void SelectGameObjectsView()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            searchObjName = EditorGUILayout.TextField("", searchObjName, "SearchTextField", GUILayout.MinWidth(450));
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log(searchObjName + " " + string.IsNullOrEmpty(searchObjName));
                SearchGameObjectName();
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("已选择物体数量：" + selectGameObjectDatas.Count);
            EditorGUILayout.EndHorizontal();

            if (searchGameObjectDatas.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("", GUILayout.Width(18));
                    GUILayout.Label("物体", GUILayout.Width(260));
                    GUILayout.Label("模型尺寸", GUILayout.Width(220));
                    GUILayout.Label("预设体路径");
                    GUI.skin.label.alignment = defaultTextAnchor;
                }
                EditorGUILayout.EndHorizontal();
                allGameObjectScrollPosition = EditorGUILayout.BeginScrollView(allGameObjectScrollPosition);
                for (int i = 0; i < searchGameObjectDatas.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(EditorGUIUtility.FindTexture("TreeEditor.Trash"), new GUIStyle(EditorStyles.toolbarButton), GUILayout.Width(25)))
                        {
                            selectGameObjectDatas.Remove(searchGameObjectDatas[i]);
                            searchGameObjectDatas.Remove(searchGameObjectDatas[i]);
                            break;
                        }
                        EditorGUILayout.ObjectField(searchGameObjectDatas[i].obj, typeof(GameObject), true, GUILayout.Width(260));
                        searchGameObjectDatas[i].objModelSize = EditorGUILayout.Vector3Field("", searchGameObjectDatas[i].objModelSize);
                        if (string.IsNullOrEmpty(searchGameObjectDatas[i].objPrefabPath) || searchGameObjectDatas[i].objPrefabPath.EndsWith(".prefab") == false)
                            GUI.color = Color.red;
                        searchGameObjectDatas[i].objPrefabPath = GUILayout.TextField(searchGameObjectDatas[i].objPrefabPath, GUILayout.MinWidth(260), GUILayout.ExpandWidth(true));
                        if (string.IsNullOrEmpty(searchGameObjectDatas[i].objPrefabPath) || searchGameObjectDatas[i].objPrefabPath.EndsWith(".prefab") == false)
                            GUI.color = defaultGUIColor;

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("补全路径", GUILayout.Width(60)))
                        {
                            FillPrefabPath(searchGameObjectDatas[i]);
                        }
                        if (GUILayout.Button("生成预设体", GUILayout.Width(75)))
                        {
                            CreatePrefab(searchGameObjectDatas[i]);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("预设体路径前缀：", GUILayout.Width(120));
                prefabPathHead = EditorGUILayout.TextField(prefabPathHead, GUILayout.Width(280));
                GUILayout.FlexibleSpace();
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(prefabPathHead) || selectGameObjectDatas.Count == 0);
                allDoForceOverride = GUILayout.Toggle(allDoForceOverride, "覆盖");
                if (GUILayout.Button("全部补全尺寸"))
                {
                    searchObjName = "";
                    AllFillModelSizes();
                }
                if (GUILayout.Button("全部补全路径"))
                {
                    searchObjName = "";
                    AllFillPrefabPaths();
                }
                if (GUILayout.Button("全部生成预设体"))
                {
                    searchObjName = "";
                    AllCreatePrefabs();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GameObject[] gameObjects = Selection.gameObjects;
            EditorGUI.BeginDisabledGroup(gameObjects.Length == 0);
            if (GUILayout.Button("添加物体"))
            {
                AddGameObjectDatas(gameObjects);
                SearchGameObjectName();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(selectGameObjectDatas.Count == 0);
            if (GUILayout.Button("清空物体"))
            {
                selectGameObjectDatas.Clear();
                SearchGameObjectName();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("下一步"))
            {
                if (IsStepFinish())
                {
                    PreviewMapData();
                    stepNum = 2;
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
        }

        private void AllFillModelSizes()
        {
            for (int i = 0; i < selectGameObjectDatas.Count; i++)
            {
                if (allDoForceOverride == false)
                    if (selectGameObjectDatas[i].objModelSize == default)
                        continue;
                if (selectGameObjectDatas[i].obj.transform.childCount == 0)
                {
                    if (selectGameObjectDatas[i].obj.GetComponent<MeshFilter>() != null && selectGameObjectDatas[i].obj.GetComponent<MeshFilter>().sharedMesh != null)
                    {
                        selectGameObjectDatas[i].objModelSize = selectGameObjectDatas[i].obj.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                    }
                    if (selectGameObjectDatas[i].obj.GetComponent<SpriteRenderer>() != null)
                    {
                        Texture2D texture = selectGameObjectDatas[i].obj.GetComponent<SpriteRenderer>().sprite.texture;
                        if (texture != null)
                            selectGameObjectDatas[i].objModelSize = new Vector3(texture.width, texture.height, 0);
                    }
                }
            }
        }

        private void AllFillPrefabPaths()
        {
            for (int i = 0; i < selectGameObjectDatas.Count; i++)
            {
                if (allDoForceOverride == false)
                    if (string.IsNullOrEmpty(selectGameObjectDatas[i].objPrefabPath) || selectGameObjectDatas[i].objPrefabPath.EndsWith(".prefab") == false)
                        continue;
                FillPrefabPath(selectGameObjectDatas[i]);
            }
        }

        private void FillPrefabPath(ObjPrefabPathData quadTreeGameObjectData)
        {
            string objPrefabPath = quadTreeGameObjectData.obj.name;
            switch (EditorSettings.gameObjectNamingScheme)
            {
                case NamingScheme.SpaceParenthesis:
                    if (objPrefabPath.Contains(" ("))
                        objPrefabPath = Regex.Split(objPrefabPath, " ()", RegexOptions.IgnoreCase)[0];
                    break;
                case NamingScheme.Dot:
                    if (objPrefabPath.IndexOf('.') != -1)
                        objPrefabPath = objPrefabPath.Split('.')[0];
                    break;
                case NamingScheme.Underscore:
                    if (objPrefabPath.IndexOf('_') != -1)
                        objPrefabPath = objPrefabPath.Split('_')[0];
                    break;
            }
            if (prefabPathHead[prefabPathHead.Length - 1] != '/')
                prefabPathHead += '/';
            quadTreeGameObjectData.objPrefabPathSimple = objPrefabPath;
            quadTreeGameObjectData.objPrefabPath = prefabPathHead + objPrefabPath + ".prefab";
        }

        private void AllCreatePrefabs()
        {
            List<string> createdPrefabs = new List<string>();
            int num = 0;
            for (int i = 0; i < selectGameObjectDatas.Count; i++)
            {
                EditorUtility.DisplayProgressBar("正在生成预设体……", "", (i + 1f) / selectGameObjectDatas.Count);
                if (string.IsNullOrEmpty(selectGameObjectDatas[i].objPrefabPath) || selectGameObjectDatas[i].objPrefabPath.EndsWith(".prefab") == false)
                    continue;
                if (createdPrefabs.Contains(selectGameObjectDatas[i].objPrefabPath))
                    continue;
                bool succeeded = CreatePrefab(selectGameObjectDatas[i]);
                if (succeeded)
                {
                    num++;
                    createdPrefabs.Add(selectGameObjectDatas[i].objPrefabPath);
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("全部生成预设体完成，数量：" + num);
        }

        private bool CreatePrefab(ObjPrefabPathData quadTreeGameObjectData)
        {
            if (File.Exists(quadTreeGameObjectData.objPrefabPath))
            {
                Debug.LogError("生成预设失败，该目录已存在预设：" + quadTreeGameObjectData.objPrefabPath);
                return false;
            }
            PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(quadTreeGameObjectData.obj);

            switch (status)
            {
                case PrefabInstanceStatus.Connected:
                case PrefabInstanceStatus.NotAPrefab:
                case PrefabInstanceStatus.Disconnected:
                    {
                        bool succeeded;
                        PrefabUtility.SaveAsPrefabAsset(quadTreeGameObjectData.obj, quadTreeGameObjectData.objPrefabPath, out succeeded);
                        if (succeeded)
                            Debug.Log("生成预设成功：" + quadTreeGameObjectData.objPrefabPath);
                        else
                            Debug.LogError("生成预设失败：" + quadTreeGameObjectData.objPrefabPath);
                    }
                    break;
                case PrefabInstanceStatus.MissingAsset:
                    {
                        PrefabUtility.UnpackPrefabInstance(quadTreeGameObjectData.obj, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                        bool succeeded;
                        PrefabUtility.SaveAsPrefabAsset(quadTreeGameObjectData.obj, quadTreeGameObjectData.objPrefabPath, out succeeded);
                        if (succeeded)
                            Debug.Log("生成预设成功：" + quadTreeGameObjectData.objPrefabPath);
                        else
                            Debug.LogError("生成预设失败：" + quadTreeGameObjectData.objPrefabPath);
                    }
                    break;
            }
            return true;
        }

        private void AddGameObjectDatas(GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i].activeSelf == false)
                    continue;
                bool isRepeat = false;
                foreach (var item in selectGameObjectDatas)
                {
                    if (gameObjects[i] == item.obj)
                    {
                        isRepeat = true;
                        break;
                    }
                }
                if (isRepeat)
                {
                    Debug.Log("有选择重复物体，已跳过该物体：" + gameObjects[i].name);
                    continue;
                }
                selectGameObjectDatas.Add(new ObjPrefabPathData(gameObjects[i]));
            }
        }

        private void SearchGameObjectName()
        {
            if (string.IsNullOrEmpty(searchObjName))
            {
                searchGameObjectDatas.Clear();
                searchGameObjectDatas.AddRange(selectGameObjectDatas);
            }
            else
            {
                searchGameObjectDatas.Clear();
                for (int i = 0; i < selectGameObjectDatas.Count; i++)
                {
                    if (selectGameObjectDatas[i].obj.name.Contains(searchObjName))
                        searchGameObjectDatas.Add(selectGameObjectDatas[i]);
                }
            }
        }

        private bool IsStepFinish()
        {
            bool isFinish = true;
            for (int i = 0; i < selectGameObjectDatas.Count; i++)
            {
                if (string.IsNullOrEmpty(selectGameObjectDatas[i].objPrefabPath) || selectGameObjectDatas[i].objPrefabPath.EndsWith(".prefab") == false)
                {
                    Debug.LogError("还有未补全的资源路径：" + selectGameObjectDatas[i].obj.name);
                    isFinish = false;
                }
                // if (selectGameObjectDatas[i].objModelSize == default)
                // {
                //     Debug.LogError("还有未填写的模型尺寸：" + selectGameObjectDatas[i].obj.name);
                //     isFinish = false;
                // }
            }
            return isFinish;
        }

        #endregion

        #region 生成地图数据
        private string mapDataPath = "Assets/mapData.json";
        private bool mapDataPrefabPathSimple = false;   //生成出的地图数据 是否预设路径简写，若简写 则仅用名称
        private ObjDataContainer objDataContainer;
        private Vector2 previewMapDataScrollPosition;
        private string mapData = "";
        private void PreviewMapData()
        {
            objDataContainer = new ObjDataContainer();
            objDataContainer.objDatas = new ObjData[selectGameObjectDatas.Count];
            if (mapDataPrefabPathSimple)
            {
                for (int i = 0; i < selectGameObjectDatas.Count; i++)
                {
                    ObjData objData = new ObjData(selectGameObjectDatas[i].objPrefabPathSimple, selectGameObjectDatas[i].obj.transform.position, selectGameObjectDatas[i].obj.transform.rotation, selectGameObjectDatas[i].obj.transform.localScale, selectGameObjectDatas[i].objModelSize);
                    objDataContainer.objDatas[i] = objData;
                }
            }
            else
            {
                for (int i = 0; i < selectGameObjectDatas.Count; i++)
                {
                    ObjData objData = new ObjData(selectGameObjectDatas[i].objPrefabPath, selectGameObjectDatas[i].obj.transform.position, selectGameObjectDatas[i].obj.transform.rotation, selectGameObjectDatas[i].obj.transform.localScale, selectGameObjectDatas[i].objModelSize);
                    objDataContainer.objDatas[i] = objData;
                }
            }
            mapData = JsonUtility.ToJson(objDataContainer);
        }

        private void CreateMapDataView()
        {
            // GUILayout.Label("地图数据生成路径：", GUILayout.Width(120));
            mapDataPath = EditorGUILayout.TextField("地图数据生成路径：", mapDataPath);
            EditorGUI.BeginChangeCheck();
            mapDataPrefabPathSimple = EditorGUILayout.Toggle("预设路径是否简写：", mapDataPrefabPathSimple);
            if (EditorGUI.EndChangeCheck())
            {
                PreviewMapData();
            }

            EditorGUILayout.Space();
            previewMapDataScrollPosition = EditorGUILayout.BeginScrollView(previewMapDataScrollPosition);
            mapData = GUILayout.TextArea(mapData);
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            EditorGUILayout.Space();
            if (GUILayout.Button("上一步"))
            {
                stepNum = 1;
            }
            if (GUILayout.Button("生成地图数据"))
            {
                TextWriter tw = new StreamWriter(mapDataPath, false);
                tw.Write(mapData);
                tw.Close();
                Debug.Log("生成地图数据完成 path:" + mapDataPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.Space();
        }

        #endregion
    }

}