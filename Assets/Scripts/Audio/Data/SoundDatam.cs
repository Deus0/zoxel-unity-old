using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    /// <summary>
    /// Bone data
    /// Exported from a maya import with bones
    /// Used to spawn bone entities and to animate in game
    /// </summary>
    [CreateAssetMenu(fileName = "Sound", menuName = "ZoxelArt/Sound")]//, order = 7)]
    public class SoundDatam : ScriptableObject//or monobehaviour
    {
        // bullet data
        // for other stuff?
        public float volume;
        public AudioClip clip;  // generated clip

        [ContextMenu("Generate Noise")]
        public void GenerateNoise()
        {

        }

        [ContextMenu("Generate Noise")]
        public void GenerateSin()
        {

        }
    }
}