using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float jumpSpeed = 5.0f;
    [SerializeField] private float gravity = 10.0f;
    [SerializeField] private Vector3 teleportPoint = new Vector3(0, 0.95f, 0);


    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownLimit = 65f;


    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnPoint = new Vector3(31.55f, 1.58f, 4.5f);


    private static float originalHeight = 1.8f;
    private static float growHeight = 7.2f;


    //Sounds
    public AudioClip teleportSound;
    public AudioClip jumpPadSound;
    public AudioClip deadSound;

    private AudioSource audioSource;



    private static float currentHeight = originalHeight;


    float currentSpeed;

    private float verticalRotation;

    [SerializeField] private Camera playerCamera;

    private Vector3 currentMovement = Vector3.zero;

    private CharacterController characterController;

    private Teleporter teleporter;

    public float teleportCooldown = 2f;
    private float lastTeleportTime = -Mathf.Infinity;

    private Rigidbody rb;



    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        characterController = this.GetComponent<CharacterController>();

        audioSource = GetComponent<AudioSource>();

        rb = this.GetComponent<Rigidbody>();

        playerCamera = GetComponentInChildren<Camera>();
    }


    void Update()
    {

        characterController.height = currentHeight;


        if (characterController.isGrounded && Input.GetKey(KeyCode.Space))
        {
            currentMovement.y = jumpSpeed;
        }
        currentMovement.y -= gravity * Time.deltaTime;
        characterController.Move(currentMovement * Time.deltaTime);

        HandleMovement();

        HandleLook();



    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "CubeGenerator")
        {
            CubeGenerator.SpawnCube();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Killbox")
        {
            if (deadSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deadSound);
            }
            characterController.enabled = false;
            gameObject.transform.position = spawnPoint;
            characterController.enabled = true;

        }

        if (other.gameObject.tag == "CheckPoint")
        {
            spawnPoint = gameObject.transform.position;
        }

        if (other.CompareTag("DoorButton"))
        {
            OpenDoor.IsPlayerInside = true;
        }

        if (other.CompareTag("BirdButton"))
        {
            currentMovement.y = 12;
            if (jumpPadSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpPadSound);
            }
        }

        if (other.CompareTag("Teleporter") && CanTeleport())
        {
            Teleporter teleporter = other.GetComponent<Teleporter>();

            if (teleporter != null && Teleporter.teleporterDestinations.ContainsKey(teleporter))
            {
                characterController.enabled = false;
                transform.position = Teleporter.teleporterDestinations[teleporter].position;
                characterController.enabled = true;
                lastTeleportTime = Time.time;
            }
            if (teleportSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(teleportSound);
            }
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DoorButton"))
        {
            OpenDoor.IsPlayerInside = false;
        }
    }


    bool CanTeleport()
    {
        return Time.time - lastTeleportTime >= teleportCooldown;
    }


    void HandleMovement()
    {


        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!Input.GetKey("c"))
            {
                currentSpeed = runSpeed;
            }
        }
        else if (Input.GetKey("c"))
        {
            currentSpeed = crouchSpeed;
        }

        else
        {
            currentSpeed = walkSpeed;
        }


        Vector3 horizontalMovement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        horizontalMovement = transform.rotation * horizontalMovement;

        currentMovement.x = horizontalMovement.x * currentSpeed;
        currentMovement.z = horizontalMovement.z * currentSpeed;



        characterController.Move(currentMovement * Time.deltaTime);


        if (Input.GetKeyDown("e"))
        {
            characterController.enabled = false;
            gameObject.transform.position = teleportPoint;
            characterController.enabled = true;
        }
    }



    void HandleLook()
    {
        float mouseXrotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        this.transform.Rotate(0, mouseXrotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownLimit, upDownLimit);


        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
