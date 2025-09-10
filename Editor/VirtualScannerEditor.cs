using Unity.VisualScripting.YamlDotNet.Core;
using UnityEditor;
using UnityEngine;

namespace GeoSharpi.Capture
{
    [CustomEditor(typeof(VirtualScanner))]
    public class VirtualScannerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Reference to the target
            VirtualScanner scanner = (VirtualScanner)target;

            // Draw default inspector (if you still want the normal fields shown)
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Scanning", EditorStyles.boldLabel);
                // Scan button
                if (GUILayout.Button("Scan"))
                {
                    scanner.ScanEnvironment();
                }
            }

            // Save path field
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Save Path", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(scanner.pointSavePath);

            if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
            {
                string path = EditorUtility.SaveFilePanel(
                    "Select Save Location",
                    Application.dataPath,
                    "scan_result",
                    "txt" // change to your preferred extension
                );

                if (!string.IsNullOrEmpty(path))
                {
                    Undo.RecordObject(scanner, "Set Save Path");
                    scanner.pointSavePath = path;
                    EditorUtility.SetDirty(scanner);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying)
            {
            EditorGUILayout.Space();
            // Scan button
            if (GUILayout.Button("Export"))
            {
                scanner.ExportPointCloud(scanner.scannedPoints, scanner.pointSavePath);
            }
            }
        }
    }
}