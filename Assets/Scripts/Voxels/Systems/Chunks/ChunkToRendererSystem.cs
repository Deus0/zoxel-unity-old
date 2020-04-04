using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;

namespace Zoxel.Voxels
{
	/// <summary>
	/// Push voxel data from Chunks to ChunkRenders!
	/// </summary>
	[DisableAutoCreation, UpdateAfter(typeof(ChunkSidesSystem))]
    public class ChunkToRendererSystem : ComponentSystem
	{
		public ChunkSpawnSystem chunkSpawnSystem;
		public ChunkRenderSystem chunkRenderSystem;
		public Dictionary<int, VoxelDatam> meta;
		public List<int> voxelIDs;

		protected override void OnUpdate()
		{
            Entities.WithAll<ChunkBuilder>().ForEach((Entity e, ref ChunkBuilder chunkBuilder) =>
            {
                if (chunkBuilder.state == 1)
				{
					UpdateChunk(e);
					World.EntityManager.RemoveComponent<ChunkBuilder>(e);
				}
			});
		}

		public void UpdateChunk(Entity chunkEntity)
		{
			Chunk chunk = World.EntityManager.GetComponentData<Chunk>(chunkEntity);
			if (ChunkSpawnSystem.isDebugLog)
			{
				UnityEngine.Debug.LogError("Telling Chunk's Renderers to start updating at: " + chunk.Value.chunkPosition + "::" + chunk.Value.voxelDimensions);
			}
			// later for all renders, do this
			for (int i = 0; i < chunk.chunkRenders.Length; i++)
			{
				Entity chunkRenderEntity = chunk.chunkRenders[i];//chunkSpawnSystem.chunkRenders[chunk.chunkRenders[i]];
				if (World.EntityManager.HasComponent<ChunkRenderer>(chunkRenderEntity))
				{
					ChunkRenderer chunkRenderer = World.EntityManager.GetComponentData<ChunkRenderer>(chunkRenderEntity);
					chunkRenderer.Value = chunk.Value;
					World.EntityManager.SetComponentData(chunkRenderEntity, chunkRenderer);
				}
				else
				{
					chunkRenderSystem.AddChunkRenderComponent(chunkRenderEntity, chunkEntity, ref chunk, 0, 0);
				}
				if (World.EntityManager.HasComponent<ChunkRendererBuilder>(chunkRenderEntity))
				{
					World.EntityManager.SetComponentData(chunkRenderEntity, new ChunkRendererBuilder { state = 1 });
				}
				else
				{
					World.EntityManager.AddComponentData(chunkRenderEntity, new ChunkRendererBuilder { state = 1 }); //.AddChunkRenderComponent(chunkRenderEntity, ref chunk, 0, 0, 1);
				}
			}
		}
	}
}