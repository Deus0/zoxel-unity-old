using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

namespace Zoxel
{
    // maybe another system sets up navigation! but using children / childrens children for it
    // set sizes up! depending in total size and rows/columns
    // include a scroll position!

    /// <summary>
    /// Grab Grid, child elements
    ///     reposition them inside itself
    ///     sets their size depending on how many we will see
    /// </summary>
    [DisableAutoCreation]
    public class GridUISystem : ComponentSystem
    {
        public UIDatam uiDatam;
        public static float depthPositionDifference = -0.001f;

        protected override void OnUpdate()
        {
            Entities.WithAll<PanelUI, GridUI, Childrens>().ForEach((Entity e, 
                ref PanelUI panelUI, ref GridUI gridUI, ref Childrens childrens) =>
            {
                if (gridUI.updated == 1)
                {
                    gridUI.updated = 0;
                    panelUI.size = gridUI.GetSize();
                    // set panel first
                    // set children positions
                    for (int i = 0; i < childrens.children.Length; i++)
                    {
                        // set positions up!
                        float3 elementPosition = GetGridPosition(i, gridUI.gridSize, gridUI.iconSize, gridUI.margins, gridUI.padding);
                        World.EntityManager.SetComponentData(childrens.children[i], new Translation { Value = elementPosition });
                    }
                    panelUI.updated = 1;
                    panelUI.navigationDirty = 1;
                }
            });
        }

        protected float2 GetGridPanelSize(float2 gridSize, float2 iconSize, float2 margins, float2 padding)
        {
            return new float2(gridSize.x * iconSize.x + (gridSize.x - 1) * padding.x + margins.x * 2f,
                gridSize.y * iconSize.y + (gridSize.y - 1) * padding.y + margins.y * 2f);
        }
        protected float3 GetGridPosition(int index, float2 gridSize, float2 iconSize, float2 margins, float2 padding)
        {
            int indexX = index % ((int)gridSize.x);
            int indexY = index / ((int)gridSize.x);
            return GetGridPosition(indexX, indexY, gridSize, iconSize, margins, padding);
        }
        protected float3 GetGridPosition(int indexX, int indexY, float2 gridSize, float2 iconSize, float2 margins, float2 padding)
        {
            float2 panelSize = GetGridPanelSize(gridSize, iconSize, margins, padding);
            return new float3(
                margins.x + indexX * (iconSize.x + padding.x) - (panelSize.x / 2f) + (iconSize.x / 2f),
                -margins.y - indexY * (iconSize.y + padding.y) + (panelSize.y / 2f) - (iconSize.y / 2f),
                depthPositionDifference);
        }
    }
}
