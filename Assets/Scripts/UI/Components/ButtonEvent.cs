using Unity.Entities;

namespace Zoxel.UI
{
    public struct ButtonClickEvent : IComponentData
    {
        public Entity character;
        public byte buttonType;          // which button, A, X, B etc
    }
    public struct ButtonSelectEvent : IComponentData
    {
        public Entity character;
    }
}
