using UnityEngine;
using UnityEngine.Animations;

public class CharacterController : MonoBehaviour
{
    IA_PlayerControls myControls;
    Rigidbody myRigidBody;
    Vector3 moveInput;

    [Header("Movement Settings")]
    [SerializeField] float movementSpeed;
    [SerializeField] float rotateSmoothing;

    private void Awake()
    {
        if (myControls == null)
            myControls = new IA_PlayerControls();
        if (myRigidBody == null)
            myRigidBody = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        myControls.Enable();
    }
    private void OnDisable()
    {
        myControls.Disable();
    }
    private void FixedUpdate()
    {
        HandleInput();
        HandleMovement();
        HandleRotation();
    }
    private void HandleInput()
    {
        moveInput = new Vector3
            (myControls.Game.Move.ReadValue<Vector2>().x,
            0,
            myControls.Game.Move.ReadValue<Vector2>().y);
    }
    private void HandleMovement()
    {
        myRigidBody.AddForce(moveInput * movementSpeed, ForceMode.Acceleration);
    }
    private void HandleRotation()
    {
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSmoothing * Time.deltaTime);
        }
        //Vector3 playerDirection = Vector3.right * moveInput.x + Vector3.forward * moveInput.y;
        //if (playerDirection.sqrMagnitude > 0)
        //{
        //    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
        //    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, rotateSmoothing * Time.deltaTime);
        //}
    }
}
