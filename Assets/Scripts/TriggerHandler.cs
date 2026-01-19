using UnityEngine;

public enum TriggerType
{
    SpeedUp,
    SlowDown,
    Stop,
    Win,
    Crash // Acts like an obstacle
}

public class TriggerHandler : MonoBehaviour
{
    public TriggerType type;
    public float duration = 2f; // For temporary effects like speed up/slow down
    public float speedMultiplier = 2f; // How much to speed up or slow down (0.5 for slow down)

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger hit by: " + other.gameObject.name + " | Tag: " + other.tag);
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyEffect(player);
                
                // Disable visual to "consume" the item, unless it's a win zone
                if (type != TriggerType.Win && type != TriggerType.Stop)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    private void ApplyEffect(PlayerController player)
    {
        switch (type)
        {
            case TriggerType.SpeedUp:
                // Speed up logic
                player.ChangeSpeed(speedMultiplier, duration);
                break;
            case TriggerType.SlowDown:
                // Slow down logic
                player.ChangeSpeed(0.5f, duration);
                break;
            case TriggerType.Stop:
                // Stop logic (e.g. cobweb)
                player.ChangeSpeed(0f, duration);
                break;
            case TriggerType.Win:
                player.WinLevel();
                break;
            case TriggerType.Crash:
                // Same as obstacle
                // We access the private Die method via a public crash method or similar if needed, 
                // but usually PlayerController handles collisions. 
                // However, triggers are triggers, not collisions.
                player.Crash(); 
                break;
        }
    }
}
