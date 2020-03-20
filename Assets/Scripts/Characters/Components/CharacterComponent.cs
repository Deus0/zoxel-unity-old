using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    // used just to tell a character apart from a turret!
    // can give the characters more data later depending on their type
    public class CharacterComponent : ComponentDataProxy<Character> { }

    /// <summary>
    /// Tags a character
    /// Don't really need data
    /// </summary>
    [System.Serializable]
    public struct Character : IComponentData
	{
        public int metaID;
        // who fired it ID!
        // public int id;
        // public byte thing;
        // public byte disabled;
    }

}