using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GeoSharpi.Utils;
using GeoSharpi.Utils.Events;
using UnityEngine.Events;

namespace GeoSharpi.Interaction
{
    public class VoxelDrawer : MonoBehaviour
    {
        [SerializeField]
        private int groundPlaneHeightIndex = 0;
        [SerializeField]
        public float voxelSize = 0.1f;
        [SerializeField]
        public int voxelDimension = 8;
        [HideInInspector]
        public int[,,] voxelGrid = new int[100, 100, 100];
        private GameObject[,,] voxelObjects = new GameObject[100, 100, 100];

        [SerializeField]
        private bool useGizmos = false;
        [SerializeField]
        private bool showVoxelGizmos = false;
        [SerializeField]
        private Material importMat;
        [SerializeField]
        private bool fillCorners = true;
        [SerializeField]
        private string filePath = "Assets/Output/voxelgrid.txt";

        [SerializeField]
        private float cameraMoveSpeed = 10;
        [SerializeField]
        private Transform cameraNull;

        [Header("VoxelOcclusion")]
        [SerializeField]
        private bool showOcclusion = false;
        [SerializeField]
        private Transform occlusionViewPoint;
        [HideInInspector]
        public int[,,] occludedVoxelGrid = new int[100, 100, 100];


        public IntEvent onGridSizeChanged = new IntEvent();
        public UnityEvent OnVoxelsChanged = new UnityEvent();

        // Start is called before the first frame update
        void Start()
        {
            // init a zero array
            voxelGrid = new int[voxelDimension, voxelDimension, voxelDimension];
            for (int i = 0; i < voxelDimension; i++)
            {
                for (int j = 0; j < voxelDimension; j++)
                {
                    for (int k = 0; k < voxelDimension; k++)
                    {
                        voxelGrid[i, j, k] = 0;
                    }
                }
            }
            if (fillCorners)
            {
                int idx = voxelDimension - 1;
                voxelGrid[0, 0, 0] = 1;
                voxelGrid[idx, 0, 0] = 1;
                voxelGrid[0, idx, 0] = 1;
                voxelGrid[0, 0, idx] = 1;
                voxelGrid[0, idx, idx] = 1;
                voxelGrid[idx, idx, 0] = 1;
                voxelGrid[idx, 0, idx] = 1;
                voxelGrid[idx, idx, idx] = 1;
            }
            onGridSizeChanged.Invoke(voxelDimension - 1);

            //Debug.Log(voxelGrid[0,0,0]);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                RotateCamera();
                return;
            }

            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (!RayInVoxelLayer()) return;

                Vector3 mouseIndex = GetMousePos().RoundToGridIndex(voxelSize);
                int x = Mathf.RoundToInt(mouseIndex.x);
                int y = Mathf.RoundToInt(mouseIndex.y);
                int z = Mathf.RoundToInt(mouseIndex.z);
                if (x >= voxelDimension || y >= voxelDimension || z >= voxelDimension) return;

                if (Input.GetMouseButton(0)) voxelGrid[x, y, z] = 1;
                if (Input.GetMouseButton(1)) voxelGrid[x, y, z] = 0;

                if (!useGizmos) UpdateVoxelGrid();
                OnVoxelsChanged.Invoke();
            }

