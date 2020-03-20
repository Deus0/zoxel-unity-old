using Unity.Entities;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Core;
using UnityEngine;
using Unity.Collections;

namespace Zoxel
{
    public struct PanelNavigation : IComponentData
    {
        public byte selectedChildIndex;
    }

}