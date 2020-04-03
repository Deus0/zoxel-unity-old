using System;

namespace Zoxel
{
    // starting point for a slot to position to for a vox
    // assume that it goes onto the other items bottom if its top slot
    [Serializable]
    public enum SlotAxis
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back,
        Center
    }
}