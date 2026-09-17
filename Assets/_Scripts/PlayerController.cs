using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    IA_PlayerControls myControls;
    Rigidbody myRigidBody;
    MeshRenderer[] myMeshRenderer;
    Vector3 moveInput;
    float originalHeightScale;
    EnemyStateBehavior enemyStateScript;

    [Header("Movement Settings")]
    [SerializeField] float movementSpeed;
    [SerializeField] float sneakingSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float sneakHeightMultiplier;

    [Header("State Flags")]
    [SerializeField] bool isSneaking;

    [Header("State Materials")]
    [SerializeField] Material baseMat;
    [SerializeField] Material sneakMat;

    private void Awake()
    {
        originalHeightScale = transform.localScale.y;
        if (myMeshRenderer == null) 
            myMeshRenderer = GetComponentsInChildren<MeshRenderer>();
        if (myControls == null)
            myControls = new IA_PlayerControls();
        if (myRigidBody == null)
            myRigidBody = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        myControls.Enable();
        myControls.Game.Sneak.performed += OnSneak;
    }
    private void OnDisable()
    {
        myControls.Game.Sneak.performed -= OnSneak;
        myControls.Disable();
    }
    private void FixedUpdate()
    {
        Debug.Log(moveInput);
        HandleInput();
        HandleMovement();
        HandleRotation();
        if (enemyStateScript != null)
            EnemySneakCheck();
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
        if (!isSneaking)
            myRigidBody.AddForce(moveInput * movementSpeed, ForceMode.Acceleration);
        else
            myRigidBody.AddForce(moveInput * sneakingSpeed, ForceMode.Acceleration);
    }
    private void HandleRotation()
    {
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            Debug.Log(targetRotation + " and " + transform.rotation);
        }
    }
    private void OnSneak(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (isSneaking)
            {
                isSneaking = false;
                transform.localScale = new Vector3(transform.localScale.x, originalHeightScale, transform.localScale.z);
                transform.position += Vector3.up * sneakHeightMultiplier;
                foreach (MeshRenderer m in myMeshRenderer)
                    m.material = baseMat;
            }
            else
            {
                isSneaking = true;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * sneakHeightMultiplier, transform.localScale.z);
                transform.position -= Vector3.up * sneakHeightMultiplier;
                foreach (MeshRenderer m in myMeshRenderer)
                    m.material = sneakMat;
            }
        }
    }
    private void EnemySneakCheck()
    {
        if (!isSneaking && moveInput.magnitude > 0)
            enemyStateScript.Alerted(transform.position);
    }    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + other.name);
        if (other.gameObject.tag == "Enemy")
        {
            enemyStateScript = other.gameObject.GetComponent<EnemyStateBehavior>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        enemyStateScript = null;
    }
}