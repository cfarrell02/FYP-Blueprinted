using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using static UnityEditor.PlayerSettings;


public class BlockyTerrain : MonoBehaviour
{
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public float scale = 1f;
    public float cubeHeight = 1f; // Set a fixed cube height
    public GameObject cubePrefab;
    public Transform playerTransform; // Reference to the player's transform

    int previousPlayerPosX;
    int previousPlayerPosZ;
    int loadDistance = 40; // Distance around the player to load new terrain
    int loadDistanceMultiplier = 2; // Multiplier for the load distance when generating terrain

    private Dictionary<Vector2, VerticalBlocks> coordsToHeight = new Dictionary<Vector2, VerticalBlocks>();

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
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= loadDistanceMultiplier ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= loadDistanceMultiplier)
        {
            //previousPlayerPosX = currentPlayerPosX;
            //previousPlayerPosZ = currentPlayerPosZ;
            GenerateTerrain();
        }
        UnloadTerrain();
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
        var surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }
    void GenerateTerrain()
    {
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;


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
        previousPlayerPosX = currentPlayerPosX;
        previousPlayerPosZ = currentPlayerPosZ;

        VerticalBlocks currentVerti = coordsToHeight[new Vector2(currentPlayerPosX, currentPlayerPosZ)];

        if (!currentVerti.navMeshBuilt)
        {
            var surface = GetComponent<NavMeshSurface>();
            surface.BuildNavMesh();
            currentVerti.navMeshBuilt = true;
            // For all loaded blocks, set navMeshBuilt to true
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
                Vector3 pos = cube.transform.position;
                Vector2 pos2D = new Vector2(pos.x, pos.z);
                if (coordsToHeight.ContainsKey(pos2D))
                {
                    var selectedBlocks = coordsToHeight[pos2D];
                    selectedBlocks.navMeshBuilt = true;
                    coordsToHeight[pos2D] = selectedBlocks;
                   
                }
            }
        }
    }


    void GenerateCubeAtPosition(int x, int z)
    {
        Vector2 currentPos = new Vector2(x, z);

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
                    cube.tag = "Cube";
                    cube.transform.parent = transform;
                }

                var blockItem = new Block("Cube", 1, 100, 100, 1, 64, cubePos, Vector3.zero, Vector3.one, cubePrefab);
                blockItem.isLoaded = i == y;
                verticalBlocks.Add(blockItem);
            }


            coordsToHeight.Add(currentPos, new VerticalBlocks { blocks = verticalBlocks, isLoaded = true, navMeshBuilt = false });
        }
        else
        {
            var blockItem = coordsToHeight[currentPos].blocks;

            for (int i = 0; i < blockItem.Count; ++i)
            {
                Block block = blockItem[i];
                if (block.isLoaded)
                {
                    GameObject cube = Instantiate(cubePrefab, block.Location, Quaternion.identity);
                    var a = coordsToHeight[currentPos];
                    a.isLoaded = true;
                    coordsToHeight[currentPos] = a;
                    cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
                    cube.tag = "Cube";
                    cube.transform.parent = transform;
                }
            }
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
                    return true;
                }
            }
        }
        return false;
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
                var cube = Instantiate(cubePrefab, position, Quaternion.identity);
                var a = coordsToHeight[pos];
                a.isLoaded = true;
                coordsToHeight[pos] = a;
                cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
                cube.tag = "Cube";
                cube.transform.parent = transform;
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
