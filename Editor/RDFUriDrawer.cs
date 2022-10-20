using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GeoSharpi.Utils;

namespace GeoSharpi
{
    /// <summary>
    /// A Custom GUI drawer for `RDFAttribute`, displaying the prefix of the resource
    /// </summary>
    [CustomPropertyDrawer(typeof(RDFUriAttribute))]
    public class RDFUriDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override the default drawer
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RDFUriAttribute RDFUri = attribute as RDFUriAttribute; // get the Custom attribute to access its variables
            label.text = RDFUri.prefix + ":" + (char.ToLowerInvariant(label.text[0]) + label.text.Substring(1)).Replace(" ", ""); //Update the label to an rdf format
            EditorGUI.PropertyField(position, property, label, true); //draw the rest of the GUI
        }

        /// <summary>
        /// Override the property height to update the height in arrays or lists
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true); return height;
        }
    }
}



