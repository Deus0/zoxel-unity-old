using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    [CreateAssetMenu(fileName = "FontData", menuName = "ZoxelUI/FontData")]
    public class FontDatam : ScriptableObject
    {
        public Material material;
        public float generalWidth; // 1
        public List<Texture2D> textures;
        public List<Texture2D> numbers;
        public List<Texture2D> lowercaseLetters;
        public List<Texture2D> uppercaseLetters;
    }
}
