using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(RDFNameSpacesScriptableObject))]
public class RDFNameSpacesScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20);

        RDFNameSpacesScriptableObject nameSpace = (RDFNameSpacesScriptableObject)target;
        if (GUILayout.Button("Serialize"))
        {
            nameSpace.Serialize();
        }
        if (GUILayout.Button("DeSerialize"))
        {
            nameSpace.DeSerialize();
        }



    }
}

