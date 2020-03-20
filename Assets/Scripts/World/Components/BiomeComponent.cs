using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel.WorldGeneration
{
	/// <summary>
	/// stores an array of biome indexes for each cell in the chunk
	/// </summary>
	public struct Biome : IComponentData
	{
		public BlitableArray<byte> biomes;
		public BlitableArray<float> blends; 
		// shuold be between 0 and 1

		public void Dispose()
		{
			biomes.Dispose();
			blends.Dispose();
		}
	}
}