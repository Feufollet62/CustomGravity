using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 20f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 50f)] float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 40f, maxStairsAngle = 60f;
    
    [SerializeField] LayerMask snapMask = -1, stairsMask = -1;
    [SerializeField, Min(0f)] float maxSnapDistance = 1f;
    [SerializeField, Range(0f, 100f)] float maxSnapSpeed = 100f;
    
    [SerializeField, Range(0f, 5f)] float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    
    private Vector3 _velocity, _desiredVelocity;
    
    private float _minGroundDotProduct, _minStairsDotProduct;
    private Vector3 _contactNormal, _steepNormal;
    private int _stepsSinceLastGrounded, _stepsSinceLastJump;
    private int _groundContactCount, _steepContactCount;
    
    private bool _desiredJump;
    private int _jumpPhase;

    private Rigidbody _rb;

    private bool Grounded => _groundContactCount > 0;
    private bool OnSteep => _steepContactCount > 0;
    
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

        ClearState();
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
        _stepsSinceLastGrounded++;
        _stepsSinceLastJump++;
        
        _velocity = _rb.velocity;
        if (Grounded || SnapToGround() || CheckSteepContacts())
        {
            _stepsSinceLastGrounded = 0;
            if (_stepsSinceLastJump > 1) _jumpPhase = 0;
            if (_groundContactCount > 1) _contactNormal.Normalize();
        }
        else _contactNormal = Vector3.up;
    }
    
    private void EvaluateCollision(Collision col)
    {
        float minDot = GetMinDot(col.gameObject.layer);
        for (int i = 0; i < col.contactCount; i++)
        {
            Vector3 normal = col.GetContact(i).normal;
            
            if (normal.y >= minDot)
            {
                _groundContactCount++;
                _contactNormal += normal;
            }
            else if (normal.y > -0.01f)
            {
                _steepContactCount += 1;
                _steepNormal += normal;
            }
        }
    }

    private void Jump()
    {
        Vector3 jumpDirection;
        if (Grounded) jumpDirection = _contactNormal;
        else if (OnSteep)
        {
            jumpDirection = _steepNormal;
            _jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps)
        {
            if (_jumpPhase == 0) _jumpPhase = 1;
            jumpDirection = _contactNormal;
        }
        else return;
        
        _stepsSinceLastJump = 0;
        _jumpPhase++;
        
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
        if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);

        _velocity += jumpDirection * jumpSpeed;
    }
    
    private void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        
        float currentX = Vector3.Dot(_velocity, xAxis);
        float currentZ = Vector3.Dot(_velocity, zAxis);
        
        float acceleration = Grounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
        
        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    
    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }

    private bool SnapToGround()
    {
        if(_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2) return false;
        
        float speed = _velocity.magnitude;
        if (speed > maxSnapSpeed) return false;
        
        if(!Physics.Raycast(_rb.position, Vector3.down, out RaycastHit hit, maxSnapDistance, snapMask)) return false;
        
        if(hit.normal.y < GetMinDot(hit.collider.gameObject.layer)) return false;
        
        _groundContactCount = 1;
        _contactNormal = hit.normal;
        float dot = Vector3.Dot(_velocity, hit.normal);
        if (dot > 0f) _velocity = (_velocity - hit.normal * dot).normalized * speed;
        
        return true;
    }
    
    private float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
    }
    
    bool CheckSteepContacts()
    {
        if (_steepContactCount > 1)
        {
            _steepNormal.Normalize();
            if (_steepNormal.y >= _minGroundDotProduct)
            {
                _groundContactCount = 1;
                _contactNormal = _steepNormal;
                return true;
            }
        }
        return false;
    }
    
    private void ClearState()
    {
        _groundContactCount = _steepContactCount = 0;
        _contactNormal = Vector3.zero;
    }
}