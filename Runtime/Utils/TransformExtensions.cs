using UnityEngine;

namespace GeoSharpi.Utils
{
    /// <summary>
    /// Extensions to add functionalities to Transforms
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Sets a transform to a Matrix4x4
        /// </summary>
        /// <param name="transform">the transform to change</param>
        /// <param name="matrix">The matrix4x4 to set the transform to</param>
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.localScale = matrix.ExtractScale();
            transform.rotation = matrix.ExtractRotation();
            transform.position = matrix.ExtractPosition();
        }
    }
}
