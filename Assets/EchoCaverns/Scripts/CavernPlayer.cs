using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Third-person walker: WASD (camera-relative), Shift run, Space jump, C or Ctrl crouch.
// Numbers match level-blockout/reference.md: walk 2 m/s, run 5.3 m/s, jump 1.2 m, step 0.25 m, slope 45.
[RequireComponent(typeof(CharacterController))]
public class CavernPlayer : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    public float walkSpeed = 2f;
    public float runSpeed = 5.3f;
    public float crouchSpeed = 1.2f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f;
    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;

    public bool InputEnabled { get; set; } = true;
    public bool IsCrouching { get; private set; }

    // For the metrics gym: fired on landing with horizontal distance and peak height gained (metres).
    public event Action<float, float> Landed;

    // Point the searchlight looks at: high when standing, low when crouching.
    public Vector3 HeadPoint => transform.position + Vector3.up * (controller.height * 0.7f);

    CharacterController controller;
    float verticalSpeed;
    bool inAir;
    Vector3 jumpStart;
    float peakY;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        Vector2 input = Vector2.zero;
        bool run = false, jump = false, crouch = false;
        if (InputEnabled && kb != null)
        {
            if (kb.wKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed) input.y -= 1f;
            if (kb.dKey.isPressed) input.x += 1f;
            if (kb.aKey.isPressed) input.x -= 1f;
            run = kb.leftShiftKey.isPressed;
            jump = kb.spaceKey.wasPressedThisFrame;
            crouch = kb.cKey.isPressed || kb.leftCtrlKey.isPressed;
        }

        SetCrouch(crouch);

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = new Vector3(forward.z, 0f, -forward.x);
        Vector3 move = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        float speed = IsCrouching ? crouchSpeed : run ? runSpeed : walkSpeed;

        if (controller.isGrounded)
        {
            if (inAir)
            {
                inAir = false;
                Vector3 d = transform.position - jumpStart;
                d.y = 0f;
                Landed?.Invoke(d.magnitude, peakY - jumpStart.y);
            }
            verticalSpeed = -2f;
            if (jump && !IsCrouching)
            {
                verticalSpeed = Mathf.Sqrt(-2f * gravity * jumpHeight);
                inAir = true;
                jumpStart = transform.position;
                peakY = transform.position.y;
            }
        }
        verticalSpeed += gravity * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalSpeed;
        Vector3 platformDelta = Vector3.zero;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 0.3f, ~0, QueryTriggerInteraction.Ignore))
        {
            var lift = hit.collider.GetComponentInParent<Lift>();
            if (lift != null) platformDelta = lift.Delta;
        }
        controller.Move(velocity * Time.deltaTime + platformDelta);

        if (inAir) peakY = Mathf.Max(peakY, transform.position.y);
        if (move.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 12f * Time.deltaTime);
    }

    void SetCrouch(bool wantCrouch)
    {
        if (wantCrouch == IsCrouching) return;
        if (!wantCrouch && Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, standHeight - 0.4f, ~0, QueryTriggerInteraction.Ignore))
            return; // something above: stay crouched
        IsCrouching = wantCrouch;
        controller.height = wantCrouch ? crouchHeight : standHeight;
        controller.center = Vector3.up * (controller.height * 0.5f);
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = true;
        verticalSpeed = 0f;
        inAir = false;
    }
}
