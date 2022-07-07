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

        public Texture2D imageTexture;

        [Header("Visual Parameters")]
        [Tooltip("The offset distance of the Image plane (m)")]
        public float displayDistance = 1;
        public Shader textureShader;

        public ImageNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);

            
        }

        public override GameObject GetResourceObject()
        {
            //CameraFovLogger cameraLogger = newObj.AddComponent<CameraFovLogger>();
            GameObject imageChild = GameObject.CreatePrimitive(PrimitiveType.Quad);

            float ratio = imageTexture.width / (float)imageTexture.height;
            float aspectAngle = Mathf.Atan(1 / ratio);
            float imageDiagonal = 35.0f / focalLengthIn35mmFilm * displayDistance * 1000;


            float height = Mathf.Sin(aspectAngle) * imageDiagonal;
            imageChild.transform.localScale = new Vector3(ratio, 1, 1) * height;

            Renderer quadRenderer = imageChild.GetComponent<Renderer>();
            quadRenderer.material = new Material(textureShader);
            quadRenderer.sharedMaterial.SetTexture("_MainTex", imageTexture);

            //cameraLogger.aspect = ratio;
            //cameraLogger.distance = distance;
            //cameraLogger.fov = simpleTransform.fov;

            imageChild.transform.position = Vector3.forward * displayDistance;

            return imageChild;
        }

    }
}