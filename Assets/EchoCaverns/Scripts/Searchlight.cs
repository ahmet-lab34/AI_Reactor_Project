using UnityEngine;

// Turns 90 degrees per second. If the player is inside the cone and the ray to their head
// isn't blocked by cover, they go back to the checkpoint. Crouching behind low cover hides you.
public class Searchlight : MonoBehaviour
{
    [SerializeField] CavernGame game;
    [SerializeField] CavernPlayer player;
    public float degreesPerSecond = 90f;
    public float coneHalfAngle = 15f;
    public float range = 14f;

    void Update()
    {
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
        if (game == null || player == null || !game.Running || game.Finished) return;

        Vector3 head = player.HeadPoint;
        Vector3 toPlayer = head - transform.position;
        if (toPlayer.magnitude > range) return;

        Vector3 flatTo = new Vector3(toPlayer.x, 0f, toPlayer.z);
        Vector3 flatFwd = new Vector3(transform.forward.x, 0f, transform.forward.z);
        if (Vector3.Angle(flatFwd, flatTo) > coneHalfAngle) return;

        if (Physics.Raycast(transform.position, toPlayer.normalized, out var hit, toPlayer.magnitude + 0.5f, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.GetComponent<CavernPlayer>() != null)
        {
            game.Respawn("Spotted - back to the checkpoint");
        }
    }
}
