using UnityEngine;

public class PerlinNoiseTerrain : MonoBehaviour
{
    public int gridSizeX = 10; // Number of cubes along the X-axis
    public int gridSizeZ = 10; // Number of cubes along the Z-axis
    public float scale = 1f; // Noise scale affecting the terrain's height

    public GameObject cubePrefab; // Prefab of the cube to be instantiated

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float y = Mathf.PerlinNoise(x * 0.1f * scale, z * 0.1f * scale) * 3f; // Adjust height with multiplier

                Vector3 cubePos = new Vector3(x, y, z); // Set cube position based on noise
                GameObject cube = Instantiate(cubePrefab, cubePos, Quaternion.identity); // Instantiate cube
                cube.transform.localScale = new Vector3(1f, 0.1f + y, 1f); // Adjust scale based on noise
                cube.transform.parent = transform; // Set cubes as children of the terrain object
            }
        }
    }
}
