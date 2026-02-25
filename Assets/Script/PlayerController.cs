using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System;

public class PlayerController : MonoBehaviour
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
    public AudioSource cameraZoomSound;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        //jumping
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

        if(canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        //zooming
        if(Input.GetButtonDown("Fire2"))
        {
            isZooming = true;
            cameraZoomSound.Play();
        }
        if(Input.GetButtonUp("Fire2"))
        {
            isZooming = false;
            cameraZoomSound.Play();
        }

        if(isZooming)
        {
            playerCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, Time.deltaTime * zoomSmoothness);
        }
        else if(!isZooming)
        {
            playerCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV, Time.deltaTime * zoomSmoothness);
        }
    }
}
