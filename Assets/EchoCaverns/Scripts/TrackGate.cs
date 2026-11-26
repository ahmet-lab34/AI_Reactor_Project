using UnityEngine;

// Start or end line of the 20 m walk-speed track in the metrics gym.
[RequireComponent(typeof(Collider))]
public class TrackGate : MonoBehaviour
{
    [SerializeField] MetricsReadout readout;
    [SerializeField] bool isEnd;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CavernPlayer>() == null) return;
        if (isEnd) readout.TrackEnd();
        else readout.TrackStart();
    }
}
