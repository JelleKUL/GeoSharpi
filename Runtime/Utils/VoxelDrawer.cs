using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using GeoSharpi.Utils.Events;
using UnityEditor;

namespace GeoSharpi.Utils
{
public class VoxelDrawer : MonoBehaviour
{
    [SerializeField]
    private int groundPlaneHeightIndex = 0;
    [SerializeField]
    private float voxelSize = 0.1f;
    [SerializeField]
    private int voxelDimension = 8;
    private int[,,] voxelGrid = new int[100, 100, 100];
    private GameObject[,,] voxelObjects = new GameObject[100, 100, 100];
    
    [SerializeField]
    private bool useGizmos = false;
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
    public IntEvent onGridSizeChanged = new IntEvent();

    // Start is called before the first frame update
    void Start()
    {
        // init a zero array
        voxelGrid = new int[voxelDimension, voxelDimension, voxelDimension];
        for (int i = 0; i < voxelDimension; i++){
            for (int j = 0; j < voxelDimension; j++){
                for (int k = 0; k < voxelDimension; k++){
                    voxelGrid[i,j,k] = 0;
                }
            }
        }
        if(fillCorners){
            int idx = voxelDimension-1;
            voxelGrid[0,0,0] = 1;
            voxelGrid[idx,0,0] = 1;
            voxelGrid[0,idx,0] = 1;
            voxelGrid[0,0,idx] = 1;
            voxelGrid[0,idx,idx] = 1;
            voxelGrid[idx,idx,0] = 1;
            voxelGrid[idx,0,idx] = 1;
            voxelGrid[idx,idx,idx] = 1;
        }
        onGridSizeChanged.Invoke(voxelDimension-1);

        //Debug.Log(voxelGrid[0,0,0]);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftAlt)){
            RotateCamera();
            return;
        }
        
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if(!RayInVoxelLayer()) return;
                
            Vector3 mouseIndex = GetMousePos().RoundToGridIndex(voxelSize);
            int x = Mathf.RoundToInt(mouseIndex.x);
            int y = Mathf.RoundToInt(mouseIndex.y);
            int z = Mathf.RoundToInt(mouseIndex.z);
            if(x >= voxelDimension || y >= voxelDimension || z >= voxelDimension) return;

            if (Input.GetMouseButton(0)) voxelGrid[x,y,z] = 1;
            if (Input.GetMouseButton(1)) voxelGrid[x,y,z] = 0;

