using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateTerrain : MonoBehaviour
{

    public GameObject cube;
    public int terrainSize = 10;
    //private float noiseSpacing = 10f;

    public float heightVariationLimit = 20f;

    public float xOrg;
    public float yOrg;

    // Start is called before the first frame update
    void Start()
    {
        for (int x = -terrainSize; x < terrainSize; x++)
        {
            for (int y = -terrainSize; y < terrainSize; y++)
            {
                float xcoord = (xOrg + (float)(x)) / (50);
                float ycoord = (yOrg + (float)(y)) / (50);
                float zVal = Mathf.PerlinNoise(xcoord, ycoord);
                zVal = (zVal - 0.5f) * 2;
                Instantiate(cube, new Vector3(x, (int)(zVal * heightVariationLimit), y), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
