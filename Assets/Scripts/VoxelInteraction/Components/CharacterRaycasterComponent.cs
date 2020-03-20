using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Zoxel.Voxels;

namespace Zoxel
{
    public class CharacterRaycasterComponent : ComponentDataProxy<CharacterRaycaster> { }

    /// <summary>
    /// Used to interfact with voxels by character
    /// </summary>
    [System.Serializable]
    public struct CharacterRaycaster : IComponentData
    {
        public Entity camera;
        public int commandID;
        public float3 voxelPosition;
        public byte triggered;
        public float lastCasted;

        public byte DidCast()
        {
            if (voxelPosition.y == VoxelRaycastSystem.failedPosition.y)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }

}