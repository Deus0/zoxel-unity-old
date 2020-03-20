using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;

namespace Zoxel
{
	/// <summary>
	/// This is what draws things
	/// 
	/// Todo: Create a special Render type - with multiple materials - and draw hat - then for all sub meshes - simply draw them
	/// or add multiple entities per mesh created
	/// </summary>
	/*public class RenderSystem : ComponentSystem
	{
		ComponentGroup m_Group;

		protected override void OnCreate()
		{
			base.OnCreate();
			m_Group = GetComponentGroup(typeof(Translation), typeof(Scale), typeof(Rotation), typeof(RenderMesh));
		}

		protected override void OnUpdate()
		{
			var renderData = m_Group.GetSharedComponentDataArray<RenderMesh>();
			var positionData = m_Group.GetComponentDataArray<Translation>();
			var scaleData = m_Group.GetComponentDataArray<Scale>();
			var rotationData = m_Group.GetComponentDataArray<Rotation>();

			for (var i = 0; i < renderData.Length; i++)
			{
				Matrix4x4 mat = Matrix4x4.TRS(new Vector3(positionData[i].Value.x, positionData[i].Value.y, positionData[i].Value.z),
					new Quaternion(rotationData[i].Value.value.x, rotationData[i].Value.value.y, rotationData[i].Value.value.z, rotationData[i].Value.value.w),
					new Vector3(scaleData[i].Value.x, scaleData[i].Value.y, scaleData[i].Value.z));
				//Matrix4x4 mat = Matrix4x4.Translate(new Vector3(positionData[i].Value.x, positionData[i].Value.y, positionData[i].Value.z));
				//mat
				//var position = math.m(positionData[i].Value); // math.translate
				var data = renderData[i];

				Graphics.DrawMesh(data.mesh, mat, data.material, 0);
			}
		}
	}*/
}