using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel.Voxels
{
	public struct ChunkData
	{
		public int3 chunkPosition;
		public float3 worldScale;
		public int3 voxelDimensions;  // 16 16 16

		public BlitableArray<byte> voxels;
		public BlitableArray<byte> voxelsUp;
		public BlitableArray<byte> voxelsDown;
		public BlitableArray<byte> voxelsLeft;
		public BlitableArray<byte> voxelsRight;
		public BlitableArray<byte> voxelsForward;
		public BlitableArray<byte> voxelsBack;


		public void Dispose()
		{
			if (voxels.Length > 0) {
				voxels.Dispose();
			}
			if (voxelsUp.Length > 0) {
				voxelsUp.Dispose();
				voxelsDown.Dispose();
				voxelsLeft.Dispose();
				voxelsRight.Dispose();
				voxelsForward.Dispose();
				voxelsBack.Dispose();
			}
		}

	}

	public struct Chunk : IComponentData
	{
		// remove these for zoxIDs
        public int id;
		public Entity world;
        public int worldID;

		public byte isGenerating;   // used in side system to know what chunks are still generating

		public ChunkData Value;
		public BlitableArray<int> chunkRenders;
		public byte isMapDirty;
		public byte isWeights;

		// 16 x 16
		public int indexUp;
		public int indexDown;
		public int indexLeft;
		public int indexRight;
		public int indexForward;
		public int indexBack;

		public byte hasUpdatedUp;
		public byte hasUpdatedDown;
		public byte hasUpdatedLeft;
		public byte hasUpdatedRight;
		public byte hasUpdatedForward;
		public byte hasUpdatedBack;

		public int3 GetVoxelPosition()
        {
            return new int3((int)(Value.chunkPosition.x * Value.voxelDimensions.x),
                (int)(Value.chunkPosition.y * Value.voxelDimensions.y), 
                (int)(Value.chunkPosition.z * Value.voxelDimensions.z));
		}
		public void InitializeData(int3 voxelDimensions)
		{
			int xyzSize = (int)(voxelDimensions.x * voxelDimensions.y * voxelDimensions.z);
			int xzSize = (int)(voxelDimensions.x * voxelDimensions.z);
			int yzSize = (int)(voxelDimensions.y * voxelDimensions.z);
			int xySize = (int)(voxelDimensions.x * voxelDimensions.y);
			Value.voxels = new BlitableArray<byte>(xyzSize, Allocator.Persistent);
			Value.voxelsUp = new BlitableArray<byte>(xzSize, Allocator.Persistent);
			Value.voxelsDown = new BlitableArray<byte>(xzSize, Allocator.Persistent);
			Value.voxelsLeft = new BlitableArray<byte>(yzSize, Allocator.Persistent);
			Value.voxelsRight = new BlitableArray<byte>(yzSize, Allocator.Persistent);
			Value.voxelsForward = new BlitableArray<byte>(xySize, Allocator.Persistent);
			Value.voxelsBack = new BlitableArray<byte>(xySize, Allocator.Persistent);
		}

		public void Dispose()
		{
			Value.Dispose();
			chunkRenders.Dispose();
		}
		
		public static void Destroy(EntityManager entityManager, Entity e)
		{
			if (entityManager.Exists(e))
			{
				if (entityManager.HasComponent<Chunk>(e))
				{
					Chunk chunk = entityManager.GetComponentData<Chunk>(e);
					chunk.Dispose();
				}
				entityManager.DestroyEntity(e);
			}
		}
	}
}