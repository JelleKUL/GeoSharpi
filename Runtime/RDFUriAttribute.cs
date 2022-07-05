using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;

[System.Serializable]
public class RDFUriAttribute : PropertyAttribute
{
    public string uri;
    public string prefix;
    public RDFModelEnums.RDFDatatypes type;

    public RDFUriAttribute(string _prefix, string _uri, RDFModelEnums.RDFDatatypes _type = RDFModelEnums.RDFDatatypes.XSD_STRING)
    {
        prefix = _prefix;
        uri = _uri;
        type = _type;
    }
}
