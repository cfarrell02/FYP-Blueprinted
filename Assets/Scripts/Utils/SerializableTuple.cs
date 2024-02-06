using System;

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
        public Block oreBlock;
        public float oreThreshold;
        public float scale;
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