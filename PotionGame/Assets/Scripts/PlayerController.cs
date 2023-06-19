using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpHeight = 10f;
    public float gravity = 9.8f;
    public float airControl = 10f;

    // 1 = no friction, 0 = no movement
    public float friction = 1f;
    float moveSpeedStore;

    public static bool isUIActive;

    public AudioClip walkingSFX;
    public AudioClip jumpStartSFX;

    AudioSource audioSource;
    CharacterController controller;
    Vector3 input, moveDirection;

    private RecipeManager recipeManager;

    // Stores the last position where we were grounded and above water
    private Vector3 lastLandedPos;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        recipeManager = GetComponent<RecipeManager>();
        audioSource = GetComponent<AudioSource>();
        moveSpeedStore = moveSpeed;
        lastLandedPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal;
        float moveVertical;
        bool jump;

        if (!isUIActive && controller.enabled)
        {
            // If we are not in UI, get input
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical = Input.GetAxis("Vertical");
            jump = Input.GetButton("Jump");
        } else
        {
            // Otherwise, don't
            moveHorizontal = 0f;
            moveVertical = 0f;
            jump = false;
        }
        

        input = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;

        // sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 1.5f * moveSpeedStore;
        }
        else
        {
            moveSpeed = moveSpeedStore;
        }

        input *= moveSpeed;

        bool isMoving = input.magnitude > 0.01f;

      
        if (controller.isGrounded)
        {
            // Record our position so we can teleport back here if we enter water
            if(!HeadUnderwaterCheck.isHeadUnderwater())
            {
                lastLandedPos = transform.position;
            }


            moveDirection = input;

            // apply friction to slow down player
            moveDirection.x *= friction;
            moveDirection.z *= friction;

            if (isMoving && !audioSource.isPlaying)
            {
                audioSource.clip = walkingSFX;
                audioSource.Play();
            }
            else if (!isMoving)
            {
                audioSource.Stop();
            }

            if (jump)
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

        // If we are in water and not under the proper potion effect, teleport us back to land.
        if (HeadUnderwaterCheck.isHeadUnderwater() && !(PotionEffectManager.Instance.GetCurrentEffect() == "SeaStrider"))
        {
            transform.position = lastLandedPos;
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