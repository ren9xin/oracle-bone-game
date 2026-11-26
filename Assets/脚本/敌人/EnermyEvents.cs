using UnityEngine;

public static class EnemyEvents
{
    public static System.Action<Transform,bool> OnPlayerSpotted;
    public static void PlayerSpotted(Transform player,bool senderIsUpper)
    {
        OnPlayerSpotted?.Invoke(player,senderIsUpper);
    }
}
