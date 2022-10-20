using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeoSharpi.Nodes;
using GeoSharpi.Utils.Events;
using GeoSharpi.Utils;
using GeoSharpi.Visualisation;

namespace GeoSharpi.Capture
{
    public class BaseCameraCapture : MonoBehaviour
    {

        [Header("Spawn Parameters")]
        [SerializeField]
        public bool spawnInScene = false;
        public ImageNodeEvent OnCaptureTaken = new ImageNodeEvent();

        protected Texture2D cameraTexture;

        [ContextMenu("Take Image")]
        public virtual void TakeCameraImage()
        {
            StartCoroutine(RecordFrame());
            
        }

        IEnumerator RecordFrame()
        {
            yield return new WaitForEndOfFrame();
            cameraTexture = ScreenCapture.CaptureScreenshotAsTexture();
            CreateNode();
        }

        public void CreateNode(Transform pos = null, float focalLength = 0)
        {
            ImageNode newImage = new ImageNode(
                cameraTexture,
                pos? pos.localToWorldMatrix: transform.localToWorldMatrix, 
                focalLength > 0? focalLength: Camera.main.Get35MillimeterFocalLength()
                );
            OnCaptureTaken.Invoke(newImage);

            if (spawnInScene)
            {
                GameObject nodeVisualiser = new GameObject();
                NodeVisualizer nodeVis =  nodeVisualiser.AddComponent<NodeVisualizer>();
                nodeVis.SetUpNode(newImage);
            }
        }

    }
    
}

