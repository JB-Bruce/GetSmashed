using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    List<PlayerController> players = new();

    [SerializeField] GameObject objectFollowerPrefab;

    [SerializeField] Color[] playerColors;

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void AddPlayer(PlayerController player)
    {
        players.Add(player);
        GameObject newObject = Instantiate(objectFollowerPrefab);
        newObject.GetComponent<ObjectFollow>().Init(player, playerColors[(players.Count - 1) % playerColors.Length], "Player " + (players.Count));

        foreach (var item in players)
        {
            if(item != player)
            {
                item.EnableCollisions(player.plat.box);
                player.EnableCollisions(item.plat.box);
            }
        }
        
    }
}
