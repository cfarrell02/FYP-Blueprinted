using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;


public class BlockyTerrain : MonoBehaviour
{
    
    public int depth = 10;
    public float scale = 1f;
    public float cubeHeight = 1f; // Set a fixed cube height

    public GameObject
        enemyPrefab; // These prefabs, will be changes to list or dictionary for different types of enemies

    public Block cubeObject, dirt; // This needs to be changed to a list or dictionary for different types of blocks



    int previousPlayerPosX;
    int previousPlayerPosZ;
    [SerializeField] int loadDistance = 40; // Distance around the player to load new terrain
    [SerializeField] int navMeshDistance = 20; // Distance around the player to load new terrain
    [SerializeField] int newTerrainDistance = 10; // Multiplier for the load distance when generating terrain
    [SerializeField] int newNavMeshDistance = 10; // Multiplier for the load distance when generating terrain
    [SerializeField] int perlinScale = 10; // Multiplier for the perlin noise scale




    private Transform playerTransform;
    private Dictionary<Vector2, VerticalBlocks> coordsToHeight = new Dictionary<Vector2, VerticalBlocks>();
    private NavMeshSurface surface;
    private float timer = 0f;
    private LightingManager lightingManager;
    private bool isFillInHolesRunning = false;

    private void Awake()
    {
        var allCubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (var cube in allCubes)
        {
            Destroy(cube);
        }
    }

    void Start()
    {
        //Get surface in child
        surface = GetComponentInChildren<NavMeshSurface>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        previousPlayerPosX = (int)playerTransform.position.x;
        previousPlayerPosZ = (int)playerTransform.position.z;
        lightingManager = GameObject.Find("LightingManager").GetComponent<LightingManager>();

        if (!isFillInHolesRunning)
            StartCoroutine(FillInHoles());
        
        StartCoroutine(LateStart()); //Late start on the loading to ensure the player has been loaded

    }
    
    IEnumerator LateStart()
    {
        yield return new WaitForNextFrameUnit();
        var file = File.Exists(GameManager.Instance.savePath + GameManager.Instance.currentSaveFile + ".json");
        if (file)
        {
            GameManager.Instance.LoadGame(GameManager.Instance.currentSaveFile + ".json");
        }
        else
        {
            GenerateInitialTerrain();
        }
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
            previousPlayerPosX = currentPlayerPosX;
            previousPlayerPosZ = currentPlayerPosZ;
            GenerateTerrain();
            UnloadTerrain();

        }

        if (lightingManager && lightingManager.isNight())
            HandleEnemySpawn();
        
    }

    void HandleNavmesh()
    {
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;

        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= newNavMeshDistance ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= newNavMeshDistance)
        {
            //print("Building navmesh");
            //Ensure only blocks that are within the navmesh distance are parented to the navmesh
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
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
                    InstantiateCube(block.location);
                }
            }

            coordsToHeight[currentPos] = verticalBlocks;
        }
        else
        {

            float y = Mathf.PerlinNoise(x * 0.1f * scale, z * 0.1f * scale) * perlinScale;
            y = Mathf.Floor(y / cubeHeight) * cubeHeight;

            List<Block> verticalBlocks = new List<Block>();

            for (int i = -depth; i <= y; ++i)
            {
                Vector3 cubePos = new Vector3(x, i, z);

                bool toBeLoaded = i >= y;

                if (!isFillInHolesRunning)
                    StartCoroutine(FillInHoles());

                Block copyOfCubeObject = ScriptableObject.CreateInstance<Block>();

                if (toBeLoaded)
                {
                    InstantiateCube(cubePos, cubeObject);
                    copyOfCubeObject.InstantiateBlock(cubeObject);
                }
                else
                {
                    copyOfCubeObject.InstantiateBlock(dirt);
                }


                copyOfCubeObject.location = cubePos;
                copyOfCubeObject.isLoaded = toBeLoaded;


                verticalBlocks.Add(copyOfCubeObject);
            }

            coordsToHeight.Add(currentPos,
                new VerticalBlocks { blocks = verticalBlocks, isLoaded = true, navMeshBuilt = false });
        }
    }

    IEnumerator FillInHoles()
    {
        isFillInHolesRunning = true;
        // var loadedBlocks = coordsToHeight.Where(x => x.Value.isLoaded).ToDictionary(x => x.Key, x => x.Value);
        // foreach (var block in loadedBlocks)
        // {
        //     var blockList = block.Value.blocks;
        //     if (blockList.Count > 0)
        //     {
        //         for (int i = 0; i < blockList.Count; ++i)
        //         {
        //             if (!blockList[i].isLoaded)
        //             {
        //                 var surroundingBlocks = GetSurroundingBlocks(blockList[i].location);
        //                 foreach (Vector3 surroundingBlock in surroundingBlocks)
        //                 {
        //                     var foundBlock = FindBlock(surroundingBlock);
        //                     if (!foundBlock)
        //                     {
        //                         InstantiateCube(blockList[i].location); //The block is touching air
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     yield return null;
        // }
        yield return null;

        isFillInHolesRunning = false;
    }

    public void InstantiateCube(Vector3 position, Block cube2 = null)
    {
        if (!cube2) cube2 = cubeObject;
        

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
                Destroy(cube); // Remove cube from the scene
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
                    Destroy(cubeToRemove);

                    var surroundingBlocks = GetSurroundingBlocks(position);


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
        Destroy(cube);
    }
}





};

[Serializable]
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
