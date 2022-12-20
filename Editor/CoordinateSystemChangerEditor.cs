using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GeoSharpi.Utils;

[CustomEditor(typeof(CoordinateSystemChanger))]
public class CoordinateSystemChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(20);
        CoordinateSystemChanger myScript = (CoordinateSystemChanger)target;
        if (GUILayout.Button("Change Coördinate System"))
        {
            myScript.ChangeSystem();
        }
    }
}
