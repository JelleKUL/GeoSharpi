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

        [Tooltip("Set each component to 1 or -1 to flip the voxel array read order along that axis. Useful when a 3D texture/JSON loads upside down or mirrored.")]
        public Vector3Int axisFlip = Vector3Int.one;

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
        private Vector3Int _axisFlip;

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

            // 5. Scale vertices to voxel size, then shift so the mesh origin
            // sits at the bottom-center of the voxel grid rather than a
            // corner: X and Z are centered, Y is left at the bottom (0).
            // Grid spans index [0, dim-1] per axis, so half-extent is
            // (dim - 1) * 0.5 * voxelSize.
            float halfWidth = (array.Width - 1) * 0.5f * voxelSize;
            float halfDepth = (array.Depth - 1) * 0.5f * voxelSize;

            for (int i = 0; i < verts.Count; i++)
            {
                Vector3 v = verts[i] * voxelSize;
                v.x -= halfWidth;
                v.z -= halfDepth;
                verts[i] = v;
            }

            // 6. Create the mesh with colors
            CreateMesh(verts, normals, indices, vertColors);
        }

        /// <summary>
        /// Resolves an axisFlip component (expected to be 1 or -1) to a safe sign,
        /// defaulting to +1 (no flip) for 0 or any other unexpected value.
        /// </summary>
        private static int FlipSign(int flipComponent)
        {
            return flipComponent < 0 ? -1 : 1;
        }

        /// <summary>
        /// Maps a destination index along one axis to the corresponding source
        /// index, given that axis's flip sign. When flip is +1, source == dest.
        /// When flip is -1, the axis is read back-to-front (dim-1-dest).
        /// </summary>
        private static int FlippedSourceIndex(int destIndex, int dim, int flipSign)
        {
            return flipSign == 1 ? destIndex : (dim - 1 - destIndex);
        }

        /// <summary>
        /// Converts a Texture3D to a VoxelArray (with values + colors).
        /// Texture3D.GetPixels() returns a flat, randomly-addressable array, so the
        /// flip is applied by choosing which source index to read from for each
        /// destination voxel, per the axisFlip setting.
        /// </summary>
        private VoxelArray Texture3DToVoxelArray(Texture3D tex)
        {
            int width = tex.width;
            int height = tex.height;
            int depth = tex.depth;

            int flipX = FlipSign(axisFlip.x);
            int flipY = FlipSign(axisFlip.y);
            int flipZ = FlipSign(axisFlip.z);

            VoxelArray voxels = new VoxelArray(width, height, depth);
            Color[] colors = tex.GetPixels();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        int sx = FlippedSourceIndex(x, width, flipX);
                        int sy = FlippedSourceIndex(y, height, flipY);
                        int sz = FlippedSourceIndex(z, depth, flipZ);

                        int idx = sx + sy * width + sz * width * height;

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
        /// <summary>
        /// Converts JSON voxel data to a VoxelArray.
        /// Unlike the Texture3D path, this source is a sequential stream
        /// (consumed via i++/c++ in fixed x,y,z loop order), so it cannot be
        /// read out of order. Instead, the flip is applied to the destination
        /// index: values are still consumed in original stream order, but each
        /// one is written into its flipped position in the output array.
        /// </summary>
        private VoxelArray JsonToVoxelArray(TextAsset jsonText)
        {
            var data = JsonUtility.FromJson<VoxelData>(jsonText.text);
            float[,,] voxels = new float[data.width, data.height, data.depth];
            Color[,,] colors = new Color[data.width, data.height, data.depth];

            int flipX = FlipSign(axisFlip.x);
            int flipY = FlipSign(axisFlip.y);
            int flipZ = FlipSign(axisFlip.z);

            int i = 0;
            int c = 0;

            for (int x = 0; x < data.width; x++)
                for (int y = 0; y < data.height; y++)
                    for (int z = 0; z < data.depth; z++)
                    {
                        int dx = FlippedSourceIndex(x, data.width, flipX);
                        int dy = FlippedSourceIndex(y, data.height, flipY);
                        int dz = FlippedSourceIndex(z, data.depth, flipZ);

                        voxels[dx, dy, dz] = -data.voxels[i++];

                        colors[dx, dy, dz] = new Color(
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

            bool flipChanged = _axisFlip != axisFlip;
            bool textureChanged = _voxelTexture != voxelTexture;
            bool jsonChanged = _jsonText != jsonText;

            if (voxelTexture != null && (textureChanged || flipChanged))
            {
                print("Updating Texture3d voxelarray");
                // 1. Convert Texture3D to VoxelArray (values + colors)
                voxelArray = Texture3DToVoxelArray(voxelTexture);
                _voxelTexture = voxelTexture;
            }
            else if (jsonText != null && (jsonChanged || flipChanged))
            {
                print("Updating jsonVoxelArray");
                // 1. Convert Texture3D to VoxelArray (values + colors)
                voxelArray = JsonToVoxelArray(jsonText);
                _jsonText = jsonText;
            }
            _axisFlip = axisFlip;
            if (voxelTexture != null || jsonText != null) UpdateMeshFromVoxelArray(voxelArray);

        }
#endif
    }
}