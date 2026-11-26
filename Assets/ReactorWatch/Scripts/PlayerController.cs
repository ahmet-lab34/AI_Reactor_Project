using System;
using UnityEngine;
using UnityEngine.InputSystem;

// First-person walk + mouse look. E opens the nearest READY valve within range.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private ReactorGame game;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Valve[] valves;

    public event Action<string> PromptChanged;   // empty text = hide the prompt

    private CharacterController body;
    private float pitch;
    private float fallSpeed;
    private string lastPrompt;

    private void Awake()
    {
        body = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!game.IsPlaying)
        {
            SetPrompt("");
            return;
        }

        ReactorConfig cfg = game.Config;
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (mouse != null)
        {
            Vector2 look = mouse.delta.ReadValue() * cfg.lookSensitivity;
            transform.Rotate(0f, look.x, 0f);
            pitch = Mathf.Clamp(pitch - look.y, -80f, 80f);
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        Vector2 move = Vector2.zero;
        if (kb != null)
        {
            if (kb.wKey.isPressed) move.y += 1f;
            if (kb.sKey.isPressed) move.y -= 1f;
            if (kb.dKey.isPressed) move.x += 1f;
            if (kb.aKey.isPressed) move.x -= 1f;
        }
        move = Vector2.ClampMagnitude(move, 1f);

        fallSpeed = body.isGrounded ? -1f : fallSpeed - 9.81f * Time.deltaTime;
        Vector3 velocity = (transform.right * move.x + transform.forward * move.y) * cfg.moveSpeed;
        velocity.y = fallSpeed;
        body.Move(velocity * Time.deltaTime);

        Valve near = NearestValve(cfg.valveRange);
        if (near == null)
        {
            SetPrompt("");
            return;
        }

        if (near.State == ValveState.Ready)
        {
            SetPrompt("[E] Open valve");
            if (kb != null && kb.eKey.wasPressedThisFrame) near.TryOpen();
        }
        else if (near.State == ValveState.Cooldown)
        {
            SetPrompt($"Valve {near.Id} cooling down: {near.WholeSecondsLeft}s");
        }
        else
        {
            SetPrompt($"Valve {near.Id} open: {near.WholeSecondsLeft}s");
        }
    }

    private Valve NearestValve(float range)
    {
        Valve best = null;
        float bestDistance = range;
        Vector3 me = transform.position;
        foreach (Valve v in valves)
        {
            if (v == null) continue;
            Vector3 d = v.transform.position - me;
            d.y = 0f;
            float distance = d.magnitude;
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = v;
            }
        }
        return best;
    }

    private void SetPrompt(string text)
    {
        if (text == lastPrompt) return;
        lastPrompt = text;
        PromptChanged?.Invoke(text);
    }
}
