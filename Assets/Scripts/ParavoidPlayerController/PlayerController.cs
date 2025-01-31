﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Objects
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Rigidbody rb;
   
    //Base Physics
    private Vector3 playerVelocity;

    //Primary Controls
    public PlayerControls playerControls;
    public Transform groundCheck;
    public bool isGrounded;
    public float playerSpeed = 2.0f;
    public float sensitivity;
    public float jumpHeight = 2.0f;
    public float gravityValue = -9.18f;
    public float jumpTimeWindow = 0.1f;
    private Vector3 moveDir;
    private Vector2 moveVect;
    [SerializeField] private Vector2 smoothInputVelocity;
    [SerializeField] private float smoothInputSpeed;
    public LayerMask whatIsGround;
    
    //Secondary Mechanics    
    public bool doubleJumpUnlocked = false;
    public bool hasDoubleJumped;
    public bool hasJumpedOnce;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerCamera = PlayerCamera.singleton;
        rb = gameObject.GetComponent<Rigidbody>();
        if (rb)
        {
            Debug.Log("Rigidbody found!");
        }
    }

    void OnEnable()
    {
        playerControls.Enable();
    }    

    void OnDisable()
    {
        playerControls.Disable();
    }

    public Vector2 getPlayerMoveVector()
    {
        return playerControls.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 getMouseDeltaVector()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerJumped()
    {
        return playerControls.Player.Jump.triggered;
    }

    void Update()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, whatIsGround);
        if (isGrounded && playerVelocity.y <= 0)
        {
            playerVelocity.y = 0;
            hasJumpedOnce = false;
            hasDoubleJumped = false;
            jumpTimeWindow = 0.1f;
        }

        HandleMouseMovement();
        HandleMoveInput();
        UpdateJumpWindow();
        
    }

    private void FixedUpdate()
    {
        HandlePlayerMovement();
    }

    private void UpdateJumpWindow()
    {
        jumpTimeWindow -= Time.deltaTime;
    }


    private void HandleMoveInput()
    {
        Vector2 newMoveVect;
        newMoveVect = new Vector2(getPlayerMoveVector().x, getPlayerMoveVector().y);
        
        moveVect = Vector2.SmoothDamp(moveVect, newMoveVect, ref smoothInputVelocity, smoothInputSpeed);
                
        moveDir = (transform.forward * moveVect.y + transform.right * moveVect.x) * playerSpeed;

        if (PlayerJumped() && canJump())
        {
            Jump();
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        
    }

    private void HandlePlayerMovement()
    {
        rb.velocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);
    }

    private void Jump()
    {
        playerVelocity.y = 0f;
        playerVelocity.y += Mathf.Sqrt(jumpHeight/2 * -3.0f * gravityValue);
        // if (!isGrounded && hasJumpedOnce && canJump())
        // {
        //     Debug.Log("Double Jump");
        //     playerVelocity.y = 0f;
        //     playerVelocity.y += Mathf.Sqrt(jumpHeight/2 * -3.0f * gravityValue);
        //     hasJumpedOnce = true;
        //     hasDoubleJumped = true;  
        // } else if ((isGrounded || jumpTimeWindow > 0) && !hasJumpedOnce)
        // {
        //     Debug.Log("Single Jump");
        //     playerVelocity.y = 0f;
        //     playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        //     hasDoubleJumped = false;
        //     hasJumpedOnce = true;
        // }
        
    }

    private void HandleMouseMovement()
    {
        float delta = Time.fixedDeltaTime;

        if (playerCamera)
        {
            if (Time.timeScale != 0f)
            {
                playerCamera.CameraRotation(delta, getMouseDeltaVector().x, getMouseDeltaVector().y);
            }
        } else
        {
            playerCamera = PlayerCamera.singleton;
            if (!playerCamera)
            {
                Debug.LogError("Could not find player camera");
            }
        }
    }

    private bool canJump()
    {
        if (isGrounded)
        {
            hasJumpedOnce = false;
            return true;
        } else if (doubleJumpUnlocked && !isGrounded && !hasDoubleJumped)
        {
            hasJumpedOnce = true;
            return true;
        }
        return false;
    }

    public void EnableDoubleJump() {
        doubleJumpUnlocked = true;
    }
}
