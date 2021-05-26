using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    public class Tree : INode
    {
        public Bounds bound { get; set; }
        private Node root;
        public int maxDepth { get; }
        public int maxChildCount { get; }
        public float viewRatio = 1;

        public Tree(Bounds bound)
        {
            this.bound = bound;
            this.maxDepth = 5;
            this.maxChildCount = 4;

            root = new Node(bound, 0, this);
        }

        public void InsertObjData(ObjData obj)
        {
            root.InsertObjData(obj);
        }

        public void Inside(Camera camera)
        {
            root.Inside(camera);
        }

        public void Outside(Camera camera)
        {
            root.Outside(camera);
        }

        public void DrawBound()
        {
            root.DrawBound();
        }
    }
}