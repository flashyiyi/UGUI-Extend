using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIExtend
{
    [AddComponentMenu("UI/UIBakeMeshCreater", 12)]
    public class UIBakeMeshCreater : Graphic
    {
        [SerializeField]
        public UIBakeMeshAsset asset;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying)
                BatchAll();
            
        }

        protected override void UpdateGeometry()
        {
        }

        private List<CombineInstance> tempCombines = new List<CombineInstance>();
        public void BatchAll()
        {
            if (asset == null)
                return;
            

            Image[] graphics = GetComponentsInChildren<Image>(true);
            int count = graphics.Length;

            asset.sprite = null;
            int index = 0;
            for (int i = 0;i < count;i++)
            {
                Image g = graphics[i];
                if (g == this)
                    continue;

                if (asset.sprite == null)
                {
                    asset.sprite = g.sprite;
                }
                else if (asset.sprite.texture != g.sprite.texture)
                {
                    continue;
                }

                CombineInstance combine;
                if (index < tempCombines.Count)
                {
                    combine = tempCombines[index];
                }
                else
                {
                    combine = new CombineInstance();
                    combine.mesh = new Mesh();
                    tempCombines.Add(combine);
                }

                BakeMesh(g, combine.mesh);
                combine.transform = transform.worldToLocalMatrix * g.transform.localToWorldMatrix;
                tempCombines[index] = combine;

                index++;
            }

            tempCombines.RemoveRange(index, tempCombines.Count - index);
            
            asset.mesh.CombineMeshes(tempCombines.ToArray(),true, true);
            asset.mesh.RecalculateBounds();
        }

        private void BakeMesh(Graphic graphic, Mesh mesh)
        {
            var methodInfo = graphic.GetType().GetMethod("UpdateGeometry", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo != null)
                methodInfo.Invoke(graphic, null);

            Mesh workMesh = Graphic.workerMesh;
            if (workMesh == null)
            {
                mesh.Clear();
                return;
            }
            
            mesh.vertices = workerMesh.vertices;
            mesh.colors = workerMesh.colors;
            mesh.uv = workerMesh.uv;
            //mesh.uv2 = workerMesh.uv2;
            //mesh.normals = workerMesh.normals;
            //mesh.tangents = workerMesh.tangents;
            mesh.triangles = workerMesh.triangles;
        }
    }
}