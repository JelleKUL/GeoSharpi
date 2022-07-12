using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.IO;

[CreateAssetMenu(fileName ="namespaces",menuName = "ScriptableObjects/nameSpaces",order = 0)]
public class RDFNameSpacesScriptableObject : ScriptableObject
{
    [System.Serializable]
    public class SerialisedNameSpace
    {
        public NameSpace[] nameSpaces;
    }

    [SerializeField]
    private string savePath = "Assets/NameSpaces.json";
    public NameSpace[] nameSpaces;

    public List<RDFNamespace> GetNameSpaces()
    {
        List<RDFNamespace> nameList = new List<RDFNamespace>();

        foreach (var name in nameSpaces)
        {
            nameList.Add(new RDFNamespace(name.prefix, name.uri));
        }

        return nameList;
    }

    public RDFNamespace GetNameSpaceByPrefix(string prefix)
    {
        foreach (var name in nameSpaces)
        {
            if (name.prefix == prefix) return new RDFNamespace(name.prefix, name.uri);
        }
        return null;
    }

    /// <summary>
    /// Converts a variable to a RDF tripple to add to a graph
    /// </summary>
    /// <param name="subject">The resource which contains this value</param>
    /// <param name="value">The variable to add</param>
    /// <param name="attr">The type of the variable</param>
    /// <returns></returns>
    public RDFTriple GetTriple(RDFResource subject,string variable, string value)
    {
        //Step 1: Find the correct attribute and namespace
        
        NameSpace correctNameSpace = null;
        Attribute correctAttribute = null;

        foreach (var name in nameSpaces)
        {
            correctNameSpace = name;
            correctNameSpace.UpdateAttributeUris();

            foreach (var attr in name.attributes)
            {
                if (attr.value == variable) correctAttribute = attr;
            }
        }

        if (correctAttribute == null) correctAttribute = new Attribute(variable);

        //Step 2: assign a new tripple with the precdicate and value

        RDFTriple newTrip = new RDFTriple(subject, correctAttribute.GetPredicate(), new RDFTypedLiteral(value, correctAttribute.GetRDFType()));

        return newTrip;
    }

    public void Serialize()
    {
        foreach (var nameSpace in nameSpaces)
        {
            for (int i = 0; i < nameSpace.attributes.Count; i++)
            {
                var ns = nameSpace.attributes[i];
                ns.dataTypeString = nameSpace.attributes[i].dataType.ToString();
                nameSpace.attributes[i] = ns;
            }
        }
        
        string value = JsonUtility.ToJson(new SerialisedNameSpace {nameSpaces = nameSpaces }, true);
        Debug.Log(value);

        File.WriteAllText(savePath, value);
        Debug.Log("Saved to:" + savePath);
    }

    public void DeSerialize()
    {
        string jsonString = File.ReadAllText(savePath);
        if (jsonString == "")
        {
            Debug.Log("No File found @ " + savePath);
            return;
        }
        SerialisedNameSpace obj = (SerialisedNameSpace) JsonUtility.FromJson(jsonString, typeof(SerialisedNameSpace));
        nameSpaces = obj.nameSpaces;
    }
}

public enum DataType { String, Int, Float, Vector2, Vector3, Matrix4x4, StringList, IntList, FloatList, Vector2List, Vector3List, Matrix4x4List };


[System.Serializable]
public class NameSpace
{
    public string prefix;
    public string uri;
    public List<Attribute> attributes;

    public void UpdateAttributeUris()
    {
        foreach (var attr in attributes)
        {
            attr.uri = uri;
        }
    }

}
[System.Serializable]
public class Attribute
{
    public string value;
    public DataType dataType;
    [HideInInspector]
    public string dataTypeString;
    [HideInInspector]
    public string uri = "";

    public Attribute(string val, DataType datatype = DataType.String, string baseUri = "undefined#")
    {
        value = val;
        dataType = datatype;
        uri = baseUri;
    }

    public RDFResource GetPredicate()
    {
        return new RDFResource(uri + value);
    }

    public RDFModelEnums.RDFDatatypes GetRDFType()
    {
        switch (dataType)
        {
            case DataType.Int:
                return RDFModelEnums.RDFDatatypes.XSD_INT;
            case DataType.Float:
                return RDFModelEnums.RDFDatatypes.XSD_FLOAT;
            default:
                return RDFModelEnums.RDFDatatypes.XSD_STRING;
        }
    }

}
