using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpHeight = 10f;
    public float gravity = 9.8f;
    public float airControl = 10f; 

    CharacterController controller;
    Vector3 input, moveDirection;
    
    private RecipeManager recipeManager;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        recipeManager = GetComponent<RecipeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        input = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;

        input *= moveSpeed; 

        if (controller.isGrounded)
        {
            moveDirection = input; 
            // we can jump
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
            } 
            else
            {
                moveDirection.y = 0.0f;
            }
        }
        else
        {
            // we are midair
            input.y = moveDirection.y;
            moveDirection = Vector3.Lerp(moveDirection, input, airControl * Time.deltaTime);
        }

        // apply gravity 
        moveDirection.y -= gravity * Time.deltaTime; 

        controller.Move(moveDirection * Time.deltaTime);

        if (InRange.isInRange && Input.GetKeyDown(KeyCode.T))
        {
            recipeManager.displayRecipies();
        }

    }
}
