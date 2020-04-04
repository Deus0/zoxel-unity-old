using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine.Rendering;

namespace Zoxel.Voxels
{
		
	public struct ChunkSides : IComponentData
	{
        // sides for each voxel - todo: make all 6 things use 2 ^ 6 = 64 different states for drawing different sides
        // use boolean byte operations to convert it 
		public BlitableArray<byte> sidesUp;
		public BlitableArray<byte> sidesDown;
		public BlitableArray<byte> sidesLeft;
		public BlitableArray<byte> sidesRight;
		public BlitableArray<byte> sidesBack;
		public BlitableArray<byte> sidesForward;

        public void Init(int3 voxelDimensions)
        {
			//Debug.LogError("Initiating chunksides: " + voxelDimensions);
			int xyzSize = (int)(voxelDimensions.x * voxelDimensions.y * voxelDimensions.z);
			sidesBack = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesDown = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesUp = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesForward = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesLeft = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
			sidesRight = new BlitableArray<byte>(xyzSize, Unity.Collections.Allocator.Persistent);
        }

        public void Dispose()
        {
			if (sidesUp.Length > 0)
            {
				sidesUp.Dispose();
				sidesDown.Dispose();
				sidesLeft.Dispose();
				sidesRight.Dispose();
				sidesBack.Dispose();
				sidesForward.Dispose();
			}
        }
    }
}