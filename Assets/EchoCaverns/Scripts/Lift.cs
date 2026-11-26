using UnityEngine;

// Rises riseHeight in riseSeconds, waits waitSeconds at the top, comes back down, waits at the bottom.
// Runs before the player so the player can ride it (CavernPlayer adds Delta).
[DefaultExecutionOrder(-50)]
public class Lift : MonoBehaviour
{
    public float riseHeight = 4f;     // 400 cm
    public float riseSeconds = 3f;
    public float waitSeconds = 2f;

    public Vector3 Delta { get; private set; }

    Vector3 bottom;
    float t;

    void Start()
    {
        bottom = transform.position;
    }

    void Update()
    {
        float cycle = 2f * (riseSeconds + waitSeconds);
        t = (t + Time.deltaTime) % cycle;

        float k;
        if (t < riseSeconds) k = t / riseSeconds;                                         // going up
        else if (t < riseSeconds + waitSeconds) k = 1f;                                   // wait at top
        else if (t < 2f * riseSeconds + waitSeconds) k = 1f - (t - riseSeconds - waitSeconds) / riseSeconds; // down
        else k = 0f;                                                                      // wait at bottom

        Vector3 next = bottom + Vector3.up * (riseHeight * Mathf.SmoothStep(0f, 1f, k));
        Delta = next - transform.position;
        transform.position = next;
    }
}
