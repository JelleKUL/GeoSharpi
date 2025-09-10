using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SphericalImageCapture : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField]
    private bool updateTexture = true;
    [Header("Prefabs")]
    [SerializeField]
    private Camera targetCam;
    [SerializeField]
    private RenderTexture targetTexture;
    [SerializeField]
    private RenderTexture equiTexture;

    private bool updatingTexture = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        if (updateTexture) UpdateTexture();
    }

    [ContextMenu("Update Texture")]
    public async void UpdateTexture()
    {
        if (!updatingTexture)
        {
            updatingTexture = true;
            await RenderToTexture();
            updatingTexture = false;
        }
    }

    public async Task<bool> RenderToTexture()
    {
        if(targetCam && targetTexture)
        {
            targetCam.stereoSeparation = 0f;
            targetCam.RenderToCubemap(targetTexture, 63, Camera.MonoOrStereoscopicEye.Left);
            targetTexture.ConvertToEquirect(equiTexture, Camera.MonoOrStereoscopicEye.Mono);
        }
        return true;
    }
}
