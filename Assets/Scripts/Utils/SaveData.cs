﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Utils
{
    [Serializable]
    public class SaveData
    {
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
            public bool navMeshBuilt;
            public List<SerializableBlockData> blocks;
            
            public SerializableVerticalBlocks(VerticalBlocks verticalBlocks)
            {
                isLoaded = verticalBlocks.isLoaded;
                navMeshBuilt = verticalBlocks.navMeshBuilt;
                blocks = verticalBlocks.blocks.Select(block => new SerializableBlockData((int)block.location.y, block.id, block.isLoaded)).ToList();
                
            }
        }
        


        [Serializable]
        public struct SerializableIntTuple
        {
            public int count;
            public int blockId;
            
            public SerializableIntTuple(int count, int blockId)
            {
                this.count = count;
                this.blockId = blockId;
            }
        }
        
        [Serializable]
        public struct SerializableBlockData
        {
            public int depth;
            public int blockId;
            public bool isLoaded;
            
            public SerializableBlockData(int depth, int blockId,bool isLoaded)
            {
                this.depth = depth;
                this.blockId = blockId;
                this.isLoaded = isLoaded;
            }
        }
        
        public SerializableVector3 playerPosition;
        public SerializableVector3 playerRotation;
        public List<SerializableIntTuple> inventory;
        public List<SerializableEntity> entitiesInScene;
        public List<SerializableKeyValuePair> coordsToHeightList;
        public float time;
        public int nightsSurvived;
        
        


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

        public SaveData(Dictionary<Vector2, VerticalBlocks> dictionary, List<GameObject> entitiesInScene, Vector3 playerPosition, InventoryItem<Entity>[] inventory, 
            float time, int nightsSurvived, Quaternion playerRotation)
        {
            // Converting entities to serializable entities, this will need to be changed as more enemies are added

            List<(string,int)> allEntities = GameManager.Instance.allEntities.Where(entity => entity is Item).ToList().Select(entity => (entity.name, entity.id)).ToList();
            List<(string, int)> allEnemies = new List<(string, int)> { ("Skeleton", 1) }; //TODO Update with actual enemies
            
            var entities = entitiesInScene.Select(entity =>
            {
                if (allEntities.Any(e => e.Item1 == entity.name))
                {
                    return new SerializableEntity
                    {
                        id = allEntities.First(e => e.Item1 == entity.name).Item2,
                        location = new SerializableVector3(entity.transform.position),
                        type = "Item"
                    };
                }
                else if (allEnemies.Any(e => e.Item1 == entity.name))
                {
                    return new SerializableEntity
                    {
                        id = allEnemies.First(e => e.Item1 == entity.name).Item2,
                        location = new SerializableVector3(entity.transform.position),
                        type = "Enemy"
                    };
                }
                else
                {
                    return new SerializableEntity
                    {
                        id = 0,
                        location = new SerializableVector3(entity.transform.position),
                        type = "Unknown"
                    };
                }
            }).ToList();
            
            this.entitiesInScene = entities;
            
            this.playerPosition = new SerializableVector3(playerPosition);

            var inventoryList = inventory.Where(item => item.item != null).ToList();
            
            this.inventory = inventoryList.Select(item => new SerializableIntTuple(item.count, item.item.id)).ToList();
            this.time = time;
            this.nightsSurvived = nightsSurvived;
            this.playerRotation = new SerializableVector3(playerRotation.eulerAngles);
            
            
            
            coordsToHeightList = new List<SerializableKeyValuePair>();
            foreach (var kvp in dictionary)
            {
                coordsToHeightList.Add(new SerializableKeyValuePair(kvp));
            }
        }
        
        public Vector3 GetPlayerPosition()
        {
            return new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        }
        
        public List<InventoryItem<Entity>> GetInventory()
        {
            return inventory.Select(item => new InventoryItem<Entity>(GameManager.Instance.allEntities.First(entity => entity.id == item.blockId), item.count)).ToList();
        }
        
        public List<(string, Vector3, string)> GetEntitiesInScene()
        {
            var enemyNames = new List<(string, int)> { ("Skeleton", 1) }; //TODO Update with actual enemies
            var itemNames = GameManager.Instance.allEntities.Where(entity => entity is Item).ToList().Select(entity => (entity.name, entity.id)).ToList();
            
            return entitiesInScene.Select(entity =>
            {
                if (enemyNames.Any(e => e.Item2 == entity.id))
                {
                    return (enemyNames.First(e => e.Item2 == entity.id).Item1, new Vector3(entity.location.x, entity.location.y, entity.location.z), "Enemy");
                }
                else if (itemNames.Any(e => e.Item2 == entity.id))
                {
                    return (itemNames.First(e => e.Item2 == entity.id).Item1, new Vector3(entity.location.x, entity.location.y, entity.location.z), "Item");
                }
                else
                {
                    return ("Unknown", new Vector3(entity.location.x, entity.location.y, entity.location.z), "Unknown");
                }
            }).ToList();
        }
        
        public Dictionary<Vector2, VerticalBlocks> GetCoordsToHeightList()
        {
            var allBlocks = GameManager.Instance.allEntities.OfType<Block>().ToList();
    
            return coordsToHeightList.ToDictionary(
                pair => new Vector2(pair.key.x, pair.key.y),
                pair => new VerticalBlocks(
                    pair.value.blocks.Select(
                        block =>
                        {
                            var foundBlock = allBlocks.FirstOrDefault(entity => entity.id == block.blockId);
                            if (foundBlock != null)
                            {
                                var newBlock = ScriptableObject.CreateInstance<Block>();
                                newBlock.InstantiateBlock(foundBlock);
                                newBlock.location = new Vector3(pair.key.x, block.depth, pair.key.y);
                                newBlock.isLoaded = block.isLoaded;
                                return newBlock;
                            }
                            else
                            {
                                // Handle the case where the block is not found
                                Debug.LogError($"Block with ID {block.blockId} not found.");
                                return null;
                            }
                        }
                    ).Where(newBlock => newBlock != null).ToList(), 
                    pair.value.isLoaded, 
                    pair.value.navMeshBuilt
                )
            );
            
        }
        
        public float GetTime()
        {
            return time;
        }
        
        public int GetNightsSurvived()
        {
            return nightsSurvived;
        }
        
        public Quaternion GetPlayerRotation()
        {
            return Quaternion.Euler(playerRotation.x, playerRotation.y, playerRotation.z);
        }

    }
}