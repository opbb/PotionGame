using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpHeight = 10f;
    public float gravity = 9.8f;
    public float airControl = 10f;
    [Tooltip("The higher the tolerance, the steeper the surfaces the player can jump up.")]
    public float jumpGroundedTolerance = -.1f;

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

        SetOrLoadPrefs(true);
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

            if (jump && ExpensiveIsGrounded())
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

    private bool OnSteepSlope()
    {
        if (!controller.isGrounded) return false;

        RaycastHit slopeHit;
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, controller.height))
        {
            float slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
            if (slopeAngle > controller.slopeLimit) return true;
        }
        return false;
    }

    // Check if we're grounded using an expensive spherecast. Use sparingly
    private bool ExpensiveIsGrounded()
    {
        // This method is meant to double check that we are in fact grounded, not check if we are not grounded. If char controller says we're not, we aren't.
        if (controller.isGrounded)
        {
            RaycastHit hitInfo;
            if(Physics.SphereCast(transform.position + controller.center, controller.radius - controller.skinWidth - jumpGroundedTolerance, Vector3.down, out hitInfo, (controller.height / 2)))
            {
                return true;
            }
        } 
        return false;
    }

    private void SetOrLoadPrefs(bool flag = false) {
        if (flag) {
            // Load Prefs
            transform.position = new Vector3(PlayerPrefs.GetFloat("PlayerX", transform.position.x), PlayerPrefs.GetFloat("PlayerY", transform.position.y), PlayerPrefs.GetFloat("PlayerZ", transform.position.z));
            transform.rotation = Quaternion.Euler(PlayerPrefs.GetFloat("PlayerRotX", transform.rotation.eulerAngles.x), PlayerPrefs.GetFloat("PlayerRotY", transform.rotation.eulerAngles.y), PlayerPrefs.GetFloat("PlayerRotZ", transform.rotation.eulerAngles.z));
            lastLandedPos = new Vector3(PlayerPrefs.GetFloat("PlayerLandedX", transform.position.x), PlayerPrefs.GetFloat("PlayerLandedY", transform.position.y), PlayerPrefs.GetFloat("PlayerLandedZ", transform.position.z));
        }
        else {
            // Set Prefs
            PlayerPrefs.SetFloat("PlayerX", transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", transform.position.z);

            PlayerPrefs.SetFloat("PlayerRotX", transform.rotation.x);
            PlayerPrefs.SetFloat("PlayerRotY", transform.rotation.y);
            PlayerPrefs.SetFloat("PlayerRotZ", transform.rotation.z);

            PlayerPrefs.SetFloat("PlayerLandedX", lastLandedPos.x);
            PlayerPrefs.SetFloat("PlayerLandedY", lastLandedPos.y);
            PlayerPrefs.SetFloat("PlayerLandedZ", lastLandedPos.z);
        }
    }

    private void OnDestroy() {
        SetOrLoadPrefs();
    }
}