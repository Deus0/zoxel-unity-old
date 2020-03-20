using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

using Unity.Rendering;

namespace Zoxel
{
	public struct FaceCameraComponent : IComponentData
	{
		//public byte thing;
        public float3 position;
	}
}