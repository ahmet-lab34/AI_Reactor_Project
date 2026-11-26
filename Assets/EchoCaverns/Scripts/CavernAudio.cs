using UnityEngine;

// Plays the Echo Caverns event sounds. Starts sounds only from events, never from Update.
// Small random pitch (0.95-1.05) so repeats don't sound identical.
public class CavernAudio : MonoBehaviour
{
    [SerializeField] CavernGame game;
    [SerializeField] AudioClip sfxGemPickup;
    [SerializeField] AudioClip sfxCheckpoint;
    [SerializeField] AudioClip sfxRespawn;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D: these are player feedback sounds

        game.SecretFound += OnSecret;
        game.CheckpointReached += OnCheckpoint;
        game.Respawned += OnRespawn;
    }

    void OnDestroy()
    {
        if (game == null) return;
        game.SecretFound -= OnSecret;
        game.CheckpointReached -= OnCheckpoint;
        game.Respawned -= OnRespawn;
    }

    void OnSecret() => Play(sfxGemPickup);
    void OnCheckpoint() => Play(sfxCheckpoint);
    void OnRespawn() => Play(sfxRespawn);

    void Play(AudioClip clip)
    {
        if (clip == null) return;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clip);
    }
}
