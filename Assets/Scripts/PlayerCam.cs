using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

    /*This basic movement code comes from "FIRST PERSON MOVEMENT in 10 MINUTES - Unity Tutorial" by "Dave / GameDevelopment" on YouTube.
    I've totally over-commented it so that y'all can understand all the moving parts here, but as you work with it, if the comments are distracting, just delete them!
    If you'd like to see him build this code from the ground up in a succinct 10 minute tutorial, please watch the original video.
    This code is designed to be simple and EXPANDABLE and in further videos he adds aditional movement options that you can add too.
    He expands on this code to include slope movement, sprinting, crouching, throwing, sliding, climbing, dashing, and grappling hook swinging.
    So if any of that sounds like something you'd like in your game, go ahead and check out his videos. */

public class PlayerCam : MonoBehaviour
{
    //These are  customizable variables for your mouse's horizontal or "X" sensitivity, and vertical or "Y" sensitivity.
    //They're public because we ant to be able to see and change them in the Unity Inspector, and floats because we want them to be able to have decimals.
    public float sensX; 
    public float sensY;

    //A "Transform" stores position, rotation, and scale. This will be for the direction the player is facing.
    //It's public because we want it exposed in the Inspector so we can drag in a GameObject and store its transform data.
    public Transform orientation; 

    //These will store the X and Y rotation of the camera. Not public because we don't need these in the Inspector.
    float xRotation;
    float yRotation;

    //As you know, Start runs just once on the very first frame.
    //the "private" means no other scripts can call this method,
    //and the "void" means it doesn't "return" any value when it is run.
    private void Start()
    {
        //These two just make the cursor locked to the center of the screen and invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //Update runs every frame. It's biggest con is that it is "frame dependent" meaning if you start dropping frames,
    //this code will LITERALLY run fewer times per second.
    //For that reason, Update is best for handling user input, animations, and non-physics-related logic (which should go in FixedUpdate)

    private void Update()
    {
        //These track mouse movement.
        //Input.GetAxisRaw("Mouse X") returns the magnitude of the mouse's movement along the X axis (left and right).
        //Time.deltaTime tracks the amount of real-world time elapsed since the last frame
        //sensX is our mouse sensitivity which we can make bigger or smaller
        //By multiplying our input by Time.deltaTime, we make our input "frame IN-dependent" even though its in Update (not FixedUpdate)
        //That way the mouse won't move faster at higher frame rates and slower on lower ones. If you want to know why we don't just put it in FixedUpdate, read on  =>        Essentially inputs can only be taken on actual frames. FixedUpdate achieves frame independence by making calculations to pretend it is running at a continuous rate divorced from the game's potentially fluctuating frame rate. But those pretend frames aren't actual frames in which a player might be pressing a key or moving a mouse, so you can't ever rely on FixedUpdate when it comes to player input code. Player input code has to be taken every REAL FRAME which is what Update does.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY; //this is the exact same but with Y for up-down input

        //if this feels backwards and confusing, you're not alone. I'll try to explain it but also feel free to just accept it as magic and keep going. Essentially   =>    Picture yourself standing straight up. The axis that goes vertically from your shoes through the top of your head is the y axis. Imagine spinning around that axis like a rotisserie. Your view is turning left and right! So our left-right input, input we think of as being along the x axis, should actually affect y rotation. Now imagine an axis that runs in one ear and out the other. That's the x axis. If you tilt your head along that axis, you'll be looking up and down. Up and down (Y) input should rotate around the x axis.
        yRotation += mouseX;
        xRotation -= mouseY;
        //We now want to make sure you can't look up past straight up, or down past straight down,
        //that would start hurting your back or mean looking through your legs after all
        //we just "clamp" the rotation so it cant go past -90 or positive 90
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Here we are actually rotating our camera! There are some big words but don't worry too much about them   =>   Here we're turning Euler angles (which are simpler but can cause problems) into Quaternion angles (more complicated, but less likely to cause problems).
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        //Here we rotate the Player, note the player only turns left and right, we don't need it rotating its whole body in space to look up or down
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}