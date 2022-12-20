using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoSharpi.Utils
{
    /// <summary>
    /// Converts Transforms to a new coördinate system
    /// </summary>
    public class CoordinateSystemChanger : MonoBehaviour
    {
        [Header("New Axis Values")]
        [SerializeField]
        private NewAxis newX;
        [SerializeField]
        private NewAxis newY;
        [SerializeField]
        private NewAxis newZ;

        /// <summary>
        /// Applies the current defined transformation by the newAxis
        /// </summary>
        [ContextMenu("Change System")]
        public void ChangeSystem()
        {
            transform.FromMatrix(transform.localToWorldMatrix.ChangeSystem(newX, newY, newZ));
        }
    }
}

