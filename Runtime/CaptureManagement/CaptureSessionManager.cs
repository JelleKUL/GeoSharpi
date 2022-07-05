using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;

namespace GeoSharpi
{

    public class CaptureSessionManager : MonoBehaviour
    {
        private CaptureSession assetSession;
        [SerializeField]
        [Tooltip("The url of the post request destination")]
        private string dataPostUrl = "";
        [SerializeField]
        private List<Node> nodes = new List<Node>();

        [ContextMenu("Check nodes")]
        public void checkNodes()
        {
            foreach (var node in nodes)
            {
                RDFGraph newGraph = node.ToGraph();
                newGraph.ToFile(RDFModelEnums.RDFFormats.Turtle, "Assets/RDF/" + node.subject.Replace("http://", "") + ".ttl");

                var triplesEnum = newGraph.TriplesEnumerator;

                while (triplesEnum.MoveNext())
                {
                    Debug.Log("Subject: " + triplesEnum.Current.Subject);

                    string pred = triplesEnum.Current.Predicate.ToString();
 
                    Debug.Log("Predicate: " + pred);

                    Debug.Log("Object: " + triplesEnum.Current.Object);
                }
            }
        }

        /// <summary>
        /// Saves the Image to the current assetSession
        /// </summary>
        /// <param name="simpleTransform">The transform of the image</param>
        /// <param name="imageTexture">The 2D captured camera texture</param>
        /// <param name="quality">The quality of the Jpeg compression</param>
        public void SaveImage(Node node, Texture2D imageTexture, int quality = 75)
        {
            CheckSession();
            //assetSession.SaveImage(simpleTransform, imageTexture, quality);
        }

        /// <summary>
        /// Saves the mesh to the current assetSession
        /// </summary>
        /// <param name="mesh">the mesh to save</param>
        public void SaveMesh(Node node, Mesh mesh)
        {
            CheckSession();
            assetSession.SaveMesh(mesh);
        }

        /// <summary>
        /// Creates a new session to store the data
        /// </summary>
        public void CreateNewSession()
        {
            assetSession = new CaptureSession();

            Matrix4x4 mat = transform.localToWorldMatrix;
        }

        /// <summary>
        /// Sends the current session to the specified server
        /// </summary>
        public void SendSessionToServer()
        {
            if (assetSession == null)
            {
                Debug.Log("there is no active session, nothing to send to the server");
                return;
            }

            // create a datastream to send the data to the server
            //StartCoroutine(UploadSession());
        }
        /*
        IEnumerator UploadSession()
        {
            // todo compress the folder to send all the data at once
            // for now we will send the json file as a test

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection(assetSession.GetJsonString()));

            UnityWebRequest www = UnityWebRequest.Post(dataPostUrl, formData);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("file upload succes");
            }

        }
        */
        /// <summary>
        /// Checks if there is an existing session. If not, creates a new session
        /// </summary>
        void CheckSession()
        {
            Debug.Log("Checking session");

            if (assetSession == null)
            {
                assetSession = new CaptureSession();

            }
        }
    }

}