using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class BlockyTerrain : MonoBehaviour
{
    //public int gridSizeX = 40;
    //public int gridSizeZ = 40;
    public float scale = 1f;
    public float cubeHeight = 1f; // Set a fixed cube height
    public GameObject cubePrefab;
    public Transform playerTransform; // Reference to the player's transform

    int chunkSize = 40;
    int previousPlayerPosX;
    int previousPlayerPosZ;
    int loadDistance = 20; // Distance around the player to load new terrain
    int loadDistanceMultiplier = 2; // Multiplier for the load distance when generating terrain

    private Dictionary<Vector2, List<Block>> coordsToHeight = new Dictionary<Vector2, List<Block>>();
    private string currentChunk = "";

    void Start()
    {
        previousPlayerPosX = (int)playerTransform.position.x;
        previousPlayerPosZ = (int)playerTransform.position.z;
        GenerateInitialTerrain();
    }

    void Update()
    {
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;

        //print(coordsToHeight.Keys.Count);
        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= loadDistance / 2 ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= loadDistance / 2)
        {
            previousPlayerPosX = currentPlayerPosX;
            previousPlayerPosZ = currentPlayerPosZ;
            GenerateTerrain();
        }
        UnloadTerrain();

         currentChunk = "(" + Mathf.Floor(playerTransform.position.x / chunkSize) + ".00, " + (float)Mathf.Floor(playerTransform.position.z / chunkSize) + ".00)";
        //// Find the chunk the player is in
        //GameObject chunk = GameObject.Find(currentChunk);
        //if (chunk != null)
        //{
        //    // Find the NavMeshSurface component and build the NavMesh
        //    NavMeshSurface surface = chunk.GetComponent<NavMeshSurface>();
        //    surface.BuildNavMesh();
        //    this.currentChunk = currentChunk;
        //}

    }

    void GenerateInitialTerrain()
    {
        int extendedLoadDistance = loadDistance * loadDistanceMultiplier;
        for (int x = previousPlayerPosX - extendedLoadDistance; x < previousPlayerPosX + extendedLoadDistance; x++)
        {
            for (int z = previousPlayerPosZ - extendedLoadDistance; z < previousPlayerPosZ + extendedLoadDistance; z++)
            {
                GenerateCubeAtPosition(x, z);
            }
        }
    }

    void GenerateTerrain()
    {
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;

        int extendedLoadDistance = loadDistance * loadDistanceMultiplier;

        for (int x = previousPlayerPosX - extendedLoadDistance; x < previousPlayerPosX + extendedLoadDistance; x++)
        {
            if (Mathf.Abs(x - currentPlayerPosX) >= extendedLoadDistance)
            {
                // Do not regenerate cubes within the visible area
                continue;
            }

            for (int z = previousPlayerPosZ - extendedLoadDistance; z < previousPlayerPosZ + extendedLoadDistance; z++)
            {
                if (Mathf.Abs(z - currentPlayerPosZ) >= extendedLoadDistance)
                {
                    // Do not regenerate cubes within the visible area
                    continue;
                }

                GenerateCubeAtPosition(x, z);
            }
        }

        previousPlayerPosX = currentPlayerPosX;
        previousPlayerPosZ = currentPlayerPosZ;

        GameObject[] chunks = GameObject.FindGameObjectsWithTag("Chunk"); // Chunk must be tagged as "Chunk"
        foreach(GameObject chunk in chunks)
        {
            NavMeshSurface surface = chunk.GetComponent<NavMeshSurface>();
            surface.BuildNavMesh();
        }

    }


    void GenerateCubeAtPosition(int x, int z)
    {
        Vector2 currentPos = new Vector2(x, z);
        Vector2 chunkPOS = new Vector2(x / chunkSize, z / chunkSize);
        var chunk = FindOrCreateChunk(chunkPOS);

        if (!coordsToHeight.ContainsKey(currentPos))
        {
            float y = Mathf.PerlinNoise(x * 0.1f * scale, z * 0.1f * scale) * 3f;
            y = Mathf.Floor(y / cubeHeight) * cubeHeight;
            List<Block> verticalBlocks = new List<Block>();
            for (int i = -5; i <= y; ++i)
            {
                Vector3 cubePos = new Vector3(x, i, z);

                if (i == y)
                {
                    GameObject cube = Instantiate(cubePrefab, cubePos, Quaternion.identity);
                    cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
                    cube.transform.parent = chunk.transform;
                }

                var blockItem = new Block("Cube", 1, 100, 100, 1, 64, cubePos, Vector3.zero, Vector3.one);
                blockItem.isLoaded = i == y;
                verticalBlocks.Add(blockItem);
            }


            coordsToHeight.Add(currentPos, verticalBlocks);
        }
        else
        {
            var blockItem = coordsToHeight[currentPos];

            for (int i = 0; i < blockItem.Count; ++i)
            {
                Block block = blockItem[i];
                if (block.isLoaded)
                {
                    GameObject cube = Instantiate(cubePrefab, block.Location, Quaternion.identity);
                    cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
                    cube.transform.parent = chunk.transform;
                }
            }
        }
    }

    private GameObject FindOrCreateChunk(Vector2 pos)
    {
        GameObject chunk = GameObject.Find(pos.ToString());
        if (chunk == null)
        {
            chunk = new GameObject(pos.ToString());
            chunk.tag = "Chunk"; // Necessary for unloading

            chunk.AddComponent<NavMeshSurface>();

            chunk.transform.parent = transform;
        }


        return chunk;
    }



    void UnloadTerrain()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube"); // Cube prefab must be tagged as "Cube"

        GameObject[] chunks = GameObject.FindGameObjectsWithTag("Chunk"); // Chunk must be tagged as "Chunk"
        foreach (var c in chunks)
        {
            if (c.transform.childCount == 0)
            {
                Destroy(c);
            }
        }

        foreach (GameObject cube in cubes)
        {
            Vector3 pos = cube.transform.position;
            int extendedLoadDistance = loadDistance * loadDistanceMultiplier;
            // Remove cubes outside the visible area from the scene
            if (Mathf.Abs(pos.x - playerTransform.position.x) >= extendedLoadDistance ||
                Mathf.Abs(pos.z - playerTransform.position.z) >= extendedLoadDistance)
            {
                Block block = FindBlock(pos);
                //block.isLoaded = false;
                //place block in the dictionary
                Vector2 pos2D = new Vector2(pos.x, pos.z);
                var blockList = coordsToHeight[pos2D];
                for (int i = 0;i < blockList.Count; ++i)
                {
                    if (blockList[i].Location == pos)
                    {
                        blockList[i] = block;
                        break;
                    }
                }
                Destroy(cube); // Remove cube from the scene
            }
        }
    }
    public Dictionary<Vector2, List<Block>> getHeightMap()
    {
        return coordsToHeight;
    }

    public bool RemoveBlock(Vector3 position)
    {
        Vector2 pos = new Vector2(position.x, position.z);
        if (coordsToHeight.ContainsKey(pos))
        {
            var blockList = coordsToHeight[pos];
            if (blockList.Count > 0)
            {
                Block block = FindBlock(position);
                if (block.Name != null)
                {
                    blockList.Remove(block);
                    return true;
                }
            }
        }
        return false;
    }

    public bool AddBlock(Vector3 position, Block block)
    {
        Vector2 pos = new Vector2(position.x, position.z);
        Vector2 chunkPOS = new Vector2(position.x / chunkSize, position.z / chunkSize);
        var chunk = FindOrCreateChunk(chunkPOS);
        if (coordsToHeight.ContainsKey(pos))
        {
            var blockList = coordsToHeight[pos];
            if (blockList.Count > 0)
            {
                block.isLoaded = true;
                for (int i = 0; i < blockList.Count; ++i)
                {
                    if (blockList[i].Location == position)
                    {
                        blockList[i] = block;
                        break;
                    }
                }
                if (!blockList.Contains(block))
                {
                    blockList.Add(block);
                }
                var cube = Instantiate(cubePrefab, position, Quaternion.identity);
                cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
                cube.transform.parent = chunk.transform;
                return true;
            }
        }
        return false;
    }   


    public Block FindBlock(Vector3 position)
    {
        Vector2 pos = new Vector2(position.x, position.z);
        if (coordsToHeight.ContainsKey(pos))
        {
            var blockList = coordsToHeight[pos];
            if (blockList.Count > 0)
            {
                Block block = new Block(); // Empty Search block
                foreach (Block b in blockList)
                {
                    if (b.Location == position)
                    {
                        block = b;
                        break;
                    }
                }
                if (block.Name != null)
                {
                    return block;
                }
            }
        }
        return new Block();
    }   
}