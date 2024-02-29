using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GeoSharpi.Capture
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VirtualScanner : MonoBehaviour
    {
        [Header("Scanning")]
        [SerializeField]
        [Tooltip("Update the scan continuously at runtime and onGizmoSelected")]
        private bool updateScan = true;
        [SerializeField]
        [Tooltip("the ratio between the point distance at a given radius")]
        [Min(0)]
        private float scanDensity = 0.01f;
        [SerializeField]
        [Tooltip("The max range of the scanner")]
        [Min(0f)]
        private float range = 10;
        [SerializeField]
        [Tooltip("The total degrees the vertical axis can cover")]
        [Range(0f,360f)]
        private float VerticalScanRange = 290;
        
        [Header("Meshing")]
        [SerializeField]
        [Tooltip("Update the mesh continuously at runtime and onGizmoSelected")]
        private bool updateMesh = true;
        [SerializeField]
        [Tooltip("The max circumference of a single triangle before being skipped")]
        [Min(0)]
        private float maxTraingleLength = 1;

        [Header("Visualisation")]
        [SerializeField]
        bool drawRays = true;
        [SerializeField]
        bool showNoHits = false;
        [SerializeField]
        bool drawPoints = false;
        [SerializeField]
        float pointSize = 0.01f;

        private Mesh mesh;
        private MeshFilter filter;
        private Vector3[,] spherePoints;
        private Vector2[,] sphereUvs;
        private bool updatingMesh = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            UpdateScan();
        }

        [ContextMenu("Update Scan")]
        public void UpdateScan()
        {
            if (!updateScan) return;

            if (updateMesh && !updatingMesh)
            {
                GetScanDirections();
                updatingMesh = true;
                UpdateMesh();
            }
            else if (!updatingMesh)
            {
                GetScanDirections();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) UpdateScan(); // Update the scan outside of play mode

            if (!updateScan) return;

            if (drawPoints && spherePoints != null)
            {
                foreach (var point in spherePoints)
                {
                    if (point != Vector3.negativeInfinity)
                    {
                        Gizmos.DrawSphere(point, pointSize);
                    }
                }
            }
            
        }

        public List<Vector3> GetScanDirections()
        {
            List<Vector3> scanVectors = new List<Vector3>();

            int pointsPerDisc = Mathf.CeilToInt(Mathf.PI * 2 / scanDensity); // the number of horizonontal captured points
            int nrOfDiscs = Mathf.CeilToInt(Mathf.PI / scanDensity); // the number of vertical rows
            spherePoints = new Vector3[nrOfDiscs, pointsPerDisc];
            sphereUvs = new Vector2[nrOfDiscs, pointsPerDisc+1];

            Vector3 vector0 = Vector3.forward; // the starting vector

            for (int i = 0; i < nrOfDiscs; i++)
            {
                for (int j = 0; j < pointsPerDisc; j++)
                {
                    if ((i * scanDensity * Mathf.Rad2Deg) > (360 - VerticalScanRange) / 2) // filter out the bottom unscannable rows
                    {
                        Vector3 newVector = Quaternion.Euler(0, j * scanDensity * Mathf.Rad2Deg, 0) * Quaternion.Euler(90 - i * scanDensity * Mathf.Rad2Deg, 0, 0) * vector0;
                        scanVectors.Add(newVector);

                        // use a raycast to determine the distance to the mesh
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, transform.TransformDirection(newVector), out hit, range))
                        {
                            //float color = hit.distance / range;
                            spherePoints[i, j] = hit.point;
                            sphereUvs[i, j] = new Vector2((j * scanDensity * Mathf.Rad2Deg) / 360, 1 - ((180 - i * scanDensity * Mathf.Rad2Deg) / 180));
                            if (j == 0)
                            {
                                sphereUvs[i, pointsPerDisc] = new Vector2(1,sphereUvs[i, j].y);
                            }

                            // draw the debug rays
                            if (drawRays)
                            {
                                float color = hit.distance / range;
                                Debug.DrawRay(transform.position, transform.TransformDirection(newVector) * hit.distance, new Color(0, color, 1 - color));
                            }

                        }
                        else
                        {
                            spherePoints[i, j] = Vector3.negativeInfinity;
                            if (showNoHits) Debug.DrawRay(transform.position, transform.TransformDirection(newVector) * range, Color.red);
                        }
                    }
                    else spherePoints[i, j] = Vector3.negativeInfinity;
                }
            }

            return scanVectors;
        }
        public async void UpdateMesh()
        {
            await CreateSphereMesh(spherePoints);
            if (!filter) filter = GetComponent<MeshFilter>();
            filter.mesh = mesh;
            filter.sharedMesh.RecalculateBounds();
            updatingMesh = false;
        }

        //generates a mesh
        public async Task<Mesh> CreateSphereMesh(Vector3[,] points)
        {

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            // go over every point in a single ring
            // get the next point in the ring and the point in the above ring
            //Debug.Log(points.Length);

            for (int i = 0; i < points.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    if (points[i, j] == Vector3.zero) Debug.Log((i, j, "Point is zero"));
                    if (points[i, j] == Vector3.negativeInfinity) continue; // if the point itself is Null skip
                    int nextPointIndex = (j + 1) % points.GetLength(1);
                    int upperPointIndex = (i + 1);

                    // create the first triangle 
                    // [1,2]
                    // [0,x]
                    if (points[upperPointIndex, nextPointIndex] != Vector3.negativeInfinity && points[upperPointIndex, j] != Vector3.negativeInfinity) // the three target points are defined
                    {
                        if (Vector3.SqrMagnitude(points[i, j] - points[upperPointIndex, j]) +
                            Vector3.SqrMagnitude(points[i, j] - points[upperPointIndex, nextPointIndex]) +
                            Vector3.SqrMagnitude(points[upperPointIndex, j] - points[upperPointIndex, nextPointIndex]) < maxTraingleLength * maxTraingleLength)
                        {
                            verts.Add(transform.InverseTransformPoint(points[i, j]));
                            uvs.Add(sphereUvs[i, j]);
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[upperPointIndex, j]));
                            uvs.Add(sphereUvs[upperPointIndex, j]);
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[upperPointIndex, nextPointIndex]));
                            uvs.Add(sphereUvs[upperPointIndex, j+1]);
                            tris.Add(verts.Count - 1);
                        }

                    }
                    // create the second triangle 
                    // [x,1]
                    // [0,2]
                    if (points[i, nextPointIndex] != Vector3.negativeInfinity && points[upperPointIndex, nextPointIndex] != Vector3.negativeInfinity) // the three target points are defined
                    {
                        if (Vector3.SqrMagnitude(points[i, j] - points[upperPointIndex, nextPointIndex]) +
                            Vector3.SqrMagnitude(points[i, j] - points[i, nextPointIndex]) +
                            Vector3.SqrMagnitude(points[upperPointIndex, nextPointIndex] - points[i, nextPointIndex]) < maxTraingleLength * maxTraingleLength)
                        {
                            verts.Add(transform.InverseTransformPoint(points[i, j]));
                            uvs.Add(sphereUvs[i, j]);
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[upperPointIndex, nextPointIndex]));
                            uvs.Add(sphereUvs[upperPointIndex, j+1]);
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[i, nextPointIndex]));
                            uvs.Add(sphereUvs[i, j+1]);
                            tris.Add(verts.Count - 1);
                        }
                    }
                }
            }

            mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.name = "scannedSphere";

            return mesh;

        }

    }
}
