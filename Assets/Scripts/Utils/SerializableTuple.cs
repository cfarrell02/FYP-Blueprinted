using System;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public struct SerializableTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public SerializableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
    
    [Serializable]
    public struct SerializableOreParameters
    {
        [Tooltip("The block to be used for the ore.")]
        public Block oreBlock;
        [Range(0, 1), Tooltip("The threshold for the ore. Recommended to keep this value between 0 and 1.")]
        public float oreThreshold;
        [Range(0, 1), Tooltip("The scale of the noise used to generate the ore. Recommended to keep this value between 0 and 1.")]
        public float scale;
        [Tooltip("The upper bound for the ore's y position.")]
        public int yUpperBound, yLowerBound;
        
        public SerializableOreParameters(Block oreBlock, float oreThreshold, float scale, int yUpperBound, int yLowerBound)
        {
            this.oreBlock = oreBlock;
            this.oreThreshold = oreThreshold;
            this.scale = scale;
            this.yUpperBound = yUpperBound;
            this.yLowerBound = yLowerBound;
        }
    }

}