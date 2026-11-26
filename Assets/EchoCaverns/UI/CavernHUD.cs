using UnityEngine;
using UnityEngine.UIElements;

// Echo Caverns HUD: run timer, "Secret 0/1", short messages, ESCAPED screen. Only listens to CavernGame events.
[RequireComponent(typeof(UIDocument))]
public class CavernHUD : MonoBehaviour
{
    [SerializeField] CavernGame game;

    Label timer, secrets, message, escapedText;
    VisualElement escapedRoot;
    IVisualElementScheduledItem hideMessage;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        timer = root.Q<Label>("timer");
        secrets = root.Q<Label>("secrets");
        message = root.Q<Label>("message");
        escapedRoot = root.Q("escaped-root");
        escapedText = root.Q<Label>("escaped-text");
        hideMessage = root.schedule.Execute(() => message.AddToClassList("hidden"));
        hideMessage.Pause();

        game.TimeChanged += OnTime;
        game.SecretsChanged += OnSecrets;
        game.Message += OnMessage;
        game.Escaped += OnEscaped;
    }

    void OnDestroy()
    {
        if (game == null) return;
        game.TimeChanged -= OnTime;
        game.SecretsChanged -= OnSecrets;
        game.Message -= OnMessage;
        game.Escaped -= OnEscaped;
    }

    void OnTime(float t) => timer.text = Format(t);

    void OnSecrets(int found, int total) => secrets.text = $"Secret {found}/{total}";

    void OnMessage(string text)
    {
        message.text = text;
        message.RemoveFromClassList("hidden");
        hideMessage.ExecuteLater(1500);
    }

    void OnEscaped(float t, int found, int total)
    {
        escapedText.text = $"Time {Format(t)}   ·   Secrets {found}/{total}";
        escapedRoot.RemoveFromClassList("hidden");
    }

    static string Format(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        float s = t - m * 60f;
        return $"{m:00}:{s:00.0}";
    }
}
