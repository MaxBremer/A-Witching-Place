using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    GameObject player;

    public GameObject invUI;

    float timer;
    public float tickSpeed;
    public float tickRate;

    public int seed;

    // Awake is called before Start.
    void Awake()
    {
        //Initialize this game's generation seed.
        seed = Random.Range(-100000000, 100000000);
        Debug.Log("seed is " + seed);
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector3(1, TerrainChunk.chunkHeight, 1);

        timer = 0f;
        
    }

    // Update is called once per frame
    void Update()
    {
        //Tick framework. Not yet used, for later features.
        //ticksPerSecond = tickSpeed / tickRate
        timer += Time.deltaTime * tickSpeed;
        if(timer >= tickRate)
        {
            timer = 0f;
            //TICK ACTIONS HERE
        }

        //Inventory framework for freezing movement and freeing mouse when opened.
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (invUI.activeSelf)
            {
                invUI.SetActive(false);
                MouseLook.frozen = false;
                PlayerMove.frozen = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                invUI.SetActive(true);
                MouseLook.frozen = true;
                PlayerMove.frozen = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    void FixedUpdate()
    {
        
    }
}
