using UnityEngine;
using UnityEngine.InputSystem;

// Orbit camera behind the player. Mouse to look; pulls in when a wall is in the way.
public class CavernCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    public float distance = 4.5f;
    public float height = 1.5f;
    public float sensitivity = 0.12f;

    float yaw, pitch = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (target != null) yaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;
        var mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 d = mouse.delta.ReadValue() * sensitivity;
            yaw += d.x;
            pitch = Mathf.Clamp(pitch - d.y, -20f, 70f);
        }

        Vector3 pivot = target.position + Vector3.up * height;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 wanted = pivot - rot * Vector3.forward * distance;
        float dist = distance;
        if (Physics.SphereCast(pivot, 0.2f, (wanted - pivot).normalized, out var hit, distance, ~0, QueryTriggerInteraction.Ignore))
            dist = Mathf.Max(0.5f, hit.distance);
        transform.SetPositionAndRotation(pivot - rot * Vector3.forward * dist, rot);
    }
}
