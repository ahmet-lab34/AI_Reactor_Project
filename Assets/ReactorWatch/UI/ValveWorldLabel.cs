using UnityEngine;

// Label above a valve: its letter and READY / OPEN 5s / COOLDOWN 8s (ui-hud-kit rule 4).
// It only listens to ReactorGame.ValveChanged.
public class ValveWorldLabel : MonoBehaviour
{
    [SerializeField] private ReactorGame game;
    [SerializeField] private Valve valve;
    [SerializeField] private TextMesh statusText;

    private static readonly Color ReadyColor = new Color32(110, 210, 125, 255);
    private static readonly Color OpenColor = new Color32(110, 175, 255, 255);
    private static readonly Color CooldownColor = new Color32(150, 150, 150, 140);

    private void OnEnable()
    {
        game.ValveChanged += OnValveChanged;
        OnValveChanged(valve);
    }

    private void OnDisable()
    {
        if (game != null) game.ValveChanged -= OnValveChanged;
    }

    private void OnValveChanged(Valve v)
    {
        if (v != valve) return;
        statusText.text = ReactorWatchHUD.ValveText(v);
        statusText.color = v.State == ValveState.Ready ? ReadyColor
                         : v.State == ValveState.Open ? OpenColor
                         : CooldownColor;
    }
}
