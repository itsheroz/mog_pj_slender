using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerController : MonoBehaviourPun
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
    private bool isInStaticRange = false;    // ตอนนี้อยู่ในระยะ static ไหม

    // === HP System ===
    [Header("HP System")]
    public float maxHP = 100f;               // เลือดสูงสุด
    public float damagePerSecond = 10f;      // ลดเลือดต่อวินาที (เมื่ออยู่ใกล้ Slenderman)
    public float regenPerSecond = 5f;        // ฟื้นเลือดต่อวินาที (เมื่ออยู่ไกล)
    public Image hpBar;                     // ลาก HP Bar (UI Image, ตั้ง Image Type = Filled) จาก Player prefab เข้ามา
    private float currentHP;                 // เลือดปัจจุบัน
    private bool isDead = false;             // ตายแล้วหรือยัง
    public bool IsDead { get { return isDead; } }  // ให้ script อื่นเช็คได้

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

            // ปิด Canvas/HUD ของ player คนอื่น (ป้องกัน UI ซ้อนทับ)
            Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
            foreach (Canvas c in canvases)
            {
                c.gameObject.SetActive(false);
            }

            // ปิด AudioListener ของ player คนอื่น (ป้องกัน warning ซ้ำ)
            AudioListener[] listeners = GetComponentsInChildren<AudioListener>(true);
            foreach (AudioListener al in listeners)
            {
                al.enabled = false;
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

        // ลบ AudioListener ตัวเกินทั้งหมด (เช่น ของ BGMPlayer)
        // เหลือแค่ตัวที่อยู่บนกล้องของเราเท่านั้น
        AudioListener myListener = playerCamera != null ? playerCamera.GetComponent<AudioListener>() : null;
        AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (AudioListener al in allListeners)
        {
            if (al != myListener)
            {
                Destroy(al);
            }
        }



        // ซ่อน static effect ตอนเริ่มเกม
        if (staticObject != null)
        {
            staticObject.SetActive(false);
        }

        // ตั้งค่า HP เริ่มต้น
        currentHP = maxHP;
        isDead = false;
        UpdateHPBar();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // ถ้าตายแล้วไม่ทำอะไร
        if (isDead) return;

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

        // === Zoom (กดค้าง = zoom, ปล่อย = หยุด) ===
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

        // === HP: ลด/เพิ่มเลือดตามระยะ Slenderman ===
        UpdateHP();

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
            isInStaticRange = true;
        }
        else
        {
            if (staticObject.activeSelf)
                staticObject.SetActive(false);
            isInStaticRange = false;
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

    /// <summary>
    /// อัพเดท HP: ลดเลือดเมื่ออยู่ใกล้ Slenderman, ฟื้นเลือดเมื่ออยู่ไกล
    /// </summary>
    private void UpdateHP()
    {
        if (isDead) return;

        if (isInStaticRange)
        {
            // อยู่ในระยะ static → ลดเลือด
            currentHP -= damagePerSecond * Time.deltaTime;
        }
        else
        {
            // อยู่นอกระยะ → ฟื้นเลือด
            currentHP += regenPerSecond * Time.deltaTime;
        }

        // จำกัดค่า HP ไม่ให้เกิน max หรือต่ำกว่า 0
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        // อัพเดท HP Bar UI
        UpdateHPBar();

        // เช็คตาย
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// อัพเดท UI แถบเลือด
    /// </summary>
    private void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHP / maxHP;
        }
    }

    /// <summary>
    /// ผู้เล่นตาย — แจ้งทุก client ผ่าน RPC
    /// </summary>
    private void Die()
    {
        isDead = true;
        canMove = false;

        // แจ้งทุก client ว่าผู้เล่นตาย
        photonView.RPC("PlayerDiedRPC", RpcTarget.All);
    }

    [PunRPC]
    private void PlayerDiedRPC()
    {
        // ซ่อน static effect
        if (staticObject != null)
            staticObject.SetActive(false);

        // ปิดการเคลื่อนไหว
        canMove = false;
        isDead = true;

        Debug.Log(gameObject.name + " has died!");

        // TODO: เพิ่ม Game Over UI หรือ Respawn logic ตรงนี้
    }
}
