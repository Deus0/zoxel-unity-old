
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

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct ZoxelVertex
	{
		public float3 position;
		public float3 color;
		public float2 uv;
	}
}