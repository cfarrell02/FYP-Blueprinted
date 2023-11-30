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

        print("Player position: " + currentPlayerPosX + ", " + currentPlayerPosZ);
        print("Previous player position: " + previousPlayerPosX + ", " + previousPlayerPosZ);
        print("Distance: " + Mathf.Abs(currentPlayerPosX - previousPlayerPosX) + ", " + Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ));
        print("Load distance: " + loadDistance);

        // Check if the player has moved to a new grid area
        if (Mathf.Abs(currentPlayerPosX - previousPlayerPosX) >= loadDistance/2 ||
            Mathf.Abs(currentPlayerPosZ - previousPlayerPosZ) >= loadDistance/2)
        {
            previousPlayerPosX = currentPlayerPosX;
            previousPlayerPosZ = currentPlayerPosZ;
            GenerateTerrain();
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
        float y = Mathf.PerlinNoise(x * 0.1f * scale, z * 0.1f * scale) * 3f;
        y = Mathf.Round(y / cubeHeight) * cubeHeight;

        Vector3 cubePos = new Vector3(x, y / 2f, z); // Adjust cube position based on height
        GameObject cube = Instantiate(cubePrefab, cubePos, Quaternion.identity);

        // Set a fixed cube size for width, height, and depth
        cube.transform.localScale = new Vector3(1f, cubeHeight, 1f);
        cube.transform.parent = transform;
    }
}
