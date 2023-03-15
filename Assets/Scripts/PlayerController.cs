using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 20f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 50f)] float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 5f)] float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    
    private Vector3 _velocity, _desiredVelocity;
    private bool _desiredJump, _grounded;
    private int _jumpPhase;
    
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
        
        _desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        _desiredJump |= Input.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        UpdateState();
        
        float acceleration = _grounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        
        _velocity.x = Mathf.MoveTowards(_velocity.x, _desiredVelocity.x, maxSpeedChange);
        _velocity.z = Mathf.MoveTowards(_velocity.z, _desiredVelocity.z, maxSpeedChange);
        
        if(_desiredJump) 
        {
            _desiredJump = false;
            Jump();
        }
        
        _rb.velocity = _velocity;

        _grounded = false;
    }
    
    private void OnCollisionEnter(Collision col)
    {
        EvaluateCollision(col);
    }

    private void OnCollisionStay(Collision col)
    {
        
        EvaluateCollision(col);
    }

    private void UpdateState()
    {
        _velocity = _rb.velocity;
        if (_grounded) _jumpPhase = 0;
    }
    
    private void EvaluateCollision(Collision col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            Vector3 normal = col.GetContact(i).normal;
            _grounded |= normal.y >= 0.9f;
        }
    }

    private void Jump()
    {
        if(_grounded || _jumpPhase < maxAirJumps)
        {
            _jumpPhase++;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            if (_velocity.y > 0f) Mathf.Max(jumpSpeed - _velocity.y, 0f);
            _velocity.y += jumpSpeed;
        }
    }
}