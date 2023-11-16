using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    List<PlayerController> players = new();


    [SerializeField] Color[] playerColors;

    [SerializeField] Transform playerHealthParent;
    [SerializeField] GameObject healthPrefab;

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Color AddPlayer(PlayerController player)
    {
        players.Add(player);
        GameObject newObject = Instantiate(objectFollowerPrefab);

        Color playerColor = playerColors[(players.Count - 1) % playerColors.Length];

        newObject.GetComponent<ObjectFollow>().Init(player, playerColor, "Player " + (players.Count));

        GameObject health = Instantiate(healthPrefab, playerHealthParent);
        health.GetComponent<PlayerStats>().Init(player, "Player " + (players.Count), playerColor);

        foreach (var item in players)
        {
            if(item != player)
            {
                item.EnableCollisions(player.plat.box);
                player.EnableCollisions(item.plat.box);
            }
        }

        return playerColor;
    }
}
