using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private bool updateTexture = true;
    private bool updatingTexture = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        if (updateTexture) UpdateTexture();
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) UpdateTexture(); // Update the scan outside of play mode
    }

    [ContextMenu("Update Texture")]
    public async void UpdateTexture()
    {
        if (!updateTexture) return;

        if (updateTexture && !updatingTexture)
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
            targetCam.RenderToCubemap(targetTexture, 63, Camera.MonoOrStereoscopicEye.Left); //or right, its the same
            targetTexture.ConvertToEquirect(equiTexture, Camera.MonoOrStereoscopicEye.Mono);

            //targetCam.RenderToCubemap(targetTexture);
            //targetTexture.ConvertToEquirect(equiTexture);
        }
        return true;
    }
}
