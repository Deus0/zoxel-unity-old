using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel.Voxels
{
	public struct Chunk : IComponentData
	{
		// links
		public Entity world;
		public BlitableArray<Entity> chunkRenders;
		// data
		public ChunkData Value;
		// states
		public byte isGenerating;   // used in side system to know what chunks are still generating
		public byte isMapDirty;
		public byte isWeights;

		// replace these with Entity indexes
		public int indexUp;
		public int indexDown;
		public int indexLeft;
		public int indexRight;
		public int indexForward;
		public int indexBack;
		// are these used?
		public byte hasUpdatedUp;
		public byte hasUpdatedDown;
		public byte hasUpdatedLeft;
		public byte hasUpdatedRight;
		public byte hasUpdatedForward;
		public byte hasUpdatedBack;
		
		public static void Destroy(EntityManager EntityManager, Entity e)
		{
			if (EntityManager.Exists(e))
			{
				if (EntityManager.HasComponent<Chunk>(e))
				{
					Chunk chunk = EntityManager.GetComponentData<Chunk>(e);
					for (int i = 0; i < chunk.chunkRenders.Length; i++)
					{
						if (EntityManager.Exists(chunk.chunkRenders[i]))
						{
							EntityManager.DestroyEntity(chunk.chunkRenders[i]);
						}
					}
					chunk.Dispose();
				}
				EntityManager.DestroyEntity(e);
			}
		}

		public int3 GetVoxelPosition()
        {
            return new int3((int)(Value.chunkPosition.x * Value.voxelDimensions.x),
                (int)(Value.chunkPosition.y * Value.voxelDimensions.y), 
                (int)(Value.chunkPosition.z * Value.voxelDimensions.z));
		}

		public void Init(int3 voxelDimensions)
		{
			int xyzSize = (int)(voxelDimensions.x * voxelDimensions.y * voxelDimensions.z);
			Value.voxels = new BlitableArray<byte>(xyzSize, Allocator.Persistent);
			int xzSize = (int)(voxelDimensions.x * voxelDimensions.z);
			Value.voxelsUp = new BlitableArray<byte>(xzSize, Allocator.Persistent);
			Value.voxelsDown = new BlitableArray<byte>(xzSize, Allocator.Persistent);
			int yzSize = (int)(voxelDimensions.y * voxelDimensions.z);
			Value.voxelsLeft = new BlitableArray<byte>(yzSize, Allocator.Persistent);
			Value.voxelsRight = new BlitableArray<byte>(yzSize, Allocator.Persistent);
			int xySize = (int)(voxelDimensions.x * voxelDimensions.y);
			Value.voxelsForward = new BlitableArray<byte>(xySize, Allocator.Persistent);
			Value.voxelsBack = new BlitableArray<byte>(xySize, Allocator.Persistent);
		}

		public void Dispose()
		{
			Value.Dispose();
			if (chunkRenders.Length > 0)
			{
				chunkRenders.Dispose();
			}
		}
	}
}