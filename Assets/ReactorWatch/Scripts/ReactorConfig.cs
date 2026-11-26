using UnityEngine;

// Every Reactor Watch number lives here. The asset is DA_Reactor.
[CreateAssetMenu(menuName = "Reactor Watch/Reactor Config", fileName = "DA_Reactor")]
public class ReactorConfig : ScriptableObject
{
    [Header("Heat")]
    public float startHeat = 40f;
    public float heatPerSecond = 2.5f;
    public float lateHeatPerSecond = 3.5f;
    public float lateAfterSeconds = 60f;
    public float meltdownHeat = 100f;

    [Header("Win")]
    public float surviveSeconds = 120f;

    [Header("Valves")]
    public float valveRange = 2f;            // 200 cm
    public float valveOpenSeconds = 6f;
    public float valveCoolPerSecond = 5f;
    public float valveCooldownSeconds = 10f;

    [Header("HUD thresholds")]
    public float yellowFromHeat = 60f;
    public float redFromHeat = 85f;
    public float criticalHeat = 85f;
    public float blinkAboveHeat = 90f;
    public float blinksPerSecond = 4f;
    public float bannerSlideSeconds = 0.3f;

    [Header("Player")]
    public float moveSpeed = 4f;
    public float lookSensitivity = 0.1f;
}
