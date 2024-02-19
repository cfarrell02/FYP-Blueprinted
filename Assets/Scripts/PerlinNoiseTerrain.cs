using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using static Utils.Utils;
using Utils;
using Random = UnityEngine.Random;


public class BlockyTerrain : MonoBehaviour
{

    public int depth = 10;
    public float scale = 1f;
    public float cubeHeight = 1f; // Set a fixed cube height

    public GameObject enemyPrefab; // These prefabs, will be changes to list or dictionary for different types of enemies

    public Block grass, dirt, stone;

    [Tooltip("Any ores to be generated in the world, with the chance of them spawning")]
    public List<SerializableOreParameters> ore = new List<SerializableOreParameters>();

    int previousPlayerPosX, previousPlayerPosZ;
    int previousPlayerPosXnav, previousPlayerPosZnav;
    
    [SerializeField] int loadDistance = 40; // Distance around the player to load new terrain
    [SerializeField] int navMeshDistance = 20; // Distance around the player to load new terrain
    [SerializeField] int newTerrainDistance = 10; // Multiplier for the load distance when generating terrain
    [SerializeField] int newNavMeshDistance = 10; // Multiplier for the load distance when generating terrain
    [SerializeField] int perlinScale = 10; // Multiplier for the perlin noise scale
    [SerializeField] int stoneThreshold = 4; // Threshold for stone generation
    [SerializeField] int stoneThresholdRange = 2; // Range for the stone threshold




    private Transform playerTransform;
    private Dictionary<Vector2, VerticalBlocks> coordsToHeight = new Dictionary<Vector2, VerticalBlocks>();
    private NavMeshSurface surface;
    private float timer = 0f;
    private LightingManager lightingManager;
    private List<Block> emptyBlocks = new List<Block>(), lightingBlocks = new List<Block>();
    private FogManager fogManager;

    private void Awake()
    {
        var allCubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (var cube in allCubes)
        {
            DestroyWithChildren(cube.gameObject);
        }
        
    }

    void Start()
    {
        //Get surface in child
        surface = GetComponentInChildren<NavMeshSurface>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        previousPlayerPosX = (int)playerTransform.position.x;
        previousPlayerPosZ = (int)playerTransform.position.z;
        previousPlayerPosXnav = (int)playerTransform.position.x;
        previousPlayerPosZnav = (int)playerTransform.position.z;
        lightingManager = GameObject.Find("LightingManager").GetComponent<LightingManager>();
        scale = Random.Range(scale/2, scale + scale/2);
        fogManager = FindObjectOfType<FogManager>();

        ore.ForEach(ore =>
        {
            ore.scale = Random.Range(ore.scale / 2, ore.scale + ore.scale / 2);
        }
        );
        
    }



    void Update()
    {
        
        FillInCaves();
        
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;
        HandleNavmesh();
        //print(coordsToHeight.Keys.Count);
        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= newTerrainDistance ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= newTerrainDistance)
        {
            previousPlayerPosX = currentPlayerPosX;
            previousPlayerPosZ = currentPlayerPosZ;
            GenerateTerrain();
            UnloadTerrain();
        
        }
        
        if (lightingManager && lightingManager.isNight())
        {
            HandleEnemySpawn();
           // ResizeTerrain(newTerrainDistance);
        }
        
