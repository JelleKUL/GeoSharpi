using System.Collections;
using System.Collections.Generic;
using System.IO;
using GeoSharpi.Utils;
using UnityEngine;

namespace GeoSharpi.Nodes
{
    [System.Serializable]
    public class MeshNode : Node
    {
        public Mesh mesh;

        [Header("Visual Parameters")]
        public float displayDistance = 1;
        public Shader meshShader;

        public MeshNode() { CreateEmptyNode(); }

        public MeshNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        public MeshNode(Mesh geometryMesh, Matrix4x4 origin)
        {
            mesh = geometryMesh;
            cartesianTransform = origin;

            CreateNode();
        }

        public override GameObject GetResourceObject()
        {
            return MeshIO.LoadMesh(path);
        }
        
        public override void LoadResource(string folderPath)
        {
            Debug.Log("Looking for Mesh at:" + Path.Combine(folderPath, path));
            path = Path.Combine(folderPath, path);
            //mesh = MeshImporter.Load(Path.Combine(folderPath, path)).;
            //mesh = null;
        }


        public override void SaveResource(string rootFolder = "")
        {
            if (rootFolder == "") rootFolder = Application.persistentDataPath;
            string relativePath = GetName() + ".obj";
            string savepath = Path.Combine(rootFolder, relativePath);
            MeshIO.SaveMesh(mesh, savepath);
            path = relativePath;
        }
    }
}
