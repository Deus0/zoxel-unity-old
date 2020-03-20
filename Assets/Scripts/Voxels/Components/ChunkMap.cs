using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections.Generic;


namespace Zoxel
{
	public struct ChunkMap : IComponentData
	{
		// state data
		public byte dirty;
		public byte isBiome;
		// for texture map
		public int width;
		public int height;
		// for brightness
		public int highestHeight;
		public int3 chunkPosition;
		// voxel data
		public BlitableArray<byte> topVoxels;   // x * z chunk slice
		public BlitableArray<byte> heights;   // x * z chunk slice

		public void Initialize(int width_, int height_)
		{
			width = width_;
			height = height_;
			topVoxels = new BlitableArray<byte>(width * height, Allocator.Persistent);
			heights = new BlitableArray<byte>(width * height, Allocator.Persistent);
		}
	}
}