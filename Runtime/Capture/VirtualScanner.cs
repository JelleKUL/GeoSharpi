using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Events;

namespace GeoSharpi.Capture
{
    public class VirtualScanner : MonoBehaviour
    {
        [Header("Scan Parameters")]
        [SerializeField]
        [Tooltip("Update the scan continuously at runtime")]
        private bool scanContinuous = true;
        [SerializeField]
        [Tooltip("the ratio between the point distance at a given radius")]
        [Min(0)]
        private float scanDensity = 0.01f;
        [SerializeField]
        [Tooltip("The max range of the scanner")]
        [Min(0f)]
        private float scanRange = 10;
        [SerializeField]
        [Tooltip("The total degrees the vertical axis can cover")]
        [Range(0f, 360f)]
        private float VerticalScanRange = 290;



        [Header("Coloring")]
        public RenderTexture colorTexture;

        [Header("Debug Visualisation")]
        [SerializeField]
        bool drawRays = true;
        [SerializeField]
        bool showNoHits = false;
        [SerializeField]
        bool drawPoints = false;
        [SerializeField]
        float pointSize = 0.01f;

        [HideInInspector]
        public string pointSavePath = "";



        private List<Transform> scannedObjects = new List<Transform>();

        // Private Properties new scan system
        private float lastDensity = -1;
        private List<ScanParameter> scanParameters = new List<ScanParameter>();
        [HideInInspector]
        public List<ScannedPoint> scannedPoints = new List<ScannedPoint>();

        

        // Start is called before the first frame update
        void Start()
        {

        }

        void Update()
        {
            if (scanContinuous) ScanEnvironment();
        }



        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            if (drawPoints && scannedPoints.Count > 0)
            {
                foreach (var point in scannedPoints)
                {
                    Gizmos.color = point.color;
                    Gizmos.DrawSphere(point.position, pointSize);
                }
            }

        }

        public void ScanEnvironment()
        {
            //check if the transform has changed, then update scan directions
            if (lastDensity != scanDensity)
            {
                lastDensity = scanDensity;
                UpdateScanParameters(scanDensity);
            }
            print("Casting " + scanParameters.Count + " rays");
            // perform the scan job
            CastRaysJob(transform.position, scanParameters);
            print("Found " + scannedPoints.Count + " points");
            // perform the color job
            if (colorTexture)
            {
                GetPointColorsJob();
                print("Points colored");
            }

        }

        // creates a list of scan directions and corresponding UV coordinates
        // This should only be updated if the Scanner moves
        private void UpdateScanParameters(float density)
        {
            scanParameters = new List<ScanParameter>();
            int pointsPerDisc = Mathf.CeilToInt(Mathf.PI * 2 / density); // the number of horizonontal captured points
            int nrOfDiscs = Mathf.CeilToInt(Mathf.PI / density); // the number of vertical rows
            Vector3 vector0 = Vector3.forward; // the starting vector

            for (int i = 0; i < nrOfDiscs; i++)
            {
                for (int j = 0; j < pointsPerDisc; j++)
                {
                    if ((i * density * Mathf.Rad2Deg) > (360 - VerticalScanRange) / 2) // filter out the bottom unscannable rows
                    {
                        Vector3 dir = Quaternion.Euler(0, j * density * Mathf.Rad2Deg, 0) * Quaternion.Euler(90 - i * density * Mathf.Rad2Deg, 0, 0) * vector0;
                        Vector2 uv = new Vector2(j * density * Mathf.Rad2Deg / 360, 1 - ((180 - i * density * Mathf.Rad2Deg) / 180));
                        scanParameters.Add(new ScanParameter(dir, uv, new Vector2Int(i,j)));
                    }
                }
            }
        }

        // Casts the rays in parallel and update the hits list
        private void CastRaysJob(Vector3 origin, List<ScanParameter> scanParams)
        {
            // reset the points
            scannedPoints = new List<ScannedPoint>();
            int rayCount = scanParams.Count;
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob);
            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(rayCount, Allocator.TempJob);

            for (int i = 0; i < rayCount; i++)
            {
                commands[i] = new RaycastCommand(origin, scanParams[i].direction, QueryParameters.Default, scanRange);
            }
            // Schedule batch of raycasts
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 32);
            // Complete the job
            handle.Complete();

            // Process results
            for (int i = 0; i < rayCount; i++)
            {
                if (results[i].collider != null)
                {
                    scannedPoints.Add(new ScannedPoint(
                        results[i].point,
                        results[i].normal,
                        scanParams[i].uv,
                        Color.white,
                        scanParams[i].pointIdx));
                }
            }
            // Dispose arrays
            commands.Dispose();
            results.Dispose();
        }

        private void GetPointColorsJob()
        {
            // keep track of the current rendertexture
            RenderTexture current = RenderTexture.active;
            RenderTexture.active = colorTexture;
            // Create a Texture2D with same size and format
            Texture2D tex = new Texture2D(colorTexture.width, colorTexture.height, TextureFormat.RGBA32, false);
            // Copy pixels from GPU -> CPU
            tex.ReadPixels(new Rect(0, 0, colorTexture.width, colorTexture.height), 0, 0);
            tex.Apply();
            //reset rendertexture
            RenderTexture.active = current;
            //convert to native array for job
            NativeArray<Color32> pixels = tex.GetRawTextureData<Color32>();
            // convert scannedPoints to a native array
            NativeArray<ScannedPoint> nativePoints = new NativeArray<ScannedPoint>(scannedPoints.Count, Allocator.TempJob);
            for (int i = 0; i < scannedPoints.Count; i++)
            {
                nativePoints[i] = scannedPoints[i];
            }
            Debug.Log(pixels.Length + ", " + tex.width + " " + tex.height + " " + tex.width * tex.height);

            // Run job
            var job = new SampleUVJob
            {
                pixels = pixels,
                points = nativePoints,
                texWidth = tex.width,
                texHeight = tex.height
            };

            JobHandle handle = job.Schedule(nativePoints.Length, 64);
            handle.Complete();
            // convert back to list
            for (int i = 0; i < scannedPoints.Count; i++)
            {
                scannedPoints[i] = nativePoints[i];
            }

            nativePoints.Dispose();
            Destroy(tex);
        }

        //MESHING



        public void SaveToCloudCompareTXT(Vector3[,] points, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var p in points)
                {
                    if (p.sqrMagnitude > scanRange * scanRange) continue;
                    // Use InvariantCulture to avoid commas instead of dots in some locales
                    writer.WriteLine(
                        string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            "{0} {1} {2}",
                            p.x, p.y, p.z
                        )
                    );
                }
            }

            Debug.Log($"Saved {points.Length} points to {filePath}");
        }
        [ContextMenu("Save Points")]
        public void SavePoints()
        {
            ExportPointCloud(scannedPoints, pointSavePath);
        }
        /// <summary>
        /// Saves a 2D array of points with normals to a .txt file in CloudCompare-compatible format.
        /// Each line will be: x y z nx ny nz r g b
        /// </summary>
        /// <param name="points">2D array of 3D positions</param>
        /// <param name="normals">2D array of normals (must match points dimensions)</param>
        /// <param name="filePath">Full path of the output .txt file</param>
        public void ExportPointCloud(List<ScannedPoint>points, string filePath)
        {

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (ScannedPoint point in points)
                {
                    Vector3 p = point.position;
                    Vector3 n = point.normal;
                    Color32 c = point.color;

                    writer.WriteLine(
                                string.Format(
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                                    p.x, p.y, p.z,
                                    n.x, n.y, n.z,
                                    c.r, c.g, c.b
                                )
                            );
                }

            }

            Debug.Log($"Saved {points.Count} scan samples (with normals & colors) to {filePath}");
        }

    }
    [System.Serializable]
    public struct ScanParameter
    {
        public Vector3 direction;
        public Vector2 uv;
        public Vector2Int pointIdx;

        public ScanParameter(Vector3 direction, Vector2 uv, Vector2Int pointIdx)
        {
            this.direction = direction;
            this.uv = uv;
            this.pointIdx = pointIdx;
        }
    }
    [System.Serializable]
    public struct ScannedPoint
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
        public Color32 color;
        public Vector2Int pointIdx;

        public ScannedPoint(Vector3 position, Vector3 normal, Vector2 uv, Color32 color, Vector2Int pointIdx)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
            this.color = color;
            this.pointIdx = pointIdx;
        }
    }

    public struct SampleUVJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color32> pixels;
        public NativeArray<ScannedPoint> points; // job will write into this
        public int texWidth;
        public int texHeight;

        public float density;

        public void Execute(int index)
        {
            ScannedPoint point = points[index];

            int x = Mathf.Clamp((int)(point.uv.x * texWidth), 0, texWidth - 1);
            int y = Mathf.Clamp((int)(point.uv.y * texHeight), 0, texHeight - 1);

            int pixelIndex = y * texWidth + x;
            point.color = pixels[pixelIndex];
            point.color.a = 255;

            points[index] = point; // write back
        }
    }
}
