using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Zoxel
{

    [DisableAutoCreation]
    public class PanelUISystem : ComponentSystem
    {
        public UIDatam uiData;

        protected override void OnUpdate()
        {
            Entities.WithAll<PanelUI>().ForEach((Entity e, ref PanelUI panelUI) =>
            {
                if (panelUI.updated == 1)
                {
                    panelUI.updated = 0;
                    // , ref GridUI gridUI
                    // resize panel
                    float2 panelSize = panelUI.size; // etGridPanelSize(gridUI.gridSize, gridUI.iconSize, gridUI.margins, gridUI.padding);
                    /*Debug.LogError("Setting children: " + childrens.children.Length + " for grid."
                        + " Panel Size is: " + panelSize
                        + ". Icon Size is: " + gridUI.iconSize);*/
                    UIUtilities.UpdateOrbiter(World.EntityManager, e, GetOrbitPosition(panelUI), uiData.orbitLerpSpeed);
                    Mesh mesh = MeshUtilities.CreateQuadMesh(panelSize);
                    var renderMesh = World.EntityManager.GetSharedComponentData<Unity.Rendering.RenderMesh>(e);
                    renderMesh.mesh = mesh;
                    World.EntityManager.SetSharedComponentData(e, renderMesh);
                }
            });
        }

        // when resizing panel, must reposition it in screen
        /*protected virtual float GetOrbitDepth() { return 0.5f; }
        protected virtual UIAnchoredPosition GetAnchor() { return UIAnchoredPosition.Middle; }*/

        protected float3 GetOrbitPosition(PanelUI panel) // panelSize, GridUI grid)
        {
            //float3 orbitPosition = uiDatam.skillbarOrbitPosition;
            float3 orbitPosition = UIUtilities.GetOrbitAnchors(
                (UIAnchoredPosition)panel.anchor,
                new float3(panel.positionOffset.x, panel.positionOffset.y, panel.orbitDepth),
                panel.size);
            return orbitPosition;
        }

    }
}
