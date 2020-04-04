using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public class WorldBoundComponent : ComponentDataProxy<WorldBound> { }

    [System.Serializable]
    public struct WorldBound : IComponentData
    {
        public byte enabled;
        // Voxel World Binding
        public Entity world;
        public int3 voxelDimensions;
        public float3 lastNoise;
        public float4x4 worldTransform;
        // character
        public float3 size;
        public int3 voxelPosition;
        public int3 chunkPosition;
        public int3 voxelPositionForward;
        public int3 voxelPositionBack;
        public int3 voxelPositionLeft;
        public int3 voxelPositionRight;
        public byte voxelTypeForward;
        public byte voxelTypeBack;
        public byte voxelTypeLeft;
        public byte voxelTypeRight;
        public byte voxelTypeForwardBelow;
        public byte voxelTypeBackBelow;
        public byte voxelTypeLeftBelow;
        public byte voxelTypeRightBelow;

        public bool IsInsideSolids()
        {
            return (voxelTypeForward != 0 ||
                voxelTypeBack != 0 ||
                voxelTypeLeft != 0 ||
                voxelTypeRight != 0);
        }
        public bool IsNoSolidsUnderneath()
        {
            return (//!IsInsideSolids() && 
                voxelTypeForwardBelow == 0 &&
                voxelTypeBackBelow == 0 &&
                voxelTypeLeftBelow == 0 &&
                voxelTypeRightBelow == 0);
        }
    }
}