        DetectLightSources();
        
        
        //Check if player has fallen off the map
        if (playerTransform.position.y < -depth)
        {
            playerTransform.position = new Vector3(playerTransform.position.x, 10, playerTransform.position.z);
        }


    }

    void DetectLightSources()
    {
        lightingBlocks.ForEach(block =>
        {
            if (block.blockType == Block.BlockType.Light)
            {
                float distance = Vector3.Distance(block.location, playerTransform.position);
                if (distance < 10)
                {
                    fogManager.SetIntensityMultiplier(0.5f);
                }
                else
                {
                    fogManager.SetIntensityMultiplier(1f);
                }
            }
        });
    }


    void HandleNavmesh()
    {
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;

        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosXnav) >= newNavMeshDistance ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZnav) >= newNavMeshDistance)
        {
            previousPlayerPosXnav = currentPlayerPosX;
            previousPlayerPosZnav = currentPlayerPosZ;  
            
            //print("Building navmesh");
            //Ensure only blocks that are within the navmesh distance are parented to the navmesh
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
                //Dont touch held items or items that arent directly children of the terrain/navmesh
                if(cube.layer == 7 || !(cube.transform.parent.name.Contains("Navmesh") || cube.transform.parent.name.Contains("Generator")))
                {
                    continue;
                }
                
                Vector3 pos = cube.transform.position;
                // print (pos);
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


        }
    }

    void HandleEnemySpawn()
    {
        timer += Time.deltaTime;

        if (timer >= 5f && GameObject.FindGameObjectsWithTag("Enemy").Length < 10)
        {
            timer = 0f;

            var noSpawnBlocks = GameObject.FindGameObjectsWithTag("NoSpawn");

            float distanceToSpawn = 10f;
            for (int i = 0; i < 10; i++) // Attempt to find a block to spawn the enemy on 10 times, give up after that
            {
                //Pick a block to spawn the enemy on
                Vector3 spawnPos = new Vector3(
                    Random.Range(playerTransform.position.x - distanceToSpawn,
                        playerTransform.position.x + distanceToSpawn), 0,
                    Random.Range(playerTransform.position.z - distanceToSpawn,
                        playerTransform.position.z + distanceToSpawn));
                //Check if the block is loaded
                Vector2 pos = new Vector2(Mathf.Floor(spawnPos.x), Mathf.Floor(spawnPos.z));
                if (coordsToHeight.ContainsKey(pos))
                {
                    var blockList = coordsToHeight[pos].blocks;
                    if (blockList.Count > 0)
                    {
                        Block block = blockList.ElementAt(blockList.Count - 1);
                        if (block.name != null)
                        {
                            //Spawn the enemy
                            foreach (GameObject noSpawnBlock in noSpawnBlocks)
                            {
                                if (Vector3.Distance(noSpawnBlock.transform.position, spawnPos) <
                                    5f) //TODO, Use the value of the block instead of 5f   
                                {
                                    return;
                                }

                            }
                            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                            break;
                        }
                    }
                }
            }
        }

    }

    internal void GenerateInitialTerrain()
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

    private async void BuildNavmesh()
    {
        await BuildNavmeshAsync(); // This works well... Not sure if this is the best way to do this
    }

    private async Task BuildNavmeshAsync()
    {
        if (surface.navMeshData == null)
        {
            surface.BuildNavMesh();
        }

        AsyncOperation operation = surface.UpdateNavMesh(surface.navMeshData);
        while (!operation.isDone)
        {
            await Task.Yield();
        }
    }



    void GenerateCubeAtPosition(int x, int z)
    {
        Vector2 currentPos = new Vector2(x, z);

        if (coordsToHeight.ContainsKey(currentPos)
            && !coordsToHeight[currentPos].isLoaded)
        {
            var verticalBlocks = coordsToHeight[currentPos];
            var blockItem = verticalBlocks.blocks;
            verticalBlocks.isLoaded = true;


            foreach (var block in blockItem)
            {
                if (block.isLoaded)
                {
                    InstantiateCube(block.location, block);
                }
            }

            coordsToHeight[currentPos] = verticalBlocks;

        }
        else
        {
            List<Block> verticalBlocks = new List<Block>();

            List<Block> oreVerticalBlocks = ore
                .SelectMany(oreBlock =>
                {
                    float yUpperBound = oreBlock.yUpperBound, yLowerBound = oreBlock.yLowerBound;
                    int yDiff = (int)Mathf.Abs(yUpperBound - yLowerBound);
                    var list = GenerateOreList(x, z, yDiff, oreBlock.oreThreshold, oreBlock.oreBlock, oreBlock.scale);
                    list.ForEach(block =>
                    {
                        var actualY = block.location.y + yLowerBound;
                        block.location = new Vector3(block.location.x, actualY, block.location.z);
                    });

                    return list;
                })
                .DistinctBy(block => block.location)
                .ToList();

            float y = Mathf.PerlinNoise(x * 0.1f * scale, z * 0.1f * scale) * perlinScale;
            y = Mathf.Floor(y / cubeHeight) * cubeHeight;
            

            for (int i = -depth; i <= y; ++i)
            {
                Vector3 cubePos = new Vector3(x, i, z);
                bool toBeLoaded = i >= y;
                

                Block copyOfCubeObject = ScriptableObject.CreateInstance<Block>();
                var oreBlock = oreVerticalBlocks.FirstOrDefault(block => block.location == cubePos);
                
                if(oreBlock && oreBlock.blockType == Block.BlockType.Empty){ 
                    emptyBlocks.Add(oreBlock);
                    continue; // Don't generate empty blocks
                }
                
                // Can be laggy
                // //Add in air block above the terrain
                // if (toBeLoaded)
                // {
                //     for (int j = (int)y + 1; j < depth; j++)
                //     {
                //         Block airBlock = ScriptableObject.CreateInstance<Block>();
                //         airBlock.InstantiateBlock(grass); //
                //         airBlock.location = new Vector3(x, i + 1, z);
                //         airBlock.isLoaded = true;
                //         emptyBlocks.Add(airBlock);
                //     }
                // }

                
                

                Block topBlock = (oreBlock ? oreBlock : grass);
                Block block;
                if (i > stoneThreshold + stoneThresholdRange)
                {
                    // Dirt above stone
                    block = dirt;
                }
                else if (i < stoneThreshold - stoneThresholdRange)
                {
                    // Stone below stone threshold
                    block = stone;
                }
                else
                {
                    // Mix of stone and dirt around the stone threshold
                    block = Random.Range(0, 100) < 50 ? stone : dirt;
                }
                
                block = oreBlock ? oreBlock : block;

                if (toBeLoaded)
                {
                    InstantiateCube(cubePos, topBlock);
                    copyOfCubeObject.InstantiateBlock(topBlock);
                }
                else
                {
                    copyOfCubeObject.InstantiateBlock(block);
                }

                copyOfCubeObject.location = cubePos;
                copyOfCubeObject.isLoaded = toBeLoaded;

                verticalBlocks.Add(copyOfCubeObject);
            }

            coordsToHeight.Add(currentPos, new VerticalBlocks { blocks = verticalBlocks, isLoaded = true });
        }
        
        
    }
    
    void FillInCaves()
    {
        
    foreach (var emptyBlock in emptyBlocks)
    {
       // InstantiateCube(emptyBlock.location, safetyBlock);
        var surroundingBlocks = GetSurroundingBlocks(emptyBlock.location);
        for (int i = 0; i < surroundingBlocks.Length; i++)
        {
            var block = FindBlock(surroundingBlocks[i]);
            if (block != null && !block.isLoaded)
            {
                InstantiateCube(block.location, block);
                block.isLoaded = true;
                coordsToHeight[new Vector2(block.location.x, block.location.z)].blocks.Add(block);
            }
        }
    }

    emptyBlocks.Clear();

    }
    
    
    public void InstantiateCube(Vector3 position, Block cube2 = null)
    {
        if (!cube2) cube2 = grass;

        GameObject cube = Instantiate(cube2.prefab, position, Quaternion.identity);
        cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
        cube.tag = "Cube";
        cube.name = cube2.name + ": " + position;
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

    List<Block> GenerateOreList(int x, int z, int height, float oreThreshold, Block blockPrefab, float oreScale)
    {
        var verticalBlock = new List<Block>();
        float halfHeight = height / 2.0f; // Half of the height

        for (int y = (int)-halfHeight; y <= halfHeight; ++y)
        {
            float xCoord = 0.1f * (x + y) * oreScale; // Adjusted x coordinate
            float zCoord = 0.1f * (z + y) * oreScale; // Adjusted z coordinate

            float oreValue = Mathf.PerlinNoise(xCoord, zCoord);

            float adjustedThreshold = oreThreshold + Mathf.Abs((float)y) / 100;
            
            if (oreValue > adjustedThreshold)
            {
                // Create a new Block object with the desired properties
                Block oreBlock = ScriptableObject.CreateInstance<Block>();
                oreBlock.InstantiateBlock(blockPrefab);
                oreBlock.location = new Vector3(x, Mathf.Floor(y + halfHeight), z);

                // Add the Block object to the list
                verticalBlock.Add(oreBlock);
            }
        }
        return verticalBlock;
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
                var blockList = coordsToHeight.ContainsKey(pos2D) ? coordsToHeight[pos2D].blocks : new List<Block>();
                if (blockList.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < blockList.Count; ++i)
                {
                    if (blockList[i].location == pos)
                    {
                        blockList[i] = block;
                        break;
                    }
                }

                var a = coordsToHeight[pos2D];
                a.isLoaded = false;
                coordsToHeight[pos2D] = a;
                DestroyWithChildren(cube.gameObject); // Remove cube from the scene
            }
        }
    }

    public Dictionary<Vector2, VerticalBlocks> GetHeightMap()
    {
        return coordsToHeight;
    }

    public void SetHeightMap(Dictionary<Vector2, VerticalBlocks> heightMap)
    {
        coordsToHeight = heightMap;
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

                if (block)
                {
                    blockList.Remove(block);

                    var cubeToRemove = GameObject.Find(block.name + ": " + position);
                    
                    DestroyWithChildren(cubeToRemove.gameObject);

                    var surroundingBlocks = GetSurroundingBlocks(position);
                    
                    if(block.blockType == Block.BlockType.Light && lightingBlocks.Contains(block))
                    {
                        lightingBlocks.Remove(block);
                    }


                    foreach (Vector3 surroundingBlock in surroundingBlocks)
                    {
                        var foundBlock = FindBlock(surroundingBlock);
                        if (foundBlock && !foundBlock.isLoaded)
                        {
                            AddBlock(surroundingBlock, foundBlock);
                        }
                    }

                    BuildNavmesh();
                    return true;
                }
            }
        }

        return false;
    }

    private Vector3[] GetSurroundingBlocks(Vector3 location)
    {
        Vector3[] surroundingBlocks = new Vector3[6];
        surroundingBlocks[0] = new Vector3(location.x + cubeHeight, location.y, location.z);
        surroundingBlocks[1] = new Vector3(location.x - cubeHeight, location.y, location.z);
        surroundingBlocks[2] = new Vector3(location.x, location.y + cubeHeight, location.z);
        surroundingBlocks[3] = new Vector3(location.x, location.y - cubeHeight, location.z);
        surroundingBlocks[4] = new Vector3(location.x, location.y, location.z + cubeHeight);
        surroundingBlocks[5] = new Vector3(location.x, location.y, location.z - cubeHeight);
        return surroundingBlocks;
    }


    public bool AddBlock(Vector3 position, Block blockToAdd)
    {
        Vector2 pos = new Vector2(position.x, position.z);
        if (coordsToHeight.ContainsKey(pos))
        {
            var blockList = coordsToHeight.ContainsKey(pos) ? coordsToHeight[pos].blocks : new List<Block>();
            if (blockList.Count > 0)
            {
                blockToAdd.isLoaded = true;
                for (int i = 0; i < blockList.Count; ++i)
                {
                    if (blockList[i].location == position)
                    {
                        blockList[i] = blockToAdd;
                        break;
                    }
                }

                if (!blockList.Contains(blockToAdd))
                {
                    blockList.Add(blockToAdd);
                }
                
                if(blockToAdd.blockType == Block.BlockType.Light && !lightingBlocks.Contains(blockToAdd))
                {
                    lightingBlocks.Add(blockToAdd);
                }

                InstantiateCube(position, blockToAdd);
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

        if (coordsToHeight.TryGetValue(pos, out var heightData) && heightData.blocks.Count > 0)
        {
            foreach (var block in heightData.blocks)
            {
                if (block.location.Equals(position))
                {
                    return block;
                }
            }
        }

        return null;
    }


    public void DestroyAllCubes()
    {
        var allCubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (var cube in allCubes)
        {
            DestroyWithChildren(cube.gameObject);
        }
    }



}



[Serializable]
public struct VerticalBlocks
{
    public List<Block> blocks { get; set; }
    public bool isLoaded { get; set; }

    public VerticalBlocks(List<Block> blocks, bool isLoaded)
    {
        this.blocks = blocks;
        this.isLoaded = isLoaded;
    }

}
