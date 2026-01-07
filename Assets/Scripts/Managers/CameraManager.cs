using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = Vector3.up * 0.5f;

    [Header("Orbit")]
    public float distance = 12f;
    public float yaw = 45f;
    public float pitch = 35f;
    public float minPitch = 15f;
    public float maxPitch = 65f;

    [Header("Smoothing")]
    public float followSmoothTime = 0.12f;
    public float rotateSmooth = 12f;

    [Header("Controls")]
    public float rotateSpeed = 120f;
    public float mouseRotateSpeed = 0.15f;

    [Header("Zoom")]
    public float zoomSpeed = 6f;
    public float minDistance = 6f;
    public float maxDistance = 25f;
    
    [Header("Follow Filtering")]
    public bool ignoreTargetY = true;
    public float lockToHeight = 0f;

    Vector3 _followVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();

        Vector3 focusPoint = new Vector3(
            target.position.x,
            lockToHeight,              // fixed height
            target.position.z
        ) + targetOffset;
        Quaternion orbitRot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPos = focusPoint - orbitRot * Vector3.forward * distance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _followVelocity,
            followSmoothTime
        );

        Quaternion lookRot = Quaternion.LookRotation(focusPoint - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRot,
            1f - Mathf.Exp(-rotateSmooth * Time.deltaTime)
        );
    }

    void HandleInput()
    {
        float dt = Time.deltaTime;

        // Keyboard rotation (Q / E)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.isPressed)
                yaw -= rotateSpeed * dt;

            if (Keyboard.current.eKey.isPressed)
                yaw += rotateSpeed * dt;
        }

        // Mouse rotation (Right Mouse Button)
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * mouseRotateSpeed;
            pitch -= delta.y * mouseRotateSpeed;
        }

        // Mouse wheel zoom
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance = Mathf.Clamp(
                    distance - scroll * zoomSpeed * dt,
                    minDistance,
                    maxDistance
                );
            }
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
