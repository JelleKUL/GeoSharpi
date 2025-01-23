using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using SFB;
using GeoSharpi;
using GeoSharpi.Interaction;
using UnityEngine.SceneManagement;

public class VoxelUIManager : MonoBehaviour
{
    [SerializeField]
    private VoxelDrawer voxelDrawer;
    
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private TMP_Text sliderValue;
    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeSliderMaxValue(int val){
         slider.maxValue = val;
    }

    public void ChangeSliderValueDisplay(Single value){
        sliderValue.text = value.ToString();
    }

    public void GetAssetPath(){
        string[] file = StandaloneFileBrowser.OpenFilePanel("Open Mesh File", "", "obj", false);

        if(voxelDrawer){
            voxelDrawer.PlaceMesh(file[0]);
        }
    }

    public void ImportVoxels(){
        string[] file = StandaloneFileBrowser.OpenFilePanel("Open Voxelgrid File", "", "txt", false);

        if(voxelDrawer){
            voxelDrawer.ReadDataFromFile(file[0]);
        }
    }

    public void ExportVoxels(){
        string file = StandaloneFileBrowser.SaveFilePanel("Save Voxelgrid", "", "voxelData", "txt");

        if(voxelDrawer){
            voxelDrawer.WriteDataToFile(file);
        }
    }

    public void ResetScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
