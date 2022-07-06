using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;

namespace GeoSharpi
{
    [System.Serializable]
    public class ImageNode : Node
    {
        [RDFUri("exif", "http://www.w3.org/2003/12/exif/ns#", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
        public float focalLengthIn35mmFilm = 0.0f;

        public ImageNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

    }
}