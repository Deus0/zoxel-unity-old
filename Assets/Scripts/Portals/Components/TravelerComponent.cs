using Unity.Entities;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;
using System;

namespace Zoxel
{
    [Serializable]
    public class TravelerComponent : ComponentDataProxy<Traveler> { }

    [Serializable]
    public struct Traveler : IComponentData
    {
        public float lastTeleported;
        public byte hasChecked;
        public Entity portal;
        public float lastCheckedClosestPortal;
        public int portalSide;
        public float distanceToPortal;
        public byte isOverlappingPortalBounds;
        public float3 originalPortalPosition;
    }
}