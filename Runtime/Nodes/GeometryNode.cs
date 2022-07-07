using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;

namespace GeoSharpi
{
    [System.Serializable]
    public class GeometryNode : Node
    {
        public Mesh mesh;

        [Header("Visual Parameters")]
        public float displayDistance = 1;
        public Shader meshShader;

        public GeometryNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);


        }

        public override GameObject GetResourceObject()
        {
            GameObject GeometryChild = new GameObject();

            GeometryChild.AddComponent<MeshFilter>();
            GeometryChild.AddComponent<MeshRenderer>();

            MeshFilter meshFilter = GeometryChild.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            meshFilter.sharedMesh.RecalculateBounds();

            MeshRenderer meshRenderer = GeometryChild.GetComponent<MeshRenderer>();
            meshRenderer.material = new Material(meshShader);

            return GeometryChild;
        }
    }
}

