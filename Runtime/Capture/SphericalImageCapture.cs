using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalImageCapture : MonoBehaviour
{
    [SerializeField]
    private Camera targetCam;
    [SerializeField]
    private RenderTexture targetTexture;
    [SerializeField]
    private RenderTexture equiTexture;
    [SerializeField]
    private bool UpdateTexture = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        if (UpdateTexture) RenderToTexture();
    }

    [ContextMenu("Render To Texture")]
    public void RenderToTexture()
    {
        if(targetCam && targetTexture)
        {
            targetCam.stereoSeparation = 0f;
            targetCam.RenderToCubemap(targetTexture, 63, Camera.MonoOrStereoscopicEye.Left); //or right, its the same
            targetTexture.ConvertToEquirect(equiTexture, Camera.MonoOrStereoscopicEye.Mono);

            //targetCam.RenderToCubemap(targetTexture);
            //targetTexture.ConvertToEquirect(equiTexture);
        }
    }
}
