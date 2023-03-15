using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 20f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 50f)] float maxAcceleration = 10f;
    
    private Vector3 _velocity;
    private Rigidbody _rb;

    void Awake() 
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        _velocity = _rb.velocity;
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        
        _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity.x, maxSpeedChange);
        _velocity.z = Mathf.MoveTowards(_velocity.z, desiredVelocity.z, maxSpeedChange);
        
        _rb.velocity = _velocity;
    }
}