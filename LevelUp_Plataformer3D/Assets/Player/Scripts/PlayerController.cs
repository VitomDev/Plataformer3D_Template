using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float orientationSpeed = 360f;
    [SerializeField] private float jumpSpeed = 7.5f;
    
    [Header("Inputs")]
    [SerializeField] private InputAction movement;
    [SerializeField] private InputAction jump;

    private Camera mainCamera;
    private CharacterController characterController;

    private float verticalVelocity = 0f;
    [Header("Data")]
    [SerializeField] private float gravity = -9.8f; // m/s2

    private Animator animator;
    

    private void OnEnable()
    {
        movement.Enable();
        jump.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        jump.Disable();
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

    }

    void Start()
    {
    }
    
    void Update()
    {
        Vector2 movementValue;
        Vector3 movementValueXZ;
        bool isWalking;
        
        UpdateVerticalMovement();
        UpdateMovement(out movementValue, out movementValueXZ, out isWalking);
        UpdateOrientation(movementValue, movementValueXZ);

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsGrounded", characterController.isGrounded);
        animator.SetFloat("NormalizedVerticalVelocity", Mathf.Clamp01(Mathf.InverseLerp(jumpSpeed, -jumpSpeed, verticalVelocity)));
    }
    
    private void UpdateVerticalMovement()
    {
        //Update jump
        if ((jump.ReadValue<float>() > 0f) && (characterController.isGrounded))
        {
            verticalVelocity = jumpSpeed;
        }
        else
        {
            //Update gravity
            if (characterController.isGrounded) {verticalVelocity = 0f;}
            
            // m/s                  += m/(s*s)  *       s       ;
            verticalVelocity += gravity * Time.deltaTime;
            
        }
    }
    
    private void UpdateMovement(out Vector2 movementValue, out Vector3 movementValueXZ, out bool isWalking)
    {
        //Detectar input
        movementValue = movement.ReadValue<Vector2>();
        movementValueXZ = new Vector3(movementValue.x, 0f, movementValue.y);
        
        //Mover jugador respecto camara
        float oldMagnitudeXZ = movementValueXZ.magnitude;
        movementValueXZ = mainCamera.transform.TransformDirection(movementValueXZ);
        movementValueXZ = Vector3.ProjectOnPlane(movementValueXZ, Vector3.up);
        movementValueXZ = movementValueXZ.normalized * oldMagnitudeXZ;
        
        Vector3 velocityXZ = movementValueXZ * movementSpeed;
        Vector3 velocityY = (Vector3.up * verticalVelocity);
        Vector3 velocity = velocityXZ + velocityY;
        
        //Mover jugador con Input System
        characterController.Move(velocity * Time.deltaTime);

        isWalking = movementValueXZ.magnitude > 0f;
    }
    
    private void UpdateOrientation(Vector2 movementValue, Vector3 movementValueXZ)
    {
        //Orientar jugador respecto camara
        if (movementValue.magnitude > 0.01f)
        {
            //Usamos el mainCamera si queremos que el jugador se oriente siempre mirando hacia enfrente
            //Vector3 desiredForward = mainCamera.transform.forward;
            Vector3 desiredForward = movementValueXZ;
            desiredForward = Vector3.ProjectOnPlane(desiredForward, Vector3.up).normalized;
            Vector3 currentForward = transform.forward;
            float angleDifference = Vector3.SignedAngle(currentForward, desiredForward, Vector3.up);
            float angleToApply = Math.Min(Mathf.Abs(angleDifference), orientationSpeed * Time.deltaTime);
            angleToApply *= Mathf.Sign(angleDifference);
        
            Quaternion rotationToApply = Quaternion.AngleAxis(angleToApply, Vector3.up);
            transform.rotation = rotationToApply * transform.rotation;
        }
    }
}
