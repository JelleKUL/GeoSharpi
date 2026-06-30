// Assets/Editor/CsdfImporter.cs
using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;

[ScriptedImporter(1, "csdf")]
public class CsdfImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        byte[] bytes = File.ReadAllBytes(ctx.assetPath);
        var asset = new TextAsset(System.Text.Encoding.UTF8.GetString(bytes));
        // Note: TextAsset stores bytes internally; .bytes property still gives raw data back
        ctx.AddObjectToAsset("main", asset);
        ctx.SetMainObject(asset);
    }
}