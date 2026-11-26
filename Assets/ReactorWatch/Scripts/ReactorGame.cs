using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Runs the heat, the countdown, pause and the end of the game.
// The HUD only listens to these events. They fire only when the shown value changes.
public class ReactorGame : MonoBehaviour
{
    private const float HeatStep = 0.05f;

    [SerializeField] private ReactorConfig config;
    [SerializeField] private Valve[] valves;

    public ReactorConfig Config => config;
    public float Heat { get; private set; }
    public float Elapsed { get; private set; }
    public int WholeSecondsLeft => Mathf.CeilToInt(Mathf.Max(0f, config.surviveSeconds - Elapsed));
    public bool IsOver { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsPlaying => !IsOver && !IsPaused;
    public Valve[] Valves => valves;

    public event Action<float> HeatChanged;          // heat
    public event Action<int> TimeLeftChanged;        // whole seconds left
    public event Action<Valve> ValveChanged;         // state or shown second changed
    public event Action<bool, float> GameEnded;      // won, seconds survived
    public event Action<bool> PauseChanged;          // paused

    private float sentHeat = -1f;
    private int sentSeconds = -1;

    private void Awake()
    {
        Time.timeScale = 1f;
        Heat = config.startHeat;
        foreach (Valve v in valves)
            if (v != null) v.Changed += RelayValve;
    }

    private void OnDestroy()
    {
        foreach (Valve v in valves)
            if (v != null) v.Changed -= RelayValve;
    }

    private void Start()
    {
        SetCursorFree(false);
        SendHeat(true);
        SendTime(true);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (!IsOver && kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused) Resume(); else Pause();
        }

        if (!IsPlaying) return;

        float dt = Time.deltaTime;
        Elapsed += dt;

        float rise = Elapsed >= config.lateAfterSeconds ? config.lateHeatPerSecond : config.heatPerSecond;
        int openValves = 0;
        foreach (Valve v in valves)
            if (v != null && v.State == ValveState.Open) openValves++;

        Heat = Mathf.Clamp(Heat + (rise - openValves * config.valveCoolPerSecond) * dt, 0f, config.meltdownHeat);
        SendHeat(false);
        SendTime(false);

        if (Heat >= config.meltdownHeat) EndGame(false);
        else if (Elapsed >= config.surviveSeconds) EndGame(true);
    }

    public void Pause()
    {
        if (IsOver || IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        SetCursorFree(true);
        PauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        SetCursorFree(false);
        PauseChanged?.Invoke(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SendHeat(bool force)
    {
        if (!force && Mathf.Abs(Heat - sentHeat) < HeatStep && Heat < config.meltdownHeat) return;
        sentHeat = Heat;
        HeatChanged?.Invoke(Heat);
    }

    private void SendTime(bool force)
    {
        int seconds = WholeSecondsLeft;
        if (!force && seconds == sentSeconds) return;
        sentSeconds = seconds;
        TimeLeftChanged?.Invoke(seconds);
    }

    private void RelayValve(Valve v) => ValveChanged?.Invoke(v);

    private void EndGame(bool won)
    {
        IsOver = true;
        Time.timeScale = 0f;
        SetCursorFree(true);
        SendHeat(true);
        SendTime(true);
        GameEnded?.Invoke(won, Mathf.Min(Elapsed, config.surviveSeconds));
    }

    private static void SetCursorFree(bool free)
    {
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }
}
