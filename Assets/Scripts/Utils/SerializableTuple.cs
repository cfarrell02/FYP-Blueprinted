using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{

    
    [Serializable]
    public struct SerialzableIntIntTuple
    {
        public int Item1;
        public int Item2;

        public SerialzableIntIntTuple(int item1, int item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
    
    [Serializable]
    public struct SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        public SerializableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }



    [Serializable]
    public struct SerializableVerticalBlocks
    {
        public bool isLoaded;
        public List<SerializableBlockData> blocks;

        public SerializableVerticalBlocks(VerticalBlocks verticalBlocks)
        {
            isLoaded = verticalBlocks.isLoaded;
            blocks = verticalBlocks.blocks
                .Select(block => new SerializableBlockData((int)block.location.y, block.id, block.isLoaded)).ToList();

        }
    }



    [Serializable]
    public struct SerializableBlockData
    {
        public int depth;
        public int blockId;
        public bool isLoaded;

        public SerializableBlockData(int depth, int blockId, bool isLoaded)
        {
            this.depth = depth;
            this.blockId = blockId;
            this.isLoaded = isLoaded;
        }
    }


    




        [Serializable]
        public struct SerializableKeyValuePair
        {
            public SerializableVector2 key;
            public SerializableVerticalBlocks value;

            public SerializableKeyValuePair(KeyValuePair<Vector2, VerticalBlocks> pair)
            {
                key = new SerializableVector2(pair.Key);
                value = new SerializableVerticalBlocks(pair.Value); 
            }
        }
        
        [Serializable]
        public struct SerializableEntity
        {
            public int id;
            public SerializableVector3 location;
            public string type;

            public SerializableEntity(int id, Vector3 location, string type)
            {
                this.id = id;
                this.location = new SerializableVector3(location);
                this.type = type;
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