            if (showOcclusion && occlusionViewPoint != null)
            {
                List<Vector3Int> occludedVoxelList = FindOccludedVoxels(voxelGrid, occlusionViewPoint.position);

                occludedVoxelGrid = new int[voxelDimension, voxelDimension, voxelDimension];
                for (int i = 0; i < voxelDimension; i++)
                {
                    for (int j = 0; j < voxelDimension; j++)
                    {
                        for (int k = 0; k < voxelDimension; k++)
                        {
                            occludedVoxelGrid[i, j, k] = 0;
                        }
                    }
                }
                foreach (Vector3Int i in occludedVoxelList)
                {
                    occludedVoxelGrid[i[0], i[1], i[2]] = 1;
                }

            }
        }

        void RotateCamera()
        {
            if (Input.GetMouseButton(0))
            {
                float horizontalInput = Input.GetAxis("Mouse X");
                float verticalInput = Input.GetAxis("Mouse Y");
                cameraNull.Rotate(Vector3.up, horizontalInput * cameraMoveSpeed * Time.deltaTime, Space.World);
                cameraNull.Rotate(Vector3.right, -verticalInput * cameraMoveSpeed * Time.deltaTime, Space.Self);
                float X = cameraNull.rotation.eulerAngles.x;
                float Y = cameraNull.rotation.eulerAngles.y;
                cameraNull.rotation = Quaternion.Euler(X, Y, 0);
            }
        }

        void UpdateVoxelGrid()
        {
            for (int i = 0; i < voxelDimension; i++)
            {
                for (int j = 0; j < voxelDimension; j++)
                {
                    Gizmos.color = Color.Lerp(Color.green, Color.blue, j / (float)voxelDimension);
                    for (int k = 0; k < voxelDimension; k++)
                    {
                        //Debug.Log(i + ", " + j + ", " + k + "= " + voxelGrid[i,j,k]);
                        if (voxelGrid[i, j, k] == 1)
                        {
                            if (voxelObjects[i, j, k] == null)
                            {
                                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube.transform.localScale = voxelSize * Vector3.one;
                                cube.transform.position = (Vector3.one * 0.5f + new Vector3(i, j, k)) * voxelSize;
                                voxelObjects[i, j, k] = cube;
                            }
                        }
                        else if (voxelObjects[i, j, k] != null)
                        {
                            GameObject cube = voxelObjects[i, j, k];
                            voxelObjects[i, j, k] = null;
                            Destroy(cube);
                        }
                    }
                }
            }

        }
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (useGizmos)
            {
                // init a zero array
                for (int i = 0; i < voxelDimension; i++)
                {
                    for (int j = 0; j < voxelDimension; j++)
                    {
                        
                        for (int k = 0; k < voxelDimension; k++)
                        {
                            //Debug.Log(i + ", " + j + ", " + k + "= " + voxelGrid[i,j,k]);
                            if (voxelGrid[i, j, k] == 1)
                            {
                                if (showVoxelGizmos)
                                {
                                    Gizmos.color = Color.Lerp(Color.green, Color.blue, j / (float)voxelDimension);
                                    Gizmos.DrawCube((Vector3.one * 0.5f + new Vector3(i, j, k)) * voxelSize, Vector3.one * voxelSize);
                                }
                            }
                            if (occludedVoxelGrid[i, j, k] == 1)
                            {
                                if (showOcclusion)
                                {
                                    Gizmos.color = Color.magenta;
                                    Gizmos.DrawCube((Vector3.one * 0.5f + new Vector3(i, j, k)) * voxelSize, Vector3.one * voxelSize);
                                }
                            }
                        }
                    }
                }
            }


            if (RayInVoxelLayer())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube((GetMousePos().RoundToGridIndex(voxelSize) + Vector3.one * 0.5f) * voxelSize, Vector3.one * voxelSize);
            }

            //Gizmos.DrawSphere(GetMousePos(), 0.1f);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.one * voxelSize * voxelDimension / 2, Vector3.one * voxelSize * voxelDimension);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(
                new Vector3(voxelSize * voxelDimension / 2, voxelSize * (groundPlaneHeightIndex + 0.5f), voxelSize * voxelDimension / 2),
                new Vector3(voxelSize * voxelDimension, voxelSize, voxelSize * voxelDimension));
            //Debug.Log(RayInVoxelGrid());

            
        }
        [ContextMenu("LogJson")]

        public void ChangeGroundPlaneHeight(System.Single val)
        {
            groundPlaneHeightIndex = (int)val;
        }

        public void LogJson()
        {
            Debug.Log(GetVoxelGrid(voxelGrid));
        }
        public string GetVoxelGrid(int[,,] _voxelGrid)
        {
            VoxelGrid grid = new VoxelGrid(voxelSize, Vector3.zero);
            for (int i = 0; i < voxelDimension; i++)
            {
                for (int j = 0; j < voxelDimension; j++)
                {
                    for (int k = 0; k < voxelDimension; k++)
                    {
                        //Debug.Log(i + ", " + j + ", " + k + "= " + voxelGrid[i,j,k]);
                        if (_voxelGrid[i, j, k] == 1)
                        {
                            grid.voxels.Add(new Voxel(new Vector3Int(i, j, k), Color.white));
                        }
                    }
                }
            }

            return grid.ToJsonString();

        }

        [ContextMenu("Write Data")]
        public void WriteData()
        {
            File.WriteAllText(filePath, GetVoxelGrid(voxelGrid));
        }

        public void WriteDataToFile(string path)
        {
            File.WriteAllText(path, GetVoxelGrid(voxelGrid));
        }
        [ContextMenu("Write Occupied Data")]
        public void WriteOccupiedData()
        {
            File.WriteAllText(filePath, GetVoxelGrid(occludedVoxelGrid));
        }

        [ContextMenu("Read Data")]
        public void ReadData()
        {
            ReadDataFromFile(filePath);
        }
        public void ReadDataFromFile(string path)
        {
            string json = File.ReadAllText(path);
            VoxelGrid newGrid = JsonUtility.FromJson<VoxelGrid>(json);
            voxelSize = newGrid.voxelSize;
            // init a zero array
            voxelGrid = new int[voxelDimension, voxelDimension, voxelDimension];
            for (int i = 0; i < voxelDimension; i++)
            {
                for (int j = 0; j < voxelDimension; j++)
                {
                    for (int k = 0; k < voxelDimension; k++)
                    {
                        voxelGrid[i, j, k] = 0;
                    }
                }
            }

            for (int i = 0; i < newGrid.voxels.Count; i++)
            {
                voxelGrid[newGrid.voxels[i].gridIndex.x, newGrid.voxels[i].gridIndex.y, newGrid.voxels[i].gridIndex.z] = 1;
            }
            UpdateVoxelGrid();
        }

        public void PlaceMesh(string meshPath)
        {
            GameObject newObj = MeshIO.LoadMesh(meshPath);
            MeshFilter mesh = newObj.GetComponentInChildren<MeshFilter>();
            Vector3 center = mesh.mesh.bounds.center;
            Vector3 extends = mesh.mesh.bounds.extents;
            mesh.GetComponent<MeshRenderer>().material = importMat;

            newObj.transform.localScale /= extends.Max() * 0.5f;
            newObj.transform.position = Vector3.one * voxelDimension / 2f * voxelSize - center;
        }

        public bool RayInVoxelGrid()
        {
            Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);
            Bounds voxelBounds = new Bounds(Vector3.one * voxelSize * voxelDimension / 2, Vector3.one * voxelSize * voxelDimension);
            return voxelBounds.IntersectRay(ray);
        }

        bool RayInVoxelLayer()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Bounds voxelBounds = new Bounds(
                new Vector3(voxelSize * voxelDimension / 2, voxelSize * (groundPlaneHeightIndex + 0.5f), voxelSize * voxelDimension / 2),
                new Vector3(voxelSize * voxelDimension, 0.001f, voxelSize * voxelDimension));
            return voxelBounds.IntersectRay(ray);
        }

        Vector3 GetMousePos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // create a plane at 0,0,0 whose normal points to +Y:
            Plane hPlane = new Plane(Vector3.up, Vector3.up * (groundPlaneHeightIndex + 0.5f) * voxelSize);
            // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
            float distance = 0;
            // if the ray hits the plane...
            if (hPlane.Raycast(ray, out distance))
            {
                // get the hit point:
                //Debug.Log("point is: " + distance.ToString());
                return ray.GetPoint(distance);
            }
            return Vector3.zero;
        }
        public static List<Vector3Int> Bresenham3D(Vector3Int p0, Vector3Int p1)
        {
            List<Vector3Int> points = new List<Vector3Int>();
            Vector3Int p = p0;

            int dx = Mathf.Abs(p1.x - p0.x);
            int dy = Mathf.Abs(p1.y - p0.y);
            int dz = Mathf.Abs(p1.z - p0.z);

            int sx = p0.x < p1.x ? 1 : -1;
            int sy = p0.y < p1.y ? 1 : -1;
            int sz = p0.z < p1.z ? 1 : -1;

            int dx2 = dx << 1;
            int dy2 = dy << 1;
            int dz2 = dz << 1;

            if (dx >= dy && dx >= dz)
            {
                int err1 = dy2 - dx;
                int err2 = dz2 - dx;
                for (int i = 0; i <= dx; i++)
                {
                    points.Add(p);
                    if (err1 > 0)
                    {
                        p.y += sy;
                        err1 -= dx2;
                    }
                    if (err2 > 0)
                    {
                        p.z += sz;
                        err2 -= dx2;
                    }
                    err1 += dy2;
                    err2 += dz2;
                    p.x += sx;
                }
            }
            else if (dy >= dx && dy >= dz)
            {
                int err1 = dx2 - dy;
                int err2 = dz2 - dy;
                for (int i = 0; i <= dy; i++)
                {
                    points.Add(p);
                    if (err1 > 0)
                    {
                        p.x += sx;
                        err1 -= dy2;
                    }
                    if (err2 > 0)
                    {
                        p.z += sz;
                        err2 -= dy2;
                    }
                    err1 += dx2;
                    err2 += dz2;
                    p.y += sy;
                }
            }
            else
            {
                int err1 = dy2 - dz;
                int err2 = dx2 - dz;
                for (int i = 0; i <= dz; i++)
                {
                    points.Add(p);
                    if (err1 > 0)
                    {
                        p.y += sy;
                        err1 -= dz2;
                    }
                    if (err2 > 0)
                    {
                        p.x += sx;
                        err2 -= dz2;
                    }
                    err1 += dy2;
                    err2 += dx2;
                    p.z += sz;
                }
            }

            return points;
        }
        public List<Vector3Int> FindOccludedVoxels(int[,,] voxelGrid, Vector3 viewpoint)
        {
            List<Vector3Int> occluded = new List<Vector3Int>();

            int sizeX = voxelGrid.GetLength(0);
            int sizeY = voxelGrid.GetLength(1);
            int sizeZ = voxelGrid.GetLength(2);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        if (voxelGrid[x, y, z] != 0) continue;

                        Vector3Int target = new Vector3Int(x, y, z);
                        List<Vector3Int> rayPath = Bresenham3D(Vector3Int.RoundToInt(viewpoint.RoundToGridIndex(voxelSize)), target);

                        bool blocked = false;
                        foreach (var point in rayPath)
                        {
                            if (point == target) break; // Stop before reaching the voxel itself

                            if (IsInsideGrid(point, sizeX, sizeY, sizeZ))
                            {
                                if (voxelGrid[point.x, point.y, point.z] == 1)
                                {
                                    blocked = true;
                                    break;
                                }
                            }
                        }

                        if (blocked)
                            occluded.Add(target);
                    }
                }
            }

            return occluded;
        }

        private static bool IsInsideGrid(Vector3Int p, int sizeX, int sizeY, int sizeZ)
        {
            return p.x >= 0 && p.x < sizeX &&
                p.y >= 0 && p.y < sizeY &&
                p.z >= 0 && p.z < sizeZ;
        }
    }

    [System.Serializable]
    public class Voxel
    {
        public Vector3Int gridIndex = Vector3Int.zero;
        public Color color = Color.white;
        public float distance = 0;

        public Voxel(Vector3Int gridIndex, Color color)
        {
            this.gridIndex = gridIndex;
            this.color = color;
        }
    }
    [System.Serializable]
    public class VoxelGrid
    {
        public float voxelSize = 1;
        public Vector3 origin = Vector3.zero;
        public List<Voxel> voxels = new List<Voxel>();

        public VoxelGrid(float voxelSize, Vector3 origin)
        {
            this.voxelSize = voxelSize;
            this.origin = origin;
        }

        public string ToJsonString(){

            string jsonString = JsonUtility.ToJson(this, prettyPrint: true);

            return jsonString;
        }
    }
}