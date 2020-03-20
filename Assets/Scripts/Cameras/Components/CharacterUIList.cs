using Unity.Entities;

namespace Zoxel {
    
    public class CharacterUIListComponent : ComponentDataProxy<CharacterUIList> { }
    
    
    [System.Serializable]
    public struct CharacterUIList : IComponentData
    {
        public BlitableArray<Entity> uis;
    }
}