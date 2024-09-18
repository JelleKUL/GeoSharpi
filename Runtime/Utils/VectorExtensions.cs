
using UnityEngine;

namespace GeoSharpi.Utils
{
    public static class VectorExtensions
    {
        public static Vector3 RoundToDecimal(this Vector3 vector, int nrDecimals)
        {

            vector *= Mathf.Pow(10,nrDecimals);
            Vector3 roundedVector = new Vector3(Mathf.Round(vector.x),Mathf.Round(vector.y),Mathf.Round(vector.z));

            return roundedVector/ Mathf.Pow(10,nrDecimals);
        }

        public static Vector3 RoundToGrid(this Vector3 vector, float gridSize)
        {

            vector /= gridSize;
            Vector3 roundedVector = new Vector3(Mathf.Round(vector.x),Mathf.Round(vector.y),Mathf.Round(vector.z));

            return roundedVector * gridSize;
        }

        public static Vector3 RoundToGridIndex(this Vector3 vector, float gridSize)
        {
            
            vector /= gridSize;
            Vector3 roundedVector = new Vector3(FloorToGrid(vector.x),FloorToGrid(vector.y),FloorToGrid(vector.z));

            return roundedVector;
            
        }
        private static int FloorToGrid(float val){
            return Mathf.FloorToInt(val>=0? val:val+1);
        }
    }
}
