﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public LayerMask whatIsGround;
    
    //Secondary Mechanics    
    public bool doubleJumpUnlocked = false;
    public bool hasDoubleJumped;
    public bool hasJumpedOnce;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerCamera = PlayerCamera.singleton;
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
        HandlePlayerMovement();
        UpdateJumpWindow();
        
    }

    private void UpdateJumpWindow()
    {
        jumpTimeWindow -= Time.deltaTime;
    }


    private void HandlePlayerMovement()
    {
        Vector3 newMoveDir;
        newMoveDir = new Vector3(getPlayerMoveVector().x, getPlayerMoveVector().y, 0);
        if ((newMoveDir - moveDir).sqrMagnitude < 0.01f)
        {
            moveDir = Vector3.Lerp(moveDir, newMoveDir, Time.deltaTime / 0.01f);
        }
        else
        {
            moveDir = newMoveDir;
        }
        Vector3 move = moveDir.x * transform.right + transform.forward * moveDir.y;

        if (PlayerJumped() && canJump())
        {
            Jump();
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        
    }

    private void Jump()
    {
        if (!isGrounded && hasJumpedOnce && canJump())
        {
            Debug.Log("Double Jump");
            playerVelocity.y = 0f;
            playerVelocity.y += Mathf.Sqrt(jumpHeight/2 * -3.0f * gravityValue);
            hasJumpedOnce = true;
            hasDoubleJumped = true;  
        } else if ((isGrounded || jumpTimeWindow > 0) && !hasJumpedOnce)
        {
            Debug.Log("Single Jump");
            playerVelocity.y = 0f;
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            hasDoubleJumped = false;
            hasJumpedOnce = true;
        }
        
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