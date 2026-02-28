using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using System;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    //camera
    public Camera playerCamera;

    //movement
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpForce = 0f;
    public float gravity = 10f;

    //camera settings
    public float lookSpeed = 2f;
    public float lookXLimit = 75f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    //camera zoom settings
    public float zoomFOV = 30f;
    public float defaultFOV;
    public float zoomSmoothness = 1;
    private bool isZooming = false;

    //can the player move?
    private bool canMove = true;
    CharacterController characterController;

    //sound effects
    public AudioClip cameraZoomSound;

    // === Page Count UI (แสดงจำนวน page ที่เก็บได้) ===
    [Header("Page Count UI")]
    public GameObject countPage;             // ลาก countPage text (TextMeshProUGUI) จาก Player prefab เข้ามา
    private GameLogic gameLogicRef;           // reference ของ GameLogic (หาตอน runtime)

    // === Static Effect (เอฟเฟกต์เมื่อ Slenderman อยู่ใกล้) ===
    [Header("Static Effect")]
    public GameObject staticObject;          // ลาก static effect object (child ของ Main Camera) เข้ามา
    public float staticObjectDistance = 5f;  // ระยะที่จะแสดง static effect
    private Transform slendermanTransform;   // เก็บ reference ของ Slenderman (หาตอน runtime)

    // Network sync variables
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        if (!photonView.IsMine)
        {
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }

            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            return;
        }

        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (defaultFOV == 0 && playerCamera != null)
        {
            defaultFOV = playerCamera.fieldOfView;
        }

        networkPosition = transform.position;
        networkRotation = transform.rotation;

        // ซ่อน static effect ตอนเริ่มเกม
        if (staticObject != null)
        {
            staticObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            return;
        }

        // === Movement ===
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpForce;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        characterController.Move(moveDirection * Time.deltaTime);

        // === Camera ===
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // === Zoom ===
        if (Input.GetButtonDown("Fire2"))
        {
            isZooming = true;
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(cameraZoomSound);
        }
        if (Input.GetButtonUp("Fire2"))
        {
            isZooming = false;
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(cameraZoomSound);
        }

        if (isZooming)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, Time.deltaTime * zoomSmoothness);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV, Time.deltaTime * zoomSmoothness);
        }

        // === Static Effect: เช็คระยะกับ Slenderman ===
        CheckSlendermanDistance();

        // === Page Count UI: อัพเดทตัวเลข page ===
        UpdatePageCountUI();
    }

    /// <summary>
    /// เช็คระยะระหว่าง Player กับ Slenderman
    /// ถ้าใกล้กว่า staticObjectDistance → แสดง static effect
    /// ถ้าไกลกว่า → ซ่อน
    /// </summary>
    private void CheckSlendermanDistance()
    {
        // ถ้ายังไม่มี reference ของ Slenderman ให้หาจาก Tag
        if (slendermanTransform == null)
        {
            GameObject slenderman = GameObject.FindGameObjectWithTag("Slenderman");
            if (slenderman != null)
            {
                slendermanTransform = slenderman.transform;
            }
        }

        // ถ้าหาไม่เจอ หรือไม่มี static object ก็ return
        if (slendermanTransform == null || staticObject == null) return;

        float distance = Vector3.Distance(transform.position, slendermanTransform.position);

        if (distance <= staticObjectDistance)
        {
            if (!staticObject.activeSelf)
                staticObject.SetActive(true);
        }
        else
        {
            if (staticObject.activeSelf)
                staticObject.SetActive(false);
        }
    }

    /// <summary>
    /// อัพเดท UI แสดงจำนวน page ที่เก็บได้ (อ่านจาก GameLogic)
    /// </summary>
    private void UpdatePageCountUI()
    {
        if (countPage == null) return;

        // หา GameLogic จาก Tag (ครั้งเดียว)
        if (gameLogicRef == null)
        {
            GameObject glObj = GameObject.FindGameObjectWithTag("GameLogic");
            if (glObj != null)
            {
                gameLogicRef = glObj.GetComponent<GameLogic>();
            }
        }

        if (gameLogicRef != null)
        {
            TextMeshProUGUI textComponent = countPage.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = gameLogicRef.pageCount + "/8";
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
