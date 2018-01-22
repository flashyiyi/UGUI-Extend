using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SubjectNerd.PsdImporter.PsdParser;

public class UIPSDViewer : MonoBehaviour
{
    public Object asset;
    void Start()
    {
        using (PsdDocument psd = PsdDocument.Create(AssetDatabase.GetAssetPath(asset)))
        {
            foreach (var layer in psd.Childs)
            {
                Debug.Log(layer.MergeChannels());
            }
        }
    }
}
