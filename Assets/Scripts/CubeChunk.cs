using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class CubeChunk : MonoBehaviour
{
    private List<Block> blocks = new List<Block>();
    private int chunkSize = 20;



    public void BuildChunkNavMesh()
    {
        //NavMeshSurface navMeshSurface = GetComponent<NavMeshSurface>();

        //navMeshSurface.BuildNavMesh(); // Rebuild the nav mesh for this surface
    }

    public void AddBlock(Block block)
    {
        blocks.Add(block);
    }

    public void RemoveBlock(Block block)
    {
        blocks.Remove(block);
    }

    public List<Block> GetBlocks()
    {
        return blocks;
    }

    public Vector2 GetChunkCenter()
    {
        return new Vector2(transform.position.x + chunkSize / 2, transform.position.z + chunkSize / 2);
    }
}
