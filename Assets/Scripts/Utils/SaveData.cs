using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Utils
{
    [Serializable]
    public class SaveData
    {
        public SerializableVector3 playerPosition;
        public SerializableVector3 playerRotation;
        public List<SerialzableIntIntTuple> inventory;
        public List<SerializableEntity> entitiesInScene;
        public List<SerializableKeyValuePair> coordsToHeightList;
        public float time, mapScale;
        public int nightsSurvived, playerXP;    
        
        
        

        public SaveData(Dictionary<Vector2, VerticalBlocks> dictionary, List<GameObject> entitiesInScene, Vector3 playerPosition, InventoryItem<Entity>[] inventory, 
            float time, int nightsSurvived, Quaternion playerRotation, float mapScale, int XP)
        {
            // Converting entities to serializable entities, this will need to be changed as more enemies are added

            List<(string,int)> allEntities = GameManager.Instance.allEntities.Where(entity => entity is Item).ToList().Select(entity => (entity.name, entity.id)).ToList();
            List<(string, int)> allEnemies = new List<(string, int)> { ("Skeleton", 1) }; //TODO Update with actual enemies
            playerXP = XP;
            
            
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
            
            this.inventory = inventoryList.Select(item => new SerialzableIntIntTuple(item.count, item.item.id)).ToList();
            this.time = time;
            this.nightsSurvived = nightsSurvived;
            this.playerRotation = new SerializableVector3(playerRotation.eulerAngles);
            this.mapScale = mapScale;
            
            
            
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
        
        public int GetPlayerXP()
        {
            return playerXP;
        }
        
        public List<InventoryItem<Entity>> GetInventory()
        {
            return inventory.Select(item => new InventoryItem<Entity>(
                    GameManager.Instance.allEntities.First(entity => entity.id == item.Item2),
                    item.Item1))
                .ToList();
        }
        
        public float GetMapScale()
        {
            return mapScale;
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
                                newBlock.CopyOf(foundBlock);
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
                    pair.value.isLoaded
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