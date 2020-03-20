/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Rendering;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class TestRenderer : MonoBehaviour
{
    private Entity entitySpawn;
    public Mesh cubeMesh;
    public Material cubeMaterial;

	// Start is called before the first frame update
	void Start()
	{
		// create the renderer
		EntityManager entityManager =Unity.Entities.World.Active.EntityManager;
		entitySpawn = entityManager.CreateEntity();
		RenderMesh renderer = new RenderMesh
		{
			mesh = cubeMesh,
			material = cubeMaterial,
			subMesh = 0,
			castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
			receiveShadows = true
		};
        Translation position = new Translation { Value = new float3(transform.position.x, transform.position.y, transform.position.z) };
		Rotation rotation = new Rotation { Value = new quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w) };
		Scale scale = new Scale { Value = transform.lossyScale.x };// new float3(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) };

        entityManager.AddSharedComponentData(entitySpawn, renderer);
        entityManager.AddComponentData<Translation>(entitySpawn, position);
        entityManager.AddComponentData<Rotation>(entitySpawn, rotation);
        entityManager.AddComponentData<Scale>(entitySpawn, scale);
    }

    private void Update()
    {
        transform.eulerAngles += new Vector3(0, 0.1f, 0);
        EntityManager entityManager =Unity.Entities.World.Active.EntityManager;
        Rotation rotation = new Rotation { Value = new quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w) };
        entityManager.SetComponentData<Rotation>(entitySpawn, rotation);
    }
}
*/