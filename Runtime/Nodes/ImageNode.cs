using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.IO;

namespace GeoSharpi
{
    [System.Serializable]
    public class ImageNode : Node
    {
        [RDFUri("exif", "http://www.w3.org/2003/12/exif/ns#", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
        public float focalLengthIn35mmFilm = 0.0f;

        public Texture2D imageTexture;

        [Header("Visual Parameters")]
        [Tooltip("The offset distance of the Image plane (m)")]
        public float displayDistance = 1;
        public Shader textureShader;

        [HideInInspector]
        public int saveQuality = 75;

        public ImageNode() { CreateEmptyNode(); }

        public ImageNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);

            
        }

        public ImageNode(Texture2D cameraTexture, Matrix4x4 cameraTransform, float focalLength35mm)
        {
            imageTexture = cameraTexture;
            cartesianTransform = cameraTransform;
            focalLengthIn35mmFilm = focalLength35mm;

            CreateNode();
        }

        public override GameObject GetResourceObject()
        {
            //CameraFovLogger cameraLogger = newObj.AddComponent<CameraFovLogger>();
            GameObject imageChild = GameObject.CreatePrimitive(PrimitiveType.Quad);

            if (!imageTexture)
            {
                imageTexture = ImageIO.LoadImage(path);
                Debug.LogWarning("No ImageTexture is provided, skipping Placement");
                return imageChild;
            }

            float sensor35mmDiagonal = Mathf.Sqrt(36 * 36 + 24 * 24); // the actual diagonal of a 35mm sensor
            float ratio = imageTexture.width / (float)imageTexture.height; //the Width to height ration
            float aspectAngle = Mathf.Atan(1 / ratio); //The angle of the diagonal to the horizon
            float imageDiagonal = sensor35mmDiagonal / focalLengthIn35mmFilm * displayDistance; // The full length of the  projected Image diagonal
            float height = Mathf.Sin(aspectAngle) * imageDiagonal; // the full height of the projected image
            imageChild.transform.localScale = new Vector3(ratio, 1, 1) * height;

            Renderer quadRenderer = imageChild.GetComponent<Renderer>();
            if (!textureShader) textureShader = Shader.Find("Standard"); //uses the default shader
            quadRenderer.material = new Material(textureShader);
            quadRenderer.sharedMaterial.SetTexture("_MainTex", imageTexture);

            //cameraLogger.aspect = ratio;
            //cameraLogger.distance = distance;
            //cameraLogger.fov = simpleTransform.fov;

            imageChild.transform.position = Vector3.forward * displayDistance;

            return imageChild;
        }

        public override void LoadResource(string folderPath)
        {
            Debug.Log("Looking for image at:" + Path.Combine(folderPath, path));
            imageTexture = ImageIO.LoadImage(Path.Combine(folderPath,path));
        }

        public override void SaveResource(string rootFolder = "")
        {
            if (rootFolder == "" || rootFolder == null) rootFolder = Application.persistentDataPath;
            string relativePath = GetName() + ".jpg";
            string savepath = Path.Combine(rootFolder, relativePath);
            ImageIO.SaveImage(imageTexture, savepath);
            path = relativePath;
        }
    }
}