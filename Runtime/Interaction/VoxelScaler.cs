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

    [Header("Gizmos")]
    public bool drawDebugPlanes = true;
    public Color startPlaneColor = new Color(0f, 1f, 0f, 0.35f);
    public Color endPlaneColor = new Color(1f, 0f, 0f, 0.35f);
    public Color repeatPlaneColor = new Color(1f, 0.65f, 0f, 0.25f);
    public int maxRepeatPlanesDrawn = 50; // safety cap so OnDrawGizmos can't hang the editor

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

    // ---------------------------------------------------------------
    // Gizmos
    // ---------------------------------------------------------------

    void OnDrawGizmosSelected()
    {
        if (!drawDebugPlanes || texture3DMarcher == null) return;
        if (texture3DMarcher.voxelArray == null) return;

        float voxelSize = texture3DMarcher.voxelSize;
        if (voxelSize <= 0f) return;

        // Use the array that actually produced the CURRENT mesh, not the
        // original texture3DMarcher.voxelArray. When this component scales
        // along X or Z, InterpolateValueRange changes the grid's Width/Depth,
        // and Texture3DMarcher re-centers the mesh using THAT array's
        // dimensions — so the gizmo must match newVoxelArray's size, not the
        // stale original, or X/Z planes drift off the mesh after a resize.
        // Falls back to the original array if no interpolation has run yet,
        // or if updateMeshAfter is off (in which case the displayed mesh is
        // still based on the original array).
        VoxelArray grid = (updateMeshAfter && newVoxelArray != null)
            ? newVoxelArray
            : texture3DMarcher.voxelArray;

        // Matches Texture3DMarcher.UpdateMeshFromVoxelArray's origin convention:
        // mesh origin sits at the bottom-center of the grid, so X and Z are
        // centered on 0 (spanning [-half, +half]) while Y starts at 0 and
        // spans upward to the full grid height.
        Vector3 fullExtent = new Vector3(
            (grid.Width - 1) * voxelSize,
            (grid.Height - 1) * voxelSize,
            (grid.Depth - 1) * voxelSize
        );
        float fullHalfWidth = fullExtent.x * 0.5f;
        float fullHalfDepth = fullExtent.z * 0.5f;

        // True centering offset used by Texture3DMarcher when it shifted
        // mesh vertices — this MUST come from the live grid's full
        // dimensions, not the clamped/padded display size below, or planes
        // sliced along X/Z will land in the wrong place after a resize.
        float centerOffsetX = fullHalfWidth;
        float centerOffsetZ = fullHalfDepth;

        // Shrink the plane's VISIBLE SIZE to 1 voxel beyond the furthest
        // extent of the actual marched mesh in each direction (cheaper than
        // rescanning the voxel grid, and reflects where the isosurface
        // really is rather than raw voxel density). This is purely a
        // display-size clamp and must stay separate from centerOffsetX/Z
        // above. Falls back to the full grid extent if no mesh exists yet.
        GetMeshBounds(out float halfWidth, out float halfDepth, out float height);

        halfWidth = Mathf.Min(halfWidth + voxelSize, fullHalfWidth);
        halfDepth = Mathf.Min(halfDepth + voxelSize, fullHalfDepth);
        height = Mathf.Min(height + voxelSize, fullExtent.y);

        Vector3 gridExtent = new Vector3(halfWidth * 2f, height, halfDepth * 2f);

        // Mirrors VoxelArray.InterpolateValueRange exactly:
        //   oldRange     = endIndex - startIndex
        //   finalRange   = round(oldRange * scalingFactor)
        //   repeatAmount = repeat ? max(1, round(scalingFactor)) : 1
        //   segmentLength = finalRange / repeatAmount   (integer division)
        // After the operation the interpolated block occupies
        // [startIndex, startIndex + finalRange], so that upper bound is
        // where the "end" plane actually sits in the new, rescaled grid.
        int oldRange = endIndex - startIndex;
        if (oldRange <= 1) return; // matches the early-out in InterpolateValueRange

        int finalRange = Mathf.RoundToInt(oldRange * scalingFactor);
        int newEndIndex = startIndex + finalRange;

        DrawIndexPlane(axis, startIndex, voxelSize, gridExtent, centerOffsetX, centerOffsetZ, startPlaneColor);
        DrawIndexPlane(axis, newEndIndex, voxelSize, gridExtent, centerOffsetX, centerOffsetZ, endPlaneColor);

        if (repeat && finalRange > 0)
        {
            int repeatAmount = Mathf.Max(1, Mathf.RoundToInt(scalingFactor));
            int segmentLength = finalRange / repeatAmount; // integer division, matches source

            if (segmentLength > 0)
            {
                // n = repeatAmount lands exactly on newEndIndex (already drawn
                // above as the end plane), so stop just before it. The cap is
                // just a safety net in case of unexpectedly large values.
                int planesDrawn = 0;
                for (int n = 1; n < repeatAmount && planesDrawn < maxRepeatPlanesDrawn; n++)
                {
                    int boundary = startIndex + n * segmentLength;
                    DrawIndexPlane(axis, boundary, voxelSize, gridExtent, centerOffsetX, centerOffsetZ, repeatPlaneColor);
                    planesDrawn++;
                }
            }
        }
    }

    /// <summary>
    /// Reads the actual marched mesh's bounding box (already in local space,
    /// already shifted to the bottom-center origin by
    /// Texture3DMarcher.UpdateMeshFromVoxelArray) and returns, per axis, how
    /// far the mesh extends from that axis's existing anchor:
    ///  - halfWidth/halfDepth: max absolute distance from local X=0 / Z=0
    ///    reached by the mesh (NOT bounds.extents — the mesh's own bounds
    ///    center can be off-origin if the filled region is asymmetric, so we
    ///    measure from the transform origin instead, to stay consistent with
    ///    "keep the plane's origin centered").
    ///  - height: max Y the mesh reaches (Y is anchored at 0, so this is
    ///    just bounds.max.y, clamped to be non-negative as a safety net).
    /// If no mesh exists yet, all three are 0.
    /// </summary>
    private void GetMeshBounds(out float halfWidth, out float halfDepth, out float height)
    {
        MeshFilter meshFilter = texture3DMarcher.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;

        if (mesh == null)
        {
            halfWidth = 0f;
            halfDepth = 0f;
            height = 0f;
            return;
        }

        Bounds b = mesh.bounds;

        halfWidth = Mathf.Max(Mathf.Abs(b.min.x), Mathf.Abs(b.max.x));
        halfDepth = Mathf.Max(Mathf.Abs(b.min.z), Mathf.Abs(b.max.z));
        height = Mathf.Max(b.max.y, 0f);
    }

    /// <summary>
    /// Draws a debug plane perpendicular to the given voxel axis, at the given
    /// voxel index, in local space relative to this transform.
    ///
    /// centerOffsetX/Z is the TRUE mesh-centering offset — matching exactly
    /// what Texture3DMarcher.UpdateMeshFromVoxelArray subtracted from vertex
    /// positions for the live grid — and is used to place the plane on its
    /// slicing axis. gridExtent is the (possibly clamped/padded-to-mesh-
    /// bounds) display size of the plane on the two non-slicing axes; using
    /// the wrong one for either purpose will visibly desync the gizmo from
    /// the actual mesh.
    /// </summary>
    private void DrawIndexPlane(VoxelArray.VoxelAxis planeAxis, int index, float voxelSize, Vector3 gridExtent, float centerOffsetX, float centerOffsetZ, Color color)
    {
        float worldOffset = index * voxelSize;

        Vector3 localCenter;
        Vector3 size; // full size of the plane in local space
        Vector3 normal;

        switch (planeAxis)
        {
            case VoxelArray.VoxelAxis.X:
                localCenter = new Vector3(worldOffset - centerOffsetX, gridExtent.y * 0.5f, 0f);
                size = new Vector3(0.01f, gridExtent.y, gridExtent.z);
                normal = Vector3.right;
                break;
            case VoxelArray.VoxelAxis.Y:
                localCenter = new Vector3(0f, worldOffset, 0f);
                size = new Vector3(gridExtent.x, 0.01f, gridExtent.z);
                normal = Vector3.up;
                break;
            default: // Z
                localCenter = new Vector3(0f, gridExtent.y * 0.5f, worldOffset - centerOffsetZ);
                size = new Vector3(gridExtent.x, gridExtent.y, 0.01f);
                normal = Vector3.forward;
                break;
        }

        Matrix4x4 prevMatrix = Gizmos.matrix;
        Color prevColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = color;

        // Flattened cube renders as a translucent filled quad; wire outline
        // on top makes the boundary readable even at low alpha.
        Gizmos.DrawCube(localCenter, size);

        Gizmos.color = new Color(color.r, color.g, color.b, Mathf.Min(1f, color.a + 0.4f));
        //Gizmos.DrawWireCube(localCenter, size);

        // Small normal indicator so slicing direction is obvious at a glance.
        Gizmos.DrawLine(localCenter, localCenter + normal * voxelSize * 2f);

        Gizmos.matrix = prevMatrix;
        Gizmos.color = prevColor;
    }
}