using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpHeight = 10f;
    public float gravity = 9.8f;
    public float airControl = 10f;

    public AudioClip walkingSFX;
    public AudioClip jumpStartSFX;

    AudioSource audioSource;
    CharacterController controller;
    Vector3 input, moveDirection;

    private RecipeManager recipeManager;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        recipeManager = GetComponent<RecipeManager>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!controller.enabled)
        {
            return;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        input = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;

        input *= moveSpeed;

        bool isMoving = input.magnitude > 0.01f;

      
        if (controller.isGrounded)
        {
            moveDirection = input;

            if (isMoving && !audioSource.isPlaying)
            {
                audioSource.clip = walkingSFX;
                audioSource.Play();
            }
            else if (!isMoving)
            {
                audioSource.Stop();
            }

            if (Input.GetButton("Jump"))
            {
                // Play jump start clip
                audioSource.clip = jumpStartSFX;
                audioSource.Play();

                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
                // doing this bc animation clips with camera on jump
                Camera.main.nearClipPlane = 0.9f;
            }
            else
            {
                moveDirection.y = 0.0f;
                Camera.main.nearClipPlane = 0.01f;
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

    public void TeleportPlayer(Vector3 position)
    {
        moveDirection = Vector3.zero;
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }
}