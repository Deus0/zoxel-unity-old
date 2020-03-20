using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

namespace Zoxel
{
    public class DebugEntityPositions : MonoBehaviour
    {
        public bool isGUI;

        // Update is called once per frame
       /* void Update()
        {
            for (int i = CharacterSpawnSystem.characters.Count - 1; i >= 0; i--)
            {
                Translation position =Unity.Entities.World.Active.EntityManager.GetComponentData<Translation>(CharacterSpawnSystem.characters[i]);
                Debug.DrawLine(position.Value, position.Value + new Unity.Mathematics.float3(0, 1, 0));
            }
        }

        public void OnGUI()
        {
            if (isGUI)
            {
                for (int i = CharacterSpawnSystem.characters.Count - 1; i >= 0; i--)
                {
                    Translation position =Unity.Entities.World.Active.EntityManager.GetComponentData<Translation>(CharacterSpawnSystem.characters[i]);
                    GUILayout.Label(i + "Position: " + position.Value.ToString());
                }
            }
        }*/
    }

}