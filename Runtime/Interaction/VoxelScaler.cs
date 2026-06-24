using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeoSharpi.Marching;

public class VoxelArrayInterpolator : MonoBehaviour
{
    [Header("Target")]
    public Texture3DMarcher texture3DMarcher;

    [Header("Interpolation Settings")]
    public VoxelArray.VoxelAxis axis = VoxelArray.VoxelAxis.Y;
    public int startIndex = 0;
    public int endIndex = 10;
    [Range(0.1f, 5f)]
    public float scalingFactor = 1f;
    public bool repeat = false;

    [Header("Options")]
    public bool updateMeshAfter = true;

    private VoxelArray newVoxelArray;

    void Start()
    {
        if (texture3DMarcher == null)
        {
            Debug.LogError("No Texture3DMarcher assigned!");
            return;
        }
    }

    void Update()
    {
        InterpolateVoxels();
    }

    [ContextMenu("Interpolate Voxels")]
    public void InterpolateVoxels()
    {
        if (texture3DMarcher.voxelArray == null) return;

        newVoxelArray = new VoxelArray(texture3DMarcher.voxelArray.Voxels, texture3DMarcher.voxelArray.colors);

        newVoxelArray.InterpolateValueRange(axis, (startIndex, endIndex), scalingFactor, repeat);

        Debug.Log($"Interpolated voxels along {axis} from {startIndex} to {endIndex} with scaling {scalingFactor} (repeat: {repeat})");

        if (updateMeshAfter)
        {
            texture3DMarcher.UpdateMeshFromVoxelArray(newVoxelArray);
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            InterpolateVoxels();
        }
    }
}