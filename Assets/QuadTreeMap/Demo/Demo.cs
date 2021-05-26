using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    public class Demo : MonoBehaviour
    {
        public Bounds bounds;
        private Tree tree;
        [SerializeField] int objCount = 10000;
        [SerializeField] float viewRatio = 1;

        // Start is called before the first frame update
        void Start()
        {
            tree = new Tree(bounds);
            for (int i = 0; i < objCount; i++)
            {
                Vector3 randomPosition = new Vector3(Random.Range(-1000f, 1000f), 0, Random.Range(-1000f, 1000f));
                Vector3 randomScale = Vector3.one * Random.Range(0.5f, 2f);
                ObjData objData = new ObjData("Cube", randomPosition, Quaternion.identity, randomScale, Vector3.one);
                objData.uid = i;
                tree.InsertObjData(objData);
            }
        }

        // Update is called once per frame
        void Update()
        {
            tree.viewRatio = viewRatio;
            tree.Inside(Camera.main);
        }


        private void OnDrawGizmos()
        {
            if (tree != null)
            {
                tree.DrawBound();
            }
            else
            {
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }

}