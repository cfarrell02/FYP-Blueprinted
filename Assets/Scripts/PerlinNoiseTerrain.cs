using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;


public class BlockyTerrain : MonoBehaviour
{
    public int depth = 10;
    public float scale = 1f;
    public float cubeHeight = 1f; // Set a fixed cube height
    public float perlinNoiseHeight = 3f; // Set a fixed cube height
    public GameObject cubePrefab;
    public Transform playerTransform; // Reference to the player's transform
    

    int previousPlayerPosX;
    int previousPlayerPosZ;
    [SerializeField]
    int loadDistance = 40; // Distance around the player to load new terrain
    [SerializeField]
    int navMeshDistance = 20; // Distance around the player to load new terrain
    [SerializeField]
    int newTerrainDistance = 2; // Multiplier for the load distance when generating terrain
    [SerializeField]
    int newNavMeshDistance = 10; // Multiplier for the load distance when generating terrain
    

    private Dictionary<Vector2, VerticalBlocks> coordsToHeight = new Dictionary<Vector2, VerticalBlocks>();
    private NavMeshSurface surface;

    void Start()
    {
        //Get surface in child
        surface = GetComponentInChildren<NavMeshSurface>();

        previousPlayerPosX = (int)playerTransform.position.x;
        previousPlayerPosZ = (int)playerTransform.position.z;
        GenerateInitialTerrain();
    }

    void Update()
    {
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;
        HandleNavmesh();
        //print(coordsToHeight.Keys.Count);
        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= newTerrainDistance ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= newTerrainDistance)
        {
            //previousPlayerPosX = currentPlayerPosX;
            //previousPlayerPosZ = currentPlayerPosZ;
            GenerateTerrain();
        }
        UnloadTerrain();
    }

    void HandleNavmesh()
    {
        // // Check if player is near enemies 
        // var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // bool enemiesNearby = false;
        // foreach (var enemy in enemies)
        // {
        //     if (Vector3.Distance(enemy.transform.position, playerTransform.position) < navMeshDistance)
        //     {
        //         enemiesNearby = true;
        //         break;
        //     }
        // }
        // if (enemiesNearby)
        // {
        //     return;
        // }
        //
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;
        
        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= newNavMeshDistance ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= newNavMeshDistance)
        {
            print("Building navmesh");
            //Ensure only blocks that are within the navmesh distance are parented to the navmesh
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
                Vector3 pos = cube.transform.position;
                print (pos);
                // Remove cubes outside the visible area from the scene
                if (Mathf.Abs(pos.x - playerTransform.position.x) >= navMeshDistance ||
                    Mathf.Abs(pos.z - playerTransform.position.z) >= navMeshDistance)
                {
                    cube.transform.parent = transform;
                }
                else
                {
                    cube.transform.parent = transform.GetChild(0); //Assuming the navmesh is the first child
                }
            }
            BuildNavmesh();
            
            previousPlayerPosX = currentPlayerPosX;
            previousPlayerPosZ = currentPlayerPosZ;
        }
        
    }

    void GenerateInitialTerrain()
    {
        for (int x = previousPlayerPosX - loadDistance; x < previousPlayerPosX + loadDistance; x++)
        {
            for (int z = previousPlayerPosZ - loadDistance; z < previousPlayerPosZ + loadDistance; z++)
            {
                GenerateCubeAtPosition(x, z);
            }
        }
        surface.BuildNavMesh();
    }
    void GenerateTerrain()
    {
        var position = playerTransform.position;
        int currentPlayerPosX = (int)position.x;
        int currentPlayerPosZ = (int)position.z;


        for (int x = currentPlayerPosX - loadDistance; x < currentPlayerPosX + loadDistance; x++)
        {


            for (int z = currentPlayerPosZ - loadDistance; z < currentPlayerPosZ + loadDistance; z++)
            {
               
                if (coordsToHeight.ContainsKey(new Vector2(x, z)) && coordsToHeight[new Vector2(x, z)].isLoaded)
                {
                    continue;
                }
                GenerateCubeAtPosition(x, z);
            }
        }
       
    }

    private void BuildNavmesh()
    { 
        // Would love to do this async but it doesn't work
        // {
        //     NavMeshData navMeshData = surface.navMeshData;
        //     NavMeshBuildSettings buildSettings = surface.GetBuildSettings();
        //     List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        //     Bounds sourceBounds = navMeshData.sourceBounds;
        //     NavMeshBuilder.CollectSources(sourceBounds, LayerMask.GetMask("NavMesh"), NavMeshCollectGeometry.RenderMeshes, 0, new List<NavMeshBuildMarkup>(), sources);
        //
        //     NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, buildSettings, sources, sourceBounds);
        // }
        
        surface.BuildNavMesh(); // Does not scale well

        // // For all loaded blocks, set navMeshBuilt to true
        // foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
        // {
        //     Vector3 pos = cube.transform.position;
        //     Vector2 pos2D = new Vector2(pos.x, pos.z);
        //     if (coordsToHeight.ContainsKey(pos2D))
        //     {
        //         var selectedBlocks = coordsToHeight[pos2D];
        //         selectedBlocks.navMeshBuilt = true;
        //         coordsToHeight[pos2D] = selectedBlocks;
        //     }
        // }
        
    }


    void GenerateCubeAtPosition(int x, int z)
    {
        Vector2 currentPos = new Vector2(x, z);

        if (coordsToHeight.ContainsKey(currentPos))
        {
            var blockItem = coordsToHeight[currentPos].blocks;

            foreach (var block in blockItem)
            {
                if (block.isLoaded)
                {
                    InstantiateCube(block.Location);
                }
            }
        }
        else
        {
            float y = Mathf.PerlinNoise(x * 0.1f * scale, z * 0.1f * scale) * perlinNoiseHeight;
            y = Mathf.Floor(y / cubeHeight) * cubeHeight;

            List<Block> verticalBlocks = new List<Block>();

            for (int i = -depth; i <= y; ++i)
            {
                Vector3 cubePos = new Vector3(x, i, z);

                bool toBeLoaded = i == y;
                
                var cube = new Block("Cube", 1, 100, 100, 1, 64, cubePos, Vector3.zero, Vector3.one, cubePrefab);

                // This is too laggy
                // if (!toBeLoaded)
                // { 
                //     // Checking not top layer blocks to see if they need to be loaded
                //     var surroundingBlocks = GetSurroundingBlocks(cube);
                //     toBeLoaded = surroundingBlocks.Any(surroundingBlock => FindBlock(surroundingBlock).Name == null);
                // }

                if (toBeLoaded)
                {
                    InstantiateCube(cubePos);
                }
                cube.isLoaded = toBeLoaded;

                verticalBlocks.Add(cube);
            }

            coordsToHeight.Add(currentPos, new VerticalBlocks { blocks = verticalBlocks, isLoaded = true, navMeshBuilt = false });
        }
    }

    void InstantiateCube(Vector3 position)
    {
        GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
        cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
        cube.tag = "Cube";
        cube.name = "Cube: " + position;
        cube.isStatic = true;
        float distanceToPlayer = Vector3.Distance(position, playerTransform.position);
        if (distanceToPlayer < navMeshDistance)
        {
            cube.transform.parent = transform.GetChild(0); //Assuming the navmesh is the first child
        }
        else
        {
            cube.transform.parent = transform;
        }

    }



    void UnloadTerrain()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube"); // Cube prefab must be tagged as "Cube"

        foreach (GameObject cube in cubes)
        {
            Vector3 pos = cube.transform.position;
            // Remove cubes outside the visible area from the scene
            if (Mathf.Abs(pos.x - playerTransform.position.x) >= loadDistance ||
                Mathf.Abs(pos.z - playerTransform.position.z) >= loadDistance)
            {
                Block block = FindBlock(pos);
                //block.isLoaded = false;
                //place block in the dictionary
                Vector2 pos2D = new Vector2(pos.x, pos.z);
                var blockList = coordsToHeight.ContainsKey(pos2D) ? coordsToHeight[pos2D].blocks: new List<Block>();
                if (blockList.Count == 0)
                {
                    continue;
                }
                for (int i = 0; i < blockList.Count; ++i)
                {
                    if (blockList[i].Location == pos)
                    {
                        blockList[i] = block;
                        break;
                    }
                }
                var a = coordsToHeight[pos2D];  
                a.isLoaded = false;
                coordsToHeight[pos2D] = a;
                Destroy(cube); // Remove cube from the scene
            }
        }
    }
    public Dictionary<Vector2, VerticalBlocks> getHeightMap()
    {
        return coordsToHeight;
    }

    public bool RemoveBlock(Vector3 position)
    {
        Vector2 pos = new Vector2(position.x, position.z);
        if (coordsToHeight.ContainsKey(pos))
        {
            var blockList = coordsToHeight[pos].blocks;
            if (blockList.Count > 0)
            {
                Block block = FindBlock(position);
                if (block.Name != null)
                {
                    blockList.Remove(block);

                    var cubeToRemove = GameObject.Find("Cube: " + position);
                    Destroy(cubeToRemove);

                    var surroundingBlocks = GetSurroundingBlocks(block);


                    foreach (Vector3 surroundingBlock in surroundingBlocks)
                    {
                        var surroundingBlockItem = FindBlock(surroundingBlock);
                        if (surroundingBlockItem.Name != null)
                        {
                            var foundBlock = FindBlock(surroundingBlock);
                            if (foundBlock.Name != null && !foundBlock.isLoaded)
                            {
                                AddBlock(surroundingBlock, surroundingBlockItem);
                            }
                        }
                    }
                    BuildNavmesh();
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3[] GetSurroundingBlocks(Block block)
    {
        Vector3[] surroundingBlocks = new Vector3[6];
        surroundingBlocks[0] = new Vector3(block.Location.x + cubeHeight, block.Location.y, block.Location.z);
        surroundingBlocks[1] = new Vector3(block.Location.x - cubeHeight, block.Location.y, block.Location.z);
        surroundingBlocks[2] = new Vector3(block.Location.x, block.Location.y + cubeHeight, block.Location.z);
        surroundingBlocks[3] = new Vector3(block.Location.x, block.Location.y - cubeHeight, block.Location.z);
        surroundingBlocks[4] = new Vector3(block.Location.x, block.Location.y, block.Location.z + cubeHeight);
        surroundingBlocks[5] = new Vector3(block.Location.x, block.Location.y, block.Location.z - cubeHeight);
        return surroundingBlocks;
    }


    public bool AddBlock(Vector3 position, Block block)
    {
        Vector2 pos = new Vector2(position.x, position.z);
        if (coordsToHeight.ContainsKey(pos))
        {
            var blockList = coordsToHeight.ContainsKey(pos) ? coordsToHeight[pos].blocks : new List<Block>();
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
                InstantiateCube(position);
                var a = coordsToHeight[pos];
                a.isLoaded = true;
                coordsToHeight[pos] = a;

                
                // Build the new block
                BuildNavmesh();                
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
            var blockList = coordsToHeight.ContainsKey(pos) ? coordsToHeight[pos].blocks : new List<Block>();
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

public struct VerticalBlocks
{
    public List<Block> blocks { get; set; }
    public bool isLoaded { get; set; }
    public bool navMeshBuilt { get; set; }

    public VerticalBlocks(List<Block> blocks, bool isLoaded, bool navMeshBuilt)
    {
        this.blocks = blocks;
        this.isLoaded = isLoaded;
        this.navMeshBuilt = navMeshBuilt;
    }
}
