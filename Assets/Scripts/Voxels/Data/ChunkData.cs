using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel.Voxels
{
    
	public struct ChunkData
	{
		public int3 chunkPosition;
		public int3 voxelDimensions;  // 16 64 16
		public float3 worldScale;

		public BlitableArray<byte> voxels;
		public BlitableArray<byte> voxelsUp;
		public BlitableArray<byte> voxelsDown;
		public BlitableArray<byte> voxelsLeft;
		public BlitableArray<byte> voxelsRight;
		public BlitableArray<byte> voxelsForward;
		public BlitableArray<byte> voxelsBack;

		public void Dispose()
		{
			if (voxels.Length > 0)
            {
				voxels.Dispose();
			}
			if (voxelsUp.Length > 0) 
            {
				voxelsUp.Dispose();
				voxelsDown.Dispose();
				voxelsLeft.Dispose();
				voxelsRight.Dispose();
				voxelsForward.Dispose();
				voxelsBack.Dispose();
			}
		}

	}
}