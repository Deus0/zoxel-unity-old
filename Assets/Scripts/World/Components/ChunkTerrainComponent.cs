using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel.WorldGeneration
{
	public struct ChunkTerrain : IComponentData
	{
		//public byte hasBuiltTerrain;
		public BlitableArray<int> heights;
		[ReadOnly]
		public int3 chunkPosition;
		[ReadOnly]
		public BlitableArray<BiomeData> biomes;

		// settings
		/*public byte dirtMetaID;
		public float landAmplitude;
		public float landBase;
		public float landScale;*/    // noise scale - 0.008
	}
}