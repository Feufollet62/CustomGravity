using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform focus = default;
    [SerializeField, Range(1f, 20f)] private float distance = 5f;
    [SerializeField, Min(0f)] private float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] float focusCentering = 0.5f;
    
    [SerializeField, Range(1f, 360f)] float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)] private float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    
    private Vector2 _orbitAngles = new Vector2(45f, 0f);
    private Vector3 _focusPoint;
    
    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle) maxVerticalAngle = minVerticalAngle;
    }
    
    private void Awake ()
    {
        _focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(_orbitAngles);
    }
    
    private void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation = Quaternion.Euler(_orbitAngles);
        if (ManualRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(_orbitAngles);
        }
        else lookRotation = transform.localRotation;

        Vector3 lookDirection = transform.forward;
        Vector3 lookPosition = _focusPoint - lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
    
    private void UpdateFocusPoint ()
    {
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, _focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f) t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            if (distance > focusRadius) t = Mathf.Min(t, focusRadius / distance);
            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }
        else _focusPoint = targetPoint;
    }
    
    private bool ManualRotation()
    {
        Vector2 input = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            _orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            return true;
        }

        return false;
    }
    
    private void ConstrainAngles()
    {
        _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (_orbitAngles.y < 0f) _orbitAngles.y += 360f;
        else if (_orbitAngles.y >= 360f) _orbitAngles.y -= 360f;
    }
}
