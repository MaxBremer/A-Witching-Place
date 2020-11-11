using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingAbilities : MonoBehaviour
{
    TerrainGenerator tg;

    //Class for debugging/testing abilities for the player.
    //Note that cube is currently a misnomer, as currently testing more advanced structures.
    Structure cubeStruct;
    Vector3Int cubeStartingPos = new Vector3Int(0, 40, 0);
    int cubeSize = 23;

    void Start()
    {
        tg = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();

        calcCubeTest();
    }

    // Update is called once per frame
    void Update()
    {
        //Place the testing structure, then increment the position for the next one by its size.
        if (Input.GetKeyDown(KeyCode.Y))
        {
            tg.placeStruct(cubeStruct, cubeStartingPos);
            cubeStartingPos += new Vector3Int(cubeSize, 0, 0);
            Debug.Log("width radius is " + cubeStruct.getRadius());
            //calcCubeTest();
        }
    }

    //Build out the Structure instance for our testing structure.
    void calcCubeTest()
    {
        cubeStruct = new Structure();
        for (int i = 0; i < cubeSize; i++)
        {
            for (int j = 0; j < cubeSize; j++)
            {
                for (int k = 0; k < cubeSize; k++)
                {
                    if (Mathf.Abs(i - (cubeSize/2)) < j && Mathf.Abs(k - (cubeSize / 2)) < j)
                    {
                        cubeStruct.addBlock(new Vector3Int(i, j, k), 1);
                    }
                }
            }
        }
    }
}
