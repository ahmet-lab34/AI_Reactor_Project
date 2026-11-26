using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Echo Caverns rules: run timer (starts when leaving the entrance), checkpoints, secrets, escape.
public class CavernGame : MonoBehaviour
{
    [SerializeField] CavernPlayer player;
    [SerializeField] Transform playerStart;
    [SerializeField] int secretsTotal = 1;

    public event Action<float> TimeChanged;          // seconds, every 0.1 s
    public event Action<int, int> SecretsChanged;    // found, total
    public event Action<string> Message;             // short flash text
    public event Action<float, int, int> Escaped;    // time, found, total
    public event Action CheckpointReached;           // sound events
    public event Action Respawned;
    public event Action SecretFound;

    public bool Running { get; private set; }
    public bool Finished { get; private set; }

    float time;
    int lastTenth = -1;
    int secretsFound;
    Vector3 checkpointPos;
    Quaternion checkpointRot;
    float safeUntil;

    void Start()
    {
        checkpointPos = playerStart.position;
        checkpointRot = playerStart.rotation;
        player.Teleport(checkpointPos, checkpointRot);
        TimeChanged?.Invoke(0f);
        SecretsChanged?.Invoke(0, secretsTotal);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (!Running || Finished) return;
        time += Time.deltaTime;
        int tenth = Mathf.FloorToInt(time * 10f);
        if (tenth != lastTenth)
        {
            lastTenth = tenth;
            TimeChanged?.Invoke(time);
        }
    }

    public void StartTimer()
    {
        if (Running || Finished) return;
        Running = true;
    }

    public void SetCheckpoint(Transform point)
    {
        if (Vector3.Distance(point.position, checkpointPos) < 0.1f) return;
        checkpointPos = point.position;
        checkpointRot = point.rotation;
        Message?.Invoke("Checkpoint");
        CheckpointReached?.Invoke();
    }

    public void Respawn(string reason)
    {
        if (Finished || Time.time < safeUntil) return;
        safeUntil = Time.time + 1f;
        player.Teleport(checkpointPos, checkpointRot);
        Message?.Invoke(reason);
        Respawned?.Invoke();
    }

    public void FoundSecret()
    {
        secretsFound++;
        SecretsChanged?.Invoke(secretsFound, secretsTotal);
        Message?.Invoke("Secret found!");
        SecretFound?.Invoke();
    }

    public void Escape()
    {
        if (Finished) return;
        Finished = true;
        player.InputEnabled = false;
        TimeChanged?.Invoke(time);
        Escaped?.Invoke(time, secretsFound, secretsTotal);
    }
}
