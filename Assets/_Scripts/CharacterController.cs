using UnityEngine;

public class CharacterController : MonoBehaviour
{
    IA_PlayerControls myControls;
    Rigidbody myRigidBody;
    Vector3 moveInput;

    [Header("Movement Settings")]
    [SerializeField] float movementSpeed;

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
    private void Update()
    {
        moveInput = new Vector3 
            (myControls.Game.Move.ReadValue<Vector2>().x,
            0, 
            myControls.Game.Move.ReadValue<Vector2>().y);
    }
    private void FixedUpdate()
    {
        myRigidBody.AddForce(moveInput * movementSpeed, ForceMode.Acceleration);
    }
}
