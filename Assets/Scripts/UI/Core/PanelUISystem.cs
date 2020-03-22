using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;

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
                if (panelUI.dirty == 1)
                {
                    panelUI.dirty = 0;
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
                    RenderBounds b = new RenderBounds
                    {
                        Value = new AABB
                        {
                            Extents = new float3(panelSize.x, panelSize.y, 0.5f)
                        }
                    };
                    EntityManager.SetComponentData(e, b);
                    panelUI.outlineDirty = 1;
                }
                
                if (panelUI.outlineDirty == 1)
                {
                    panelUI.outlineDirty = 0;
                    if (World.EntityManager.HasComponent<OutlineLink>(e))
                    {
                        float2 panelSize = panelUI.size;
                        RenderBounds b = new RenderBounds
                        {
                            Value = new AABB
                            {
                                Extents = new float3(panelSize.x, panelSize.y, 0.5f)
                            }
                        };
                        //Debug.LogError("Updating Outline Render Mesh.");
                        var outlineLink = World.EntityManager.GetComponentData<OutlineLink>(e);
                        var outline = outlineLink.outline;
                        var outlineRenderMesh = World.EntityManager.GetSharedComponentData<Unity.Rendering.RenderMesh>(outline);
                        Mesh mesh2 = MeshUtilities.CreateReverseQuadMesh(panelSize, 0.004f);
                        outlineRenderMesh.mesh = mesh2;
                        outlineRenderMesh.material = uiData.defaultPlayerOutline;
                        World.EntityManager.SetSharedComponentData(outline, outlineRenderMesh);
                        World.EntityManager.SetComponentData(outline, b);
                    }
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
