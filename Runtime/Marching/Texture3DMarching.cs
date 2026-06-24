using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GeoSharpi.Marching;
using System.IO;
namespace GeoSharpi.Marching
{
    public enum MARCHING_MODE { CUBES, COMPACTCUBES, TETRAHEDRON }

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Texture3DMarcher : MonoBehaviour
    {
        [Header("Marching Cubes Settings")]
        public MARCHING_MODE mode = MARCHING_MODE.CUBES;
        [Range(0, 1)]
        public float surfaceThreshold = 0.5f;
        public bool smoothNormals = true;

        [Header("Voxel Settings")]
        public float voxelSize = 1f;  // Size of each voxel in world units

        [Header("Rendering")]
        public Material material;

        [Header("3D Texture Input")]
        public Texture3D voxelTexture;

        [Header("Json Input")]
        public TextAsset jsonText;
        public VoxelArray voxelArray;
        public bool updateContinuous = true;
        private Texture3D _voxelTexture;
        private TextAsset _jsonText;

        void Start()
        {
            if (voxelTexture == null)
            {
                Debug.LogWarning("No Texture3D assigned for marching cubes.");
                return;
            }
            voxelArray = Texture3DToVoxelArray(voxelTexture);
        }
        /// <summary>
        /// Generate the mesh from the assigned 3D texture
        /// </summary>
        public void UpdateMeshFromVoxelArray(VoxelArray array)
        {

            if (array == null)
            {
                Debug.LogWarning("No VoxelArray assigned for marching cubes.");
                return;
            }

            // 2. Choose marching algorithm
            Marching marching = mode switch
            {
                MARCHING_MODE.TETRAHEDRON => new MarchingTertrahedron(),
                MARCHING_MODE.COMPACTCUBES => new CompactMarchingCubes(),
                _ => new MarchingCubes()
            };
            marching.Surface = surfaceThreshold;

            // 3. Prepare mesh data
            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Color> vertColors = new List<Color>();
            List<Vector3> normals = new List<Vector3>(); // optional, can recalc later

            // 4. Generate mesh with color support
            marching.Generate(array.Voxels, array.colors, verts, indices, vertColors);

            // 5. Scale vertices to voxel size
            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] *= voxelSize;
            }

            // 6. Create the mesh with colors
            CreateMesh(verts, normals, indices, vertColors);
        }

        /// <summary>
        /// Converts a Texture3D to a VoxelArray (with values + colors)
        /// </summary>
        private VoxelArray Texture3DToVoxelArray(Texture3D tex)
        {
            int width = tex.width;
            int height = tex.height;
            int depth = tex.depth;

            VoxelArray voxels = new VoxelArray(width, height, depth);
            Color[] colors = tex.GetPixels();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        int idx = x + y * width + z * width * height;

                        // Use alpha or grayscale for marching surface value
                        voxels[x, y, z] = colors[idx].a;

                        // Store full color
                        voxels.colors[x, y, z] = colors[idx];
                    }
                }
            }

            return voxels;
        }
        [System.Serializable]
        public class VoxelData
        {
            public int width;
            public int height;
            public int depth;

            public float[] voxels;
            public float[] colors;
        }
        private VoxelArray JsonToVoxelArray(TextAsset jsonText)
        {
            var data = JsonUtility.FromJson<VoxelData>(jsonText.text);
            float[,,] voxels = new float[data.width, data.height, data.depth];
            Color[,,] colors = new Color[data.width, data.height, data.depth];

            int i = 0;
            int c = 0;

            for (int x = 0; x < data.width; x++)
                for (int y = 0; y < data.height; y++)
                    for (int z = 0; z < data.depth; z++)
                    {
                        voxels[x, y, z] = -data.voxels[i++];

                        colors[x, y, z] = new Color(
                            data.colors[c++],
                            data.colors[c++],
                            data.colors[c++],
                            data.colors[c++]
                        );
                    }
            print(colors[10,10,10]);
            VoxelArray newArray =  new VoxelArray(voxels, colors);
            return newArray;
        }

        /// <summary>
        /// Creates a Unity mesh from lists of vertices, normals, indices, and vertex colors
        /// </summary>
        private void CreateMesh(List<Vector3> verts, List<Vector3> normals, List<int> indices, List<Color> colors)
        {
            Mesh mesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };

            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (smoothNormals && normals.Count == verts.Count)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            if (colors != null && colors.Count == verts.Count)
                mesh.SetColors(colors);

            GetComponent<MeshFilter>().mesh = mesh;
            if (material != null)
                GetComponent<Renderer>().material = material;
        }

#if UNITY_EDITOR
        // Optional: automatically generate mesh in editor
        private void OnValidate()
        {
            if (!updateContinuous) return;

            if (_voxelTexture != voxelTexture && voxelTexture != null)
            {
                print("Updating Texture3d voxelarray");
                // 1. Convert Texture3D to VoxelArray (values + colors)
                voxelArray = Texture3DToVoxelArray(voxelTexture);
                _voxelTexture = voxelTexture;
            }
            else if (_jsonText != jsonText && jsonText != null)
            {
                print("Updating jsonVoxelArray");
                // 1. Convert Texture3D to VoxelArray (values + colors)
                voxelArray = JsonToVoxelArray(jsonText);
                _jsonText = jsonText;
            }
            if (voxelTexture != null || jsonText != null) UpdateMeshFromVoxelArray(voxelArray);

        }
#endif
    }
}