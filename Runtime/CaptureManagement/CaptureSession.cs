using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeoSharpi
{
    public class CaptureSession
    {
        public SessionNode session;
        public List<ImageNode> imageNodes;
        public List<GeometryNode> geometryNodes;
        /// <summary>
        /// The instantiator for a new session
        /// automatically creates a new folder with the current timestamp.
        /// </summary>
        public CaptureSession()
        {
            //session = new Node()
            
            //sessionData.sessionId = "session-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            //System.IO.Directory.CreateDirectory(Application.persistentDataPath + System.IO.Path.DirectorySeparatorChar + sessionData.sessionId);
        }

        /// <summary>
        /// Adds the image to the session and sends a save request to the ImageSaver
        /// </summary>
        /// <param name="simpleTransform">The transform of the image</param>
        /// <param name="imageTexture">The 2D captured camera texture</param>
        /// <param name="quality">The quality of the Jpeg compression</param>
        public void SaveImage(Node node, Texture2D imageTexture, int quality = 75)
        {

        }

        /// <summary>
        ///  Adds the mesh to the session and sends a save request to the ObjExporter
        /// </summary>
        /// <param name="mesh">the mesh to save to obj and session</param>
        public void SaveMesh(Mesh mesh)
        {

        }

        public void UpdateGraph()
        {

        }



    }



}
