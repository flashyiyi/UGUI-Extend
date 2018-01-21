using System;
using UnityEngine;
namespace UGUIExtend
{
    [Serializable]
    public class UIBakeMeshAsset : ScriptableObject
    {
        [SerializeField] public Mesh mesh;
        [SerializeField] public Sprite sprite;
    }
}