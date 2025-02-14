using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    /*This basic movement code comes from "FIRST PERSON MOVEMENT in 10 MINUTES - Unity Tutorial" by "Dave / GameDevelopment" on YouTube.
    I've totally over-commented it so that y'all can understand all the moving parts here, but as you work with it, if the comments are distracting, just delete them!
    If you'd like to see him build this code from the ground up in a succinct 10 minute tutorial, please watch the original video.
    This code is designed to be simple and EXPANDABLE and in further videos he adds aditional movement options that you can add too.
    He expands on this code to include slope movement, sprinting, crouching, throwing, sliding, climbing, dashing, and grappling hook swinging.
    So if any of that sounds like something you'd like in your game, go ahead and check out his videos. */

public class PlayerMovement : MonoBehaviour
{
    //Headers are a great way to keep your Inspector organized if you're going to have lots of public, exposed variables
    [Header("Movement")]
    public float moveSpeed; //Public so we'll be able to customize this in the inspector

    public float groundDrag; //essentially how much friction we'll have on the floor.

    public float jumpForce; //How much force we exert on the rigidbody when we jump. Basically ike jump speed/height 
    public float jumpCooldown; //How long until we can jump again
    public float airMultiplier; //kind of like our air resistance
    bool readyToJump; //True if we can jump, false if not

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")] //We're gonna check when we're touching the ground for a couple of reasons like knowing when we can jump and when to apply drag
    public float playerHeight; //so we can tell where the ground is relative to the player's center
    public LayerMask whatIsGround; //we'll apply this layer to any "Ground" objects in our scene
    bool grounded; //This will be true if we're touching the ground, false if we're not


    public Transform orientation; //public because we'll drag and drop our Orientation object in here to capture its transform data (location, rotation, and scale)

    //these are Not public because they'll be accepting our keyboard inputs, not something we want visible or customizable in our Inspector
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection; //A vector3 holds a 3d direction, and a magnitude 

    Rigidbody rb; //This will hold a reference to our player's rigidbody. We'll assign it in the code so we don't need it to be public



    void Start() // Start is called before the first frame update
    {
        rb = GetComponent<Rigidbody>(); //tada now "rb" is a reference to our RigidBody component
        rb. freezeRotation = true; //we don't want it rotating on its own.

        readyToJump = true; //makes us able to jump when the game start
    }



    private void Update() // Update is called once per frame, making it frame dependent and good for inputs
    {
        //this is where we'll check if we're grounded, before letting us input movement, just in case we try to hit the jump button while we're in the air or something
        // we are grounded if when we shoot a raycast from our center to just below us (half our height plus a little extra) we hit something with our ground layer
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput(); //Every frame we're gonna collect our inputs using a custom method of our own creation
    
        SpeedControl(); //call our function to cap our max movement speed


        //handle drag
        if(grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }


    private void FixedUpdate() //FixedUpdate is called at a constant REAL WORLD rate, indpendent of rame rate, making it good for physics calculations
    {
        MovePlayer(); //We are gonna run our custom physics method continuously and at a constant rate
    }


    private void MyInput() //this is our own custom method. Private because no other scripts need to call it, and void because it won't return a value when run
    {
        //Here we're getting our inputs. By default in Unity "Horizontal" is A/D and Left arrow/Right arrow
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical"); //Vertical W/S and Up/Down

        //when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown); //really cool method that lets us call our custom ResetJump method after waiting the length of time of jumpCooldown
        }
    }


    private void MovePlayer()
    {
        //calculate movement direction
        //The direction we move = Y axis direction (forward) * Forward/Backwards input + X axis direction (right) * Left/Right input
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //now that we have our direction, we add it as a force to our rigidbody
        //normalizing a vector sets its magnitude to 1, so we won't have bigger or smaller vectors, just directions. Force mode force is like continuously pushing on something, rather than one force one time
        if (grounded) //we have certain movement rules for on the ground, and different ones for in the air
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded) //you could probably just say "else" but better safe than sorry
        {
            //we just multiply in our air multiplier so we can't move as much in the air as we can on the ground
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    //Without this, the player's max speed would be greater than his move speed. We're just gonna put a cap on his max speed
    private void SpeedControl()
    {
        //We're gonna get the player's "flat velocity", literally just his velocity in the X direction and Z direction without caring how fast he's moving up or down
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            //again, normalizing a vector just sets its magnitude to 1 while keeping the direction its facing, then we set its magnitude to our movespeed because that's as fast as we ever want the player going
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z); //you'll notice we're not changing the up down speed. We're only putting a cap on the "flat velocity"
        }
    }

    private void Jump()
    {
        //reset y velocity to 0 just in case
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //Add our jump force in the up direction. Force mode impulse is like a single one time force, rather than a continuous pushing force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true; //let us be able to jump again
    }
}