            if(!useGizmos) UpdateVoxelGrid();
        }
    }

    void RotateCamera(){
        if(Input.GetMouseButton(0)) {
            float horizontalInput = Input.GetAxis("Mouse X");
            float verticalInput = Input.GetAxis("Mouse Y");
            cameraNull.Rotate(Vector3.up, horizontalInput * cameraMoveSpeed * Time.deltaTime, Space.World);
            cameraNull.Rotate(Vector3.right, -verticalInput * cameraMoveSpeed * Time.deltaTime, Space.Self);
			float X = cameraNull.rotation.eulerAngles.x;
			float Y = cameraNull.rotation.eulerAngles.y;
			cameraNull.rotation = Quaternion.Euler(X, Y, 0);
		}
    }

    void UpdateVoxelGrid(){
        for (int i = 0; i < voxelDimension; i++){
                for (int j = 0; j < voxelDimension; j++){
                    Gizmos.color = Color.Lerp(Color.green, Color.blue, j/(float)voxelDimension);
                    for (int k = 0; k < voxelDimension; k++){
                        //Debug.Log(i + ", " + j + ", " + k + "= " + voxelGrid[i,j,k]);
                        if(voxelGrid[i,j,k] ==1){
                            if(voxelObjects[i,j,k] == null){
                                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube.transform.localScale = voxelSize * Vector3.one;
                                cube.transform.position = (Vector3.one * 0.5f + new Vector3(i,j,k)) * voxelSize;
                                voxelObjects[i,j,k] = cube;
                            } 
                        }
                        else if(voxelObjects[i,j,k] != null){
                            GameObject cube = voxelObjects[i,j,k];
                            voxelObjects[i,j,k] = null;
                            Destroy(cube);
                        }
                    }
                }
            }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(useGizmos){
            // init a zero array
            for (int i = 0; i < voxelDimension; i++){
                for (int j = 0; j < voxelDimension; j++){
                    Gizmos.color = Color.Lerp(Color.green, Color.blue, j/(float)voxelDimension);
                    for (int k = 0; k < voxelDimension; k++){
                        //Debug.Log(i + ", " + j + ", " + k + "= " + voxelGrid[i,j,k]);
                        if(voxelGrid[i,j,k] ==1){
                            Gizmos.DrawCube((Vector3.one * 0.5f + new Vector3(i,j,k)) * voxelSize, Vector3.one*voxelSize);
                        }
                    }
                }
            }
        }
        
        if(RayInVoxelLayer())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube((GetMousePos().RoundToGridIndex(voxelSize) + Vector3.one * 0.5f) * voxelSize, Vector3.one*voxelSize);
        }
        
        //Gizmos.DrawSphere(GetMousePos(), 0.1f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube( Vector3.one * voxelSize*voxelDimension/2, Vector3.one * voxelSize*voxelDimension );
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(
            new Vector3(voxelSize*voxelDimension/2,voxelSize * (groundPlaneHeightIndex + 0.5f), voxelSize*voxelDimension/2), 
            new Vector3(voxelSize*voxelDimension,voxelSize , voxelSize*voxelDimension ));
        //Debug.Log(RayInVoxelGrid());
    }
    [ContextMenu("LogJson")]
    
    public void ChangeGroundPlaneHeight(System.Single val){
        groundPlaneHeightIndex = (int)val;
    }
    
    public void LogJson(){
        Debug.Log(GetVoxelGrid());
    }
    public string GetVoxelGrid()
    {
        VoxelGrid grid = new VoxelGrid(voxelSize, Vector3.zero);
        for (int i = 0; i < voxelDimension; i++){
            for (int j = 0; j < voxelDimension; j++){
                for (int k = 0; k < voxelDimension; k++){
                    //Debug.Log(i + ", " + j + ", " + k + "= " + voxelGrid[i,j,k]);
                    if(voxelGrid[i,j,k] ==1){
                        grid.voxels.Add(new Voxel(new Vector3Int(i,j,k), Color.white));
                    }
                }
            }
        }

        return grid.ToJsonString();

    }

    [ContextMenu("Write Data")]
    public void WriteData()
    {
        File.WriteAllText(filePath, GetVoxelGrid());
    }
  
    [ContextMenu("Read Data")]
    public void ReadData()
    {
        string json = File.ReadAllText(filePath);
        VoxelGrid newGrid = JsonUtility.FromJson<VoxelGrid>(json);
        voxelSize = newGrid.voxelSize;
        // init a zero array
        voxelGrid = new int[voxelDimension, voxelDimension, voxelDimension];
        for (int i = 0; i < voxelDimension; i++){
            for (int j = 0; j < voxelDimension; j++){
                for (int k = 0; k < voxelDimension; k++){
                    voxelGrid[i,j,k] = 0;
                }
            }
        }

        for (int i = 0; i<newGrid.voxels.Count; i++){
            voxelGrid[newGrid.voxels[i].gridIndex.x, newGrid.voxels[i].gridIndex.y, newGrid.voxels[i].gridIndex.z] = 1;
        }
    }

    public void PlaceMesh(string meshPath){
        GameObject newObj = MeshIO.LoadMesh(meshPath);
        MeshFilter mesh = newObj.GetComponentInChildren<MeshFilter>();
        Vector3 center = mesh.mesh.bounds.center;
        Vector3 extends = mesh.mesh.bounds.extents;
        mesh.GetComponent<MeshRenderer>().material = importMat;

        newObj.transform.localScale /= extends.Max()* 0.5f;
        newObj.transform.position = Vector3.one * voxelDimension/2f * voxelSize - center;
    }

    public bool RayInVoxelGrid(){
        Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);
        Bounds voxelBounds = new Bounds( Vector3.one * voxelSize*voxelDimension/2, Vector3.one * voxelSize*voxelDimension);
        return voxelBounds.IntersectRay(ray);
    }

    bool RayInVoxelLayer(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Bounds voxelBounds = new Bounds(
            new Vector3(voxelSize*voxelDimension/2,voxelSize * (groundPlaneHeightIndex + 0.5f), voxelSize*voxelDimension/2), 
            new Vector3(voxelSize*voxelDimension,0.001f , voxelSize*voxelDimension ));
        return voxelBounds.IntersectRay(ray);
    }

    Vector3 GetMousePos(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // create a plane at 0,0,0 whose normal points to +Y:
        Plane hPlane = new Plane(Vector3.up, Vector3.up * (groundPlaneHeightIndex+0.5f) * voxelSize);
        // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
        float distance = 0; 
        // if the ray hits the plane...
        if (hPlane.Raycast(ray, out distance)){
            // get the hit point:
            //Debug.Log("point is: " + distance.ToString());
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}

[System.Serializable]
public class Voxel
{
    public Vector3Int gridIndex = Vector3Int.zero;
    public Color color = Color.white;

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