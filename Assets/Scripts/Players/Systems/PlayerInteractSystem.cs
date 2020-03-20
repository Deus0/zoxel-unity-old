using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Zoxel {
    
    public enum ControllerMapping {
        Character,
        // UIs
        Dialogue,
        PauseMenu,
        GameMenu
    }
    [DisableAutoCreation]
    public class PlayerInteractSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            Entities.WithAll<Controller, Targeter>().ForEach((Entity e, ref Controller controller, ref Targeter targeter) =>
            {
                // if target and new target
                // change material of character to selected
                // change last selected entity to deselected!
                if (controller.gameState != ((byte)GameState.InGame))
                {
                    return;
                }

                // seperate into another system later
                if (controller.mappingType == ((byte)ControllerMapping.Character)) {
                    if (targeter.hasTarget == 1 && controller.Value.buttonA == 1)
                    {
                        // if clicked on target
                        //UnityEngine.Debug.LogError("Started talking with character: " + targeter.nearbyCharacter.character.Index);
                        // start dialogue here!
                        controller.mappingType = 1;
                        DialogueUISpawnSystem.SpawnUI(World.EntityManager, e);
                    }
                }
                else if (controller.mappingType == ((byte)ControllerMapping.Dialogue))
                {
                    if (controller.Value.buttonB == 1)
                    {
                        FinishedSpeaking(e, ref controller);
                    }
                    CameraLink clink = World.EntityManager.GetComponentData<CameraLink>(e);
                    CharacterUIList list = World.EntityManager.GetComponentData<CharacterUIList>(clink.camera);
                    var uis = list.uis.ToArray();
                    //Debug.LogError("UIs: " + uis.Length);
                    for (int i = 0; i < uis.Length; i++)
                    {
                        if (World.EntityManager.HasComponent<DialogueUI>(uis[i]))
                        {
                            //Debug.LogError("UI is dialogue: " + i);
                            if (World.EntityManager.GetComponentData<DialogueUI>(uis[i]).completedTree == 1)
                            {
                                FinishedSpeaking(e, ref controller);
                            }
                            break;
                        }
                        //else {
                        //    Debug.LogError("UI isnt' dialogue: " + i);
                        //}
                    }
                }
            });
        }

        private void FinishedSpeaking(Entity e, ref Controller controller)
        {
            // dialogue stuff
            //Debug.LogError("Finished Dialogue");
            DialogueUISpawnSystem.RemoveUI(EntityManager, e);
            controller.mappingType = 0;
        }
    }
}