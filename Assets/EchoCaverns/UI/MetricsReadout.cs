using UnityEngine;
using UnityEngine.UIElements;

// Metrics gym readout: walk speed over the 20 m track, last and best jump (height and distance).
// Listens to CavernPlayer.Landed and the two TrackGate triggers.
[RequireComponent(typeof(UIDocument))]
public class MetricsReadout : MonoBehaviour
{
    [SerializeField] CavernPlayer player;
    public float trackLength = 20f; // 2000 cm

    Label speedLabel, lastJumpLabel, bestJumpLabel;
    float trackStartTime = -1f;
    float bestHeight, bestDistance;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        speedLabel = root.Q<Label>("speed");
        lastJumpLabel = root.Q<Label>("last-jump");
        bestJumpLabel = root.Q<Label>("best-jump");
        player.Landed += OnLanded;
    }

    void OnDestroy()
    {
        if (player != null) player.Landed -= OnLanded;
    }

    public void TrackStart()
    {
        trackStartTime = Time.time;
        speedLabel.text = "Walk speed: timing...";
    }

    public void TrackEnd()
    {
        if (trackStartTime < 0f) return;
        float seconds = Time.time - trackStartTime;
        trackStartTime = -1f;
        speedLabel.text = $"Walk speed: {trackLength * 100f / seconds:0} cm/s  ({seconds:0.0} s for 2000 cm)";
    }

    void OnLanded(float distance, float height)
    {
        if (distance < 0.2f && height < 0.05f) return;
        bestHeight = Mathf.Max(bestHeight, height);
        bestDistance = Mathf.Max(bestDistance, distance);
        lastJumpLabel.text = $"Last jump: {distance * 100f:0} cm far, {height * 100f:0} cm high";
        bestJumpLabel.text = $"Best: {bestDistance * 100f:0} cm far, {bestHeight * 100f:0} cm high";
    }
}
