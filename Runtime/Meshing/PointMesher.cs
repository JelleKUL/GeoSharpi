using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoSharpi.Utils;
using UnityEngine;

public class PointMesher : MonoBehaviour
{

        private Mesh mesh;
        private MeshFilter filter;

                [SerializeField]
        private string meshSavePath = "";

        [Header("Meshing")]
        [SerializeField]
        [Tooltip("Update the mesh continuously at runtime and onGizmoSelected")]
        private bool updateMesh = true;
        [SerializeField]
        [Tooltip("The max circumference of a single triangle before being skipped")]
        [Min(0)]
        private float maxTraingleLength = 1;

        private bool updatingMesh = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public async void UpdateMesh()
    {
        //await CreateSphereMesh();
        if (!filter) filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;
        filter.sharedMesh.RecalculateBounds();
        updatingMesh = false;
    }

    // generates a mesh
    public async Task<Mesh> CreateSphereMesh(Vector3[,] points)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        // maps (i,j) -> index in verts[]
        Dictionary<(int, int), int> vertexLookup = new Dictionary<(int, int), int>();

        int rows = points.GetLength(0);
        int cols = points.GetLength(1);

        // helper to get/create vertex index
        int GetVertexIndex(int i, int j)
        {
            if (vertexLookup.TryGetValue((i, j), out int idx))
                return idx;

            Vector3 v = points[i, j];
            if (v == Vector3.negativeInfinity) return -1; // skip invalid
            if (v.x <= float.NegativeInfinity ||
                v.y <= float.NegativeInfinity ||
                v.z <= float.NegativeInfinity
                )
            {
                return -1; // if the point itself is Null skip
            }
            idx = verts.Count;
            verts.Add(transform.InverseTransformPoint(v));
            //uvs.Add(sphereUvs[i, j]);
            vertexLookup[(i, j)] = idx;
            return idx;
        }

        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (points[i, j] == Vector3.negativeInfinity)
                {
                    continue;
                }
                if (points[i, j].x <= float.NegativeInfinity ||
                    points[i, j].y <= float.NegativeInfinity ||
                    points[i, j].z <= float.NegativeInfinity
                )
                {
                    continue; // if the point itself is Null skip
                }

                int nextJ = (j + 1) % cols;
                int upperI = i + 1;

                int v0 = GetVertexIndex(i, j);
                int v1 = GetVertexIndex(upperI, j);
                int v2 = GetVertexIndex(upperI, nextJ);
                int v3 = GetVertexIndex(i, nextJ);


                // first triangle [1,2] / [0,x]
                if (v0 >= 0 && v1 >= 0 && v2 >= 0)
                {
                    float d = Vector3.SqrMagnitude(points[i, j] - points[upperI, j]) +
                            Vector3.SqrMagnitude(points[i, j] - points[upperI, nextJ]) +
                            Vector3.SqrMagnitude(points[upperI, j] - points[upperI, nextJ]);

                    if (d < maxTraingleLength * maxTraingleLength)
                    {
                        tris.Add(v0);
                        tris.Add(v1);
                        tris.Add(v2);
                    }
                }

                // second triangle [x,1] / [0,2]
                if (v0 >= 0 && v2 >= 0 && v3 >= 0)
                {
                    float d = Vector3.SqrMagnitude(points[i, j] - points[upperI, nextJ]) +
                            Vector3.SqrMagnitude(points[i, j] - points[i, nextJ]) +
                            Vector3.SqrMagnitude(points[upperI, nextJ] - points[i, nextJ]);

                    if (d < maxTraingleLength * maxTraingleLength)
                    {
                        tris.Add(v0);
                        tris.Add(v2);
                        tris.Add(v3);
                    }
                }
            }
        }

        print(verts.Count);
        print(tris.Count);

        mesh = new Mesh
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray(),
            uv = uvs.ToArray(),
            name = "scannedSphere"
        };
        mesh.RecalculateNormals();

        print(mesh.vertexCount);

        return mesh;
    }


    [ContextMenu("Save Mesh")]
    public void SaveMesh()
    {
        if (!filter) filter = GetComponent<MeshFilter>();
        MeshIO.SaveMesh(filter.mesh, meshSavePath);
    }
}
