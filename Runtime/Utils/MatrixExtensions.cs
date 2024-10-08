using UnityEngine;
using System.Text.RegularExpressions;

namespace GeoSharpi.Utils
{
    [System.Serializable]
    public enum NewAxis { X, Y, Z, minX, minY, minZ };

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
            string outputString = Regex.Replace(value, @"[\s\[\]\t\n]+", " "); // remove all the brackets and newlines
            outputString = Regex.Replace(outputString, @"\s+", " "); // remove all the double spaces
            string[] chars = outputString.Trim().Split(' ');

            for (int i = 0; i < 16; i++)
            {
                if (float.TryParse(chars[i], out float f))
                {
                    matrix[i] = f;
                }
                else Debug.Log("Value: " + chars[i] + "cannot be parsed to a float");

            }

            return matrix.transpose;
        }

        /// <summary>
        /// Transforms a Matrix to another co�rdinate system by defining new axis
        /// </summary>
        /// <param name="matrix">the input matrix</param>
        /// <param name="newX">the new axis you want the original x to be</param>
        /// <param name="newY">the new axis you want the original y to be</param>
        /// <param name="newZ">the new axis you want the original z to be</param>
        /// <returns>the transformed matrix4x4</returns>
        public static Matrix4x4 ChangeSystem(this Matrix4x4 matrix, NewAxis newX, NewAxis newY, NewAxis newZ)
        {
            Matrix4x4 transformation = Matrix4x4.zero;
            transformation.m33 = 1;

            switch (newX)
            {
                case NewAxis.X:
                    transformation.m00 = 1;
                    break;
                case NewAxis.Y:
                    transformation.m01 = 1;
                    break;
                case NewAxis.Z:
                    transformation.m02 = 1;
                    break;
                case NewAxis.minX:
                    transformation.m00 = -1;
                    break;
                case NewAxis.minY:
                    transformation.m01 = -1;
                    break;
                case NewAxis.minZ:
                    transformation.m02 = -1;
                    break;
            }

            switch (newY)
            {
                case NewAxis.X:
                    transformation.m10 = 1;
                    break;
                case NewAxis.Y:
                    transformation.m11 = 1;
                    break;
                case NewAxis.Z:
                    transformation.m12 = 1;
                    break;
                case NewAxis.minX:
                    transformation.m10 = -1;
                    break;
                case NewAxis.minY:
                    transformation.m11 = -1;
                    break;
                case NewAxis.minZ:
                    transformation.m12 = -1;
                    break;
            }

            switch (newZ)
            {
                case NewAxis.X:
                    transformation.m20 = 1;
                    break;
                case NewAxis.Y:
                    transformation.m21 = 1;
                    break;
                case NewAxis.Z:
                    transformation.m22 = 1;
                    break;
                case NewAxis.minX:
                    transformation.m20 = -1;
                    break;
                case NewAxis.minY:
                    transformation.m21 = -1;
                    break;
                case NewAxis.minZ:
                    transformation.m22 = -1;
                    break;
            }
            return transformation.transpose * matrix * transformation;
        }
    }
}
