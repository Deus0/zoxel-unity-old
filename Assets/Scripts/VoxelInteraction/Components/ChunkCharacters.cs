using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Zoxel.Voxels;

namespace Zoxel.Voxels
{

    /// <summary>
    /// Used to interfact with voxels by character
    /// </summary>
    [System.Serializable]
    public struct ChunkCharacters : IComponentData
    {
        public BlitableArray<int> characters;
    }

}