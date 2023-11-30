using System.Collections.Generic;
using UnityEngine;

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
    int loadDistance = 30; // Distance around the player to load new terrain

    Dictionary<Vector2, float> coordsToHeight = new Dictionary<Vector2, float>();

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

        print(coordsToHeight.Keys.Count);
        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= loadDistance/2 ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= loadDistance/2)
        {
            previousPlayerPosX = currentPlayerPosX;
            previousPlayerPosZ = currentPlayerPosZ;
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
    }

    void GenerateTerrain()
    {
        int currentPlayerPosX = (int)playerTransform.position.x;
        int currentPlayerPosZ = (int)playerTransform.position.z;

        for (int x = previousPlayerPosX - loadDistance; x < previousPlayerPosX + loadDistance; x++)
        {
            if (Mathf.Abs(x - currentPlayerPosX) >= loadDistance)
            {
                // Do not regenerate cubes within the visible area
                continue;
            }

            for (int z = previousPlayerPosZ - loadDistance; z < previousPlayerPosZ + loadDistance; z++)
            {
                if (Mathf.Abs(z - currentPlayerPosZ) >= loadDistance)
                {
                    // Do not regenerate cubes within the visible area
                    continue;
                }

                GenerateCubeAtPosition(x, z);
            }
        }

        previousPlayerPosX = currentPlayerPosX;
        previousPlayerPosZ = currentPlayerPosZ;
    }

    void GenerateCubeAtPosition(int x, int z)
    {

        if (!coordsToHeight.ContainsKey(new Vector2(x, z)))
        {
            float y = Mathf.PerlinNoise(x * 0.1f * scale, z * 0.1f * scale) * 3f;
            y = Mathf.Round(y / cubeHeight) * cubeHeight;
            Vector3 cubePos = new Vector3(x, y / 2f, z); // Adjust cube position based on height
            GameObject cube = Instantiate(cubePrefab, cubePos, Quaternion.identity);

            // Set a fixed cube size for width, height, and depth
            cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
            cube.transform.parent = transform;
            coordsToHeight.Add(new Vector2(x, z), y);

        }
        else
        {
            // We will instantiate a new cube here with the same coordinates
            // as the one we removed from the scene
            Instantiate(cubePrefab, new Vector3(x, coordsToHeight[new Vector2(x, z)] / 2f, z), Quaternion.identity);
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
            Destroy(cube); // Remove cube from the scene
        }
    }
}
}