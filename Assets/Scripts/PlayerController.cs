using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 20f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 50f)] float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 40f;
    
    [SerializeField, Range(0f, 5f)] float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    
    private Vector3 _velocity, _desiredVelocity;
    private float _minGroundDotProduct;

    private Vector3 _contactNormal;
    private bool _desiredJump, _grounded;
    private int _jumpPhase;
    
    private Rigidbody _rb;

    private void OnValidate()
    {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }
    
    private void Awake() 
    {
        _rb = GetComponent<Rigidbody>();
        OnValidate();
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
        AdjustVelocity();
        
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
        else _contactNormal = Vector3.up;
    }
    
    private void EvaluateCollision(Collision col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            Vector3 normal = col.GetContact(i).normal;
            
            if (normal.y >= _minGroundDotProduct)
            {
                _grounded = true;
                _contactNormal = normal;
            }
        }
    }

    private void Jump()
    {
        if(_grounded || _jumpPhase < maxAirJumps)
        {
            _jumpPhase++;
            
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(_velocity, _contactNormal);
            if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);

            _velocity += _contactNormal * jumpSpeed;
        }
    }
    
    private void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        
        float currentX = Vector3.Dot(_velocity, xAxis);
        float currentZ = Vector3.Dot(_velocity, zAxis);
        
        float acceleration = _grounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
        
        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    
    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }
}