using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GeoSharpi.Utils;
using RDFSharp.Model;

namespace GeoSharpi.Nodes
{
    [System.Serializable]
    public class Texture3DNode : Node
    {
        public Texture3D texture3d;
        [RDFUri("v4d", "https://w3id.org/v4d/core#",_type: RDFModelEnums.RDFDatatypes.XSD_INT)]
        public int voxelResolution = 64;
        
        [Header("Visual Parameters")]
        public float displayDistance = 1;
        public Shader meshShader;

        public Texture3DNode() { CreateEmptyNode(); }

        public Texture3DNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        public Texture3DNode(Texture3D texture, Matrix4x4 origin)
        {
            texture3d = texture;
            cartesianTransform = origin;

            CreateNode();
        }

        public override GameObject GetResourceObject()
        {
            GameObject texture3dNode = GameObject.CreatePrimitive(PrimitiveType.Cube);

            if (!texture3d)
            {
                LoadResource(path);
                if (!texture3d)
                {
                    Debug.LogWarning("No ImageTexture is provided. Creating placeholder cube");
                    return texture3dNode;
                }
                
            }
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(voxelResolution));
            texture3dNode.transform.localScale = new Vector3(voxelResolution,voxelResolution, gridSize * gridSize)/voxelResolution;
            MeshRenderer meshRenderer = texture3dNode.GetComponent<MeshRenderer>();
            if (!meshShader) meshShader = Shader.Find("Unlit/volumeShader"); //uses the 3D texture shader
            meshRenderer.material = new Material(meshShader);
            meshRenderer.material.SetTexture("_MainTex", texture3d);
            
            return texture3dNode;
        }
        public override void LoadResource(string folderPath)
        {
            Debug.Log("Looking for 3D texture at:" + Path.Combine(folderPath, path));
            byte[] fileData = File.ReadAllBytes(Path.Combine(folderPath, path));
            Texture2D tiledTexture = new Texture2D(2, 2);
            tiledTexture.LoadImage(fileData);
            if (tiledTexture != null)
            {
                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(voxelResolution));
                texture3d = ConvertTiled2DTo3D(tiledTexture, voxelResolution, voxelResolution, gridSize * gridSize);
                Debug.Log("Created3DTexture");
            }
        }

        public override void SaveResource(string rootFolder = "")
        {
            if (rootFolder == "") rootFolder = Application.persistentDataPath;
            string relativePath = GetName() + ".png";
            string savePath = Path.Combine(rootFolder, relativePath);
            SaveTexture3DAsTiled2D(texture3d, savePath);
            path = relativePath;
        }

        public Texture3D ConvertTiled2DTo3D(Texture2D tiledTexture, int width, int height, int depth)
        {
            if (tiledTexture == null)
            {
                Debug.LogError("Tiled texture is null!");
                return null;
            }

            if (!tiledTexture.isReadable)
            {
                Debug.LogError("Texture is not readable. Enable 'Read/Write' in import settings.");
                return null;
            }

            Texture3D texture3D = new Texture3D(width, height, depth, TextureFormat.RGBA32, false);
            Color[] colors = tiledTexture.GetPixels();
            Color[] volumeColors = new Color[width * height * depth];
            int offset = depth - height;

            int tiles = tiledTexture.width / width;

            for (int z = offset; z < depth; z++) // use the offset to remove empty squares because the image must be a 
            {
                int tileX = z % tiles;
                int tileY = tiles - 1 - (z / tiles); // Flip Y-axis

                int startX = tileX * width;
                int startY = tileY * height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int flippedY = height - 1 - y; // Flip Y-axis for pixel indexing
                        int srcIndex = (startY + flippedY) * tiledTexture.width + (startX + x);
                        int dstIndex = z * width * height + y * width + x;
                        volumeColors[dstIndex] = colors[srcIndex];
                    }
                }
            }

            texture3D.SetPixels(volumeColors);
            texture3D.Apply();
            Debug.Log($"Texture3D created with dimensions: {texture3D.width}x{texture3D.height}x{texture3D.depth}");

            return texture3D;
        }
        

        public void SaveTexture3DAsTiled2D(Texture3D texture3D, string path)
        {
            int width = texture3D.width;
            int height = texture3D.height;
            int depth = texture3D.depth;

            int tiles = Mathf.CeilToInt(Mathf.Sqrt(depth));

            Texture2D tiledTexture = new Texture2D(width * tiles, height * tiles, TextureFormat.RGBA32, false);
            Color[] colors3D = texture3D.GetPixels();
            Color[] colors2D = new Color[tiledTexture.width * tiledTexture.height];

            for (int z = 0; z < depth; z++)
            {
                int tileX = z % tiles;
                int tileY = tiles - 1 - (z / tiles); // Flip Y-axis for bottom-left origin

                int startX = tileX * width;
                int startY = tileY * height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int flippedY = height - 1 - y; // Flip Y-axis when storing pixels
                        int srcIndex = z * width * height + y * width + x;
                        int dstIndex = (startY + flippedY) * tiledTexture.width + (startX + x);
                        colors2D[dstIndex] = colors3D[srcIndex];
                    }
                }
            }

            tiledTexture.SetPixels(colors2D);
            tiledTexture.Apply();

            byte[] bytes = tiledTexture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Debug.Log("Saved Texture3D as Tiled 2D PNG: " + path);
        }


    }
}
