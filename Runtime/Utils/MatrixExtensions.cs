using UnityEngine;

namespace GeoSharpi.Utils
{
    /// <summary>
    /// Extensions to add functionalities to Matrix 4x4's
    /// </summary>
    public static class MatrixExtensions
    {
        /// <summary>
        /// Extracts the rotation quaternion from a matrix
        /// </summary>
        /// <param name="matrix">the input matrix</param>
        /// <returns> The rotation as a quaternion</returns>
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        /// <summary>
        /// Extracts the position from a matrix
        /// </summary>
        /// <param name="matrix">the input matrix></param>
        /// <returns>the position as a Vector3</returns>
        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;
            return position;
        }

        /// <summary>
        /// Extracts the scale from the matrix
        /// </summary>
        /// <param name="matrix">the input matrix</param>
        /// <returns>the scale as a Vector3</returns>
        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        /// <summary>
        /// Parses a string formatted matrix into a Matrix4x4
        /// </summary>
        /// <param name="matrix">the returned matrix</param>
        /// <param name="value">the string value of the matrix, split with newlines and spaces</param>
        /// <returns>a Matrix4x4</returns>
        public static Matrix4x4 Parse(this Matrix4x4 matrix, string value)
        {
            string[] chars = value.Split(new char[3] { ' ', '\t', '\n'});

            for (int i = 0; i < 16; i++)
            {
                matrix[i] = float.Parse(chars[i]);
            }
            
            return matrix;
        }
    }
}
