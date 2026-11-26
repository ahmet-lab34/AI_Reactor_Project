using System;
using UnityEngine;

public enum ValveState { Ready, Open, Cooldown }

// One valve on a wall. READY -> OPEN (cools the core) -> COOLDOWN -> READY.
public class Valve : MonoBehaviour
{
    [SerializeField] private string valveId = "A";
    [SerializeField] private ReactorConfig config;
    [SerializeField] private Renderer valveRenderer;

    public string Id => valveId;
    public ValveState State { get; private set; } = ValveState.Ready;
    public float SecondsLeft { get; private set; }
    public int WholeSecondsLeft => Mathf.CeilToInt(SecondsLeft);

    // Fires when the state changes and every time the shown whole second changes.
    public event Action<Valve> Changed;

    private int lastWholeSecond = -1;

    private void Start()
    {
        ApplyColor();
        Changed?.Invoke(this);
    }

    public bool TryOpen()
    {
        if (State != ValveState.Ready) return false;
        SetState(ValveState.Open, config.valveOpenSeconds);
        return true;
    }

    private void Update()
    {
        if (State == ValveState.Ready) return;

        SecondsLeft -= Time.deltaTime;
        if (SecondsLeft <= 0f)
        {
            if (State == ValveState.Open) SetState(ValveState.Cooldown, config.valveCooldownSeconds);
            else SetState(ValveState.Ready, 0f);
            return;
        }

        int whole = WholeSecondsLeft;
        if (whole != lastWholeSecond)
        {
            lastWholeSecond = whole;
            Changed?.Invoke(this);
        }
    }

    private void SetState(ValveState state, float seconds)
    {
        State = state;
        SecondsLeft = seconds;
        lastWholeSecond = WholeSecondsLeft;
        ApplyColor();
        Changed?.Invoke(this);
    }

    private void ApplyColor()
    {
        if (valveRenderer == null) return;
        Color c = State == ValveState.Ready ? new Color(0.2f, 0.8f, 0.35f)
                : State == ValveState.Open ? new Color(0.2f, 0.55f, 1f)
                : new Color(0.35f, 0.35f, 0.35f);
        valveRenderer.material.SetColor("_BaseColor", c);
    }
}
