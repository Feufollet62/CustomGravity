using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform focus = default;
    [SerializeField, Range(1f, 20f)] private float distance = 5f;
    [SerializeField, Min(0f)] private float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] float focusCentering = 0.5f;
    
    
    private Vector3 _focusPoint;
    
    private void Awake ()
    {
        _focusPoint = focus.position;
    }
    
    private void LateUpdate()
    {
        UpdateFocusPoint();
        Vector3 lookDirection = transform.forward;
        transform.localPosition = _focusPoint - lookDirection * distance;
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
}
