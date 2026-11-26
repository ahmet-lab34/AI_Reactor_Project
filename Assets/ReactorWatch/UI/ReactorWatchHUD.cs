using UnityEngine;
using UnityEngine.UIElements;

// Screen HUD. It only listens to ReactorGame and PlayerController events (ui-hud-kit rule 5)
// and only switches USS classes (rule 7). No Update loop.
public class ReactorWatchHUD : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private ReactorGame game;
    [SerializeField] private PlayerController player;
    [SerializeField] private VisualTreeAsset pauseMenuAsset;
    [SerializeField] private VisualTreeAsset endScreenAsset;

    private VisualElement heatPanel, heatFill, banner, pauseMenu, endScreen;
    private Label heatValue, countdown, prompt, endTitle, endTime;
    private IVisualElementScheduledItem blinkJob;
    private bool blinking;

    private void Start()
    {
        VisualElement root = document.rootVisualElement;

        pauseMenu = AddOverlay(root, pauseMenuAsset);
        endScreen = AddOverlay(root, endScreenAsset);

        heatPanel = root.Q("heat-panel");
        heatFill = root.Q("heat-fill");
        heatValue = root.Q<Label>("heat-value");
        countdown = root.Q<Label>("countdown");
        prompt = root.Q<Label>("prompt");
        banner = root.Q("banner");
        endTitle = root.Q<Label>("end-title");
        endTime = root.Q<Label>("end-time");

        root.Q<Button>("pause-resume").clicked += game.Resume;
        root.Q<Button>("pause-restart").clicked += game.Restart;
        root.Q<Button>("pause-quit").clicked += game.Quit;
        root.Q<Button>("end-restart").clicked += game.Restart;

        long blinkMs = (long)(1000f / (game.Config.blinksPerSecond * 2f));   // 4 per second = 125 ms
        blinkJob = heatPanel.schedule.Execute(() => heatPanel.ToggleInClassList("blink-off")).Every(blinkMs);
        blinkJob.Pause();

        game.HeatChanged += OnHeatChanged;
        game.TimeLeftChanged += OnTimeLeftChanged;
        game.ValveChanged += OnValveChanged;
        game.GameEnded += OnGameEnded;
        game.PauseChanged += OnPauseChanged;
        player.PromptChanged += OnPromptChanged;

        // First draw from the current values; after that, events only.
        OnHeatChanged(game.Heat);
        OnTimeLeftChanged(game.WholeSecondsLeft);
        OnPromptChanged("");
        foreach (Valve v in game.Valves) OnValveChanged(v);
    }

    private void OnDestroy()
    {
        if (game != null)
        {
            game.HeatChanged -= OnHeatChanged;
            game.TimeLeftChanged -= OnTimeLeftChanged;
            game.ValveChanged -= OnValveChanged;
            game.GameEnded -= OnGameEnded;
            game.PauseChanged -= OnPauseChanged;
        }
        if (player != null) player.PromptChanged -= OnPromptChanged;
    }

    private static VisualElement AddOverlay(VisualElement root, VisualTreeAsset asset)
    {
        TemplateContainer overlay = asset.Instantiate();
        overlay.AddToClassList("overlay-holder");
        overlay.AddToClassList("hidden");
        root.Add(overlay);
        return overlay;
    }

    private void OnHeatChanged(float heat)
    {
        ReactorConfig cfg = game.Config;
        heatFill.style.width = Length.Percent(Mathf.Clamp01(heat / cfg.meltdownHeat) * 100f);
        heatValue.text = Mathf.FloorToInt(heat).ToString();

        heatFill.EnableInClassList("heat--green", heat < cfg.yellowFromHeat);
        heatFill.EnableInClassList("heat--yellow", heat >= cfg.yellowFromHeat && heat < cfg.redFromHeat);
        heatFill.EnableInClassList("heat--red", heat >= cfg.redFromHeat);

        bool shouldBlink = heat > cfg.blinkAboveHeat && !game.IsOver;
        if (shouldBlink != blinking)
        {
            blinking = shouldBlink;
            if (blinking) blinkJob.Resume();
            else
            {
                blinkJob.Pause();
                heatPanel.RemoveFromClassList("blink-off");
            }
        }

        banner.EnableInClassList("banner--shown", heat >= cfg.criticalHeat);
    }

    private void OnTimeLeftChanged(int secondsLeft)
    {
        countdown.text = FormatTime(secondsLeft);
    }

    private void OnValveChanged(Valve v)
    {
        VisualElement row = document.rootVisualElement.Q("valve-" + v.Id);
        Label state = document.rootVisualElement.Q<Label>("valve-" + v.Id + "-state");
        if (row == null || state == null) return;

        row.EnableInClassList("valve--open", v.State == ValveState.Open);
        row.EnableInClassList("valve--cooldown", v.State == ValveState.Cooldown);
        state.text = ValveText(v);
    }

    private void OnPromptChanged(string text)
    {
        prompt.text = text;
        prompt.EnableInClassList("hidden", string.IsNullOrEmpty(text));
    }

    private void OnPauseChanged(bool paused)
    {
        pauseMenu.EnableInClassList("hidden", !paused);
    }

    private void OnGameEnded(bool won, float survived)
    {
        blinkJob.Pause();
        heatPanel.RemoveFromClassList("blink-off");
        pauseMenu.AddToClassList("hidden");
        prompt.AddToClassList("hidden");
        endTitle.text = won ? "REACTOR STABLE" : "MELTDOWN";
        endTime.text = "Time survived " + FormatTime(Mathf.FloorToInt(survived));
        endScreen.RemoveFromClassList("hidden");
    }

    public static string ValveText(Valve v)
    {
        return v.State == ValveState.Ready ? "READY"
             : v.State == ValveState.Open ? $"OPEN {v.WholeSecondsLeft}s"
             : $"COOLDOWN {v.WholeSecondsLeft}s";
    }

    private static string FormatTime(int seconds)
    {
        return $"{seconds / 60:00}:{seconds % 60:00}";
    }
}
