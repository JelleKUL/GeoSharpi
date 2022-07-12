using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.IO;

namespace GeoSharpi
{
    [System.Serializable]
    public class GeometryNode : Node
    {
        public Mesh mesh;

        [Header("Visual Parameters")]
        public float displayDistance = 1;
        public Shader meshShader;

        public GeometryNode() { }

        public GeometryNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        public GeometryNode(Mesh geometryMesh, Matrix4x4 origin)
        {
            mesh = geometryMesh;
            cartesianTransform = origin;

            CreateNode();
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
            if (!meshShader) meshShader = Shader.Find("Standard"); //uses the default shader
            meshRenderer.material = new Material(meshShader);

            return GeometryChild;
        }

        public override void SaveResource(string rootFolder = "")
        {
            if (rootFolder == "") rootFolder = Application.persistentDataPath;
            string relativePath = GetName() + ".obj";
            string savepath = Path.Combine(rootFolder, relativePath);
            MeshIO.MeshToFile(mesh, savepath);
            path = relativePath;
        }
    }
}

