using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

namespace Zoxel
{


    // Gives cameras the character data
    // this data is used for orbitors?
    [DisableAutoCreation]
    //[UpdateAfter(typeof(PositionalForceSystem))]
    public class CharacterToCameraSystem : ComponentSystem
    {
        //public CharacterSpawnSystem characterSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<CharacterLink>().ForEach((Entity e, ref CharacterLink characterToCamera) => // , ref RenderMesh renderer
            {
                // get character
                if (World.EntityManager.Exists(characterToCamera.character)) //characterToCamera.characterID != 0 && characterSpawnSystem.characters.ContainsKey(characterToCamera.characterID))
                {
                    Entity character = characterToCamera.character; //characterSpawnSystem.characters[characterToCamera.characterID];
                    // get position and rotation
                    characterToCamera.position = World.EntityManager.GetComponentData<Translation>(character).Value;
                    // set head of character rotation X to camera one

                    // set character rotation Y to camera one
                    Rotation newCharacterRotation1 = World.EntityManager.GetComponentData<Rotation>(character);
                    Quaternion newCharacterRotation2 = new Quaternion(newCharacterRotation1.Value.value.x,
                        newCharacterRotation1.Value.value.y, newCharacterRotation1.Value.value.z, newCharacterRotation1.Value.value.w);
                    float3 newCharacterRotation3 = newCharacterRotation2.eulerAngles;

                    Rotation newCameraRotation1 = World.EntityManager.GetComponentData<Rotation>(e);
                    Quaternion newCameraRotation2 = new Quaternion(newCameraRotation1.Value.value.x,
                        newCameraRotation1.Value.value.y, newCameraRotation1.Value.value.z, newCameraRotation1.Value.value.w);
                    float3 newCameraRotation3 = newCameraRotation2.eulerAngles;

                    newCharacterRotation3.y = newCameraRotation3.y;
                    World.EntityManager.SetComponentData(character, new Rotation { Value = Quaternion.Euler(newCharacterRotation3)});
                    characterToCamera.rotation = World.EntityManager.GetComponentData<Rotation>(character).Value;
                }
            });
        }
    }
}