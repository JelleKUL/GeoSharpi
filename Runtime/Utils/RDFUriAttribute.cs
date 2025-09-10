using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;

namespace GeoSharpi.Utils
{
    /// <summary>
    /// A Custom property to add to variables in a script, this enables them to be serialized to RDF and contain the Correct namespace
    /// </summary>
    [System.Serializable]
    public class RDFUriAttribute : PropertyAttribute
    {
        /// <summary>
        /// The full namespace URI
        /// </summary>
        public string uri;
        /// <summary>
        ///  the short prefix to shorten the Uri
        /// </summary>
        public string prefix;
        /// <summary>
        /// The serialis ed name
        /// </summary>
        public string name;
        /// <summary>
        /// the type of the object, default to untyped plain literal
        /// </summary>
        public RDFModelEnums.RDFDatatypes type;

        /// <summary>
        /// A Custom Attribute to enable RDF Serialisation and Namespace definition
        /// </summary>
        /// <param name="_prefix">the short name for the uri</param>
        /// <param name="_uri">the full Namespace uri</param>
        /// <param name="_type">The type of variable, defaults to plain untyped literal</param>
        public RDFUriAttribute(string _prefix = "geomapi", string _uri = "https://w3id.org/geomapi#",string _name = "", RDFModelEnums.RDFDatatypes _type = RDFModelEnums.RDFDatatypes.XSD_STRING)
        {
            prefix = _prefix;
            uri = _uri;
            name = _name;
            type = _type;
        }
    }
}


