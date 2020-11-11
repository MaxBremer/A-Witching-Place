using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    public CharacterController controller;

    //A few basic player stats.
    //Public instead of const so they can be changed in-editor and/or in-game.
    public float speed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    //Ground info.
    public Transform groundCheck;
    public float groundDistance = .4f;
    public LayerMask groundMask;

    public static bool frozen;

    //layermask for the Player itself.
    public LayerMask layerMask;

    //to check for collisions.
    public TerrainGenerator tg;

    Vector3 velocity;
    //serialized for debugging, so I can see in-editor without turning on big ol debug mode.
    [SerializeField]bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        frozen = false;
        tg = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!frozen)
        {
            
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                //If we are grounded, halt our downwards velocity.
                //Though, for some reason, it seems to work better to use -2f than 0, makes player "stick" better.
                velocity.y = -2f;
            }

            //Movement axes. Simple stuff.
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                //y velocity to be added for jump. -2f increases magnitude and cancels negative of gravity coefficient.
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move((move * speed + velocity) * Time.deltaTime);
        }
    }

    
}
