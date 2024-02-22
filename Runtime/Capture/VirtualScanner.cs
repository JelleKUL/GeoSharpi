using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GeoSharpi.Capture
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VirtualScanner : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("the ratio between the point distance at a given radius")]
        private float scanDensity = 0.01f;
        [SerializeField]
        private float range = 10;
        [SerializeField]
        private float VerticalScanRange = 290;

        [SerializeField]
        private bool updateMesh = true;
        [SerializeField]
        private float maxTraingleLength = 1;

        [SerializeField]
        bool showNoHits = false;
        [SerializeField]
        bool drawRays = true;
        [SerializeField]
        bool drawPoints = false;
        [SerializeField]
        float pointSize = 0.01f;
        [SerializeField]
        bool useObjectColor = true;

        private Mesh mesh;
        private MeshFilter filter;
        private MeshRenderer renderer;
        private Vector3[,] spherePoints;

        private bool updatingMesh = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (updateMesh && !updatingMesh)
            {
                updatingMesh = true;
                UpdateMesh();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (updateMesh && !updatingMesh)
            {
                updatingMesh = true;
                UpdateMesh();
            }

            if (!drawPoints) return;
            if (spherePoints == null) return;
            foreach (var point in spherePoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point, pointSize);
                }
            }

            /*
            List<Vector3> vectors = GetScanDirections();

            foreach (var item in vectors)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(item), out hit, range))
                {
                    float color = hit.distance / range;
                    if(drawRays) Debug.DrawRay(transform.position, transform.TransformDirection(item) * hit.distance, new Color(0,color,1- color));
                    if (drawPoints)
                    {
                        if (useObjectColor)
                        {
                            Gizmos.color = hit.transform.GetComponent<MeshRenderer>().sharedMaterial.color;
                        }
                        else
                        {
                            Gizmos.color = new Color(0, color, 1 - color);
                        }

                        Gizmos.DrawSphere(hit.point, pointSize);
                    }
                }
                else if(showNoHits)
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(item) * range, Color.red);
                }

            }
            */
        }

        public List<Vector3> GetScanDirections()
        {
            List<Vector3> scanVectors = new List<Vector3>();

            float scanAngle = scanDensity;
            int pointsPerDisc = Mathf.CeilToInt(Mathf.PI * 2 / scanDensity);
            int nrOfDiscs = Mathf.CeilToInt(Mathf.PI / scanDensity);
            spherePoints = new Vector3[nrOfDiscs, pointsPerDisc];

            Vector3 vector0 = Vector3.forward;

            for (int i = 0; i < nrOfDiscs; i++)
            {
                for (int j = 0; j < pointsPerDisc; j++)
                {
                    if ((i * scanDensity * Mathf.Rad2Deg) > (360 - VerticalScanRange) / 2)
                    {
                        Vector3 newVector = Quaternion.Euler(0, j * scanDensity * Mathf.Rad2Deg, 0) * Quaternion.Euler(90 - i * scanDensity * Mathf.Rad2Deg, 0, 0) * vector0;
                        scanVectors.Add(newVector);


                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, transform.TransformDirection(newVector), out hit, range))
                        {
                            //float color = hit.distance / range;
                            spherePoints[i, j] = hit.point;

                        }
                        else spherePoints[i, j] = Vector3.negativeInfinity;
                    }
                    else spherePoints[i, j] = Vector3.negativeInfinity;
                    /*
                    if (i > 0)
                    {
                        Vector3 newVectorUp = Quaternion.Euler(0, j * scanDensity * Mathf.Rad2Deg, 0) * Quaternion.Euler( - i * scanDensity * Mathf.Rad2Deg, 0, 0) * vector0;
                        scanVectors.Add(newVectorUp);
                    }
                    */


                }
            }

            return scanVectors;
        }

        [ContextMenu("Update mesh")]
        public async void UpdateMesh()
        {
            GetScanDirections();
            await CreateSphereMesh(spherePoints);
            if (!filter) filter = GetComponent<MeshFilter>();
            filter.mesh = mesh;
            filter.sharedMesh.RecalculateBounds();
            //if (!renderer) renderer = GetComponent<MeshRenderer>();
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
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[upperPointIndex, j]));
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[upperPointIndex, nextPointIndex]));
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
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[upperPointIndex, nextPointIndex]));
                            tris.Add(verts.Count - 1);
                            verts.Add(transform.InverseTransformPoint(points[i, nextPointIndex]));
                            tris.Add(verts.Count - 1);
                        }
                    }

                }

                //Debug.DrawLine(points[i].Pos(), points[i].Pos() + left * points[i].roadWidth * 1.5f, Color.green);
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
