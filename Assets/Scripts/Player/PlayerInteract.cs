using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{

    public float playerReach = 4;
    public LayerMask groundLayer;
    TerrainGenerator tg;
    public GameObject terrainController;
    // Start is called before the first frame update
    void Start()
    {
        tg = terrainController.GetComponent<TerrainGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool left = Input.GetMouseButtonDown(0);
        bool right = Input.GetMouseButtonDown(1);
        //On left or right click, raycast to look for terrain so we can destroy/add a block respectively.
        if (left || right)
        {
            Debug.Log("click");
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, playerReach, groundLayer))
            {
                Debug.Log("hit");
                Vector3 truPos = hit.point;
                Vector3 hitPos;
                if (left)
                {
                    //move the hit position slightly inwards to ensure that we destroy the block at the correct location
                    hitPos = truPos + (transform.forward * .01f);
                    tg.setBlockAt(Mathf.FloorToInt(hitPos.x), Mathf.FloorToInt(hitPos.y), Mathf.FloorToInt(hitPos.z), 0);
                }
                else
                {
                    //move the hit position slightly outwards to ensure that we add a block at the correct location.
                    hitPos = truPos - (transform.forward * .01f);
                    //NOTE: for testing, player can only add block of type 1.
                    tg.setBlockAt(Mathf.FloorToInt(hitPos.x), Mathf.FloorToInt(hitPos.y), Mathf.FloorToInt(hitPos.z), 1);
                }
                
            }
        }
    }
}
