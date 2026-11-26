using UnityEngine;

// One trigger script for every level trigger. Pick what it does in Kind.
[RequireComponent(typeof(Collider))]
public class CavernTrigger : MonoBehaviour
{
    public enum Kind { StartTimerOnExit, Checkpoint, KillZone, Goal, Secret }

    [SerializeField] CavernGame game;
    [SerializeField] Kind kind;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CavernPlayer>() == null) return;
        switch (kind)
        {
            case Kind.Checkpoint: game.SetCheckpoint(transform); break;
            case Kind.KillZone: game.Respawn("You fell - back to the checkpoint"); break;
            case Kind.Goal: game.Escape(); break;
            case Kind.Secret:
                game.FoundSecret();
                gameObject.SetActive(false);
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (kind == Kind.StartTimerOnExit && other.GetComponent<CavernPlayer>() != null) game.StartTimer();
    }

    void Update()
    {
        if (kind == Kind.Secret) transform.Rotate(0f, 90f * Time.deltaTime, 0f); // the gem spins
    }
}
