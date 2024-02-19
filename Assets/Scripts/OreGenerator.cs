using System.Collections.Generic;
using UnityEngine;

public class OreGenerator : MonoBehaviour
{
    public int width = 256;
    public int length = 256;
    public int height = 6;
    public float scale = 20f;

    public float oreThreshold = 0.5f;

    // Define your Block prefab or create it dynamically
    public Block blockPrefab;

    void Start()
    {
        List<Block> oreBlocks = GenerateOreMap();
        
        // Now you have a list of Block objects. You can use this list as needed.
        foreach (Block oreBlock in oreBlocks)
        {
            // Do something with each Block object, such as adding it to a collection or processing it further.
            Debug.Log($"Generated Ore Block at {oreBlock.location}");
            Instantiate(oreBlock.prefab, oreBlock.location, Quaternion.identity);
        }
    }

    List<Block> GenerateOreMap()
    {
        List<Block> oreBlocks = new List<Block>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    float xCoord = (float)x / width * scale;
                    float zCoord = (float)z / length * scale;

                    float oreValue = Mathf.PerlinNoise(xCoord, zCoord);
                    float newThreshold = oreThreshold + (y / (float)height);
                    
                    if (oreValue > newThreshold)
                    {
                        Vector3 position = new Vector3(x, y, z);

                        // Create a new Block object with the desired properties
                        Block oreBlock = ScriptableObject.CreateInstance<Block>();
                        oreBlock.CopyOf(blockPrefab);
                        oreBlock.location = position;

                        // Add the Block object to the list
                        oreBlocks.Add(oreBlock);
                    }
                }
            }
        }

        return oreBlocks;
    }
}
