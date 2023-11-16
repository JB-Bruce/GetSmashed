using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    List<PlayerController> players = new();

    [SerializeField] GameObject cursor;

    [SerializeField] Color[] playerColors;

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void AddPlayer(PlayerController player)
    {
        players.Add(player);
        GameObject newObject = Instantiate(cursor);
        newObject.GetComponent<ObjectFollow>().Init(player, playerColors[(players.Count - 1) % playerColors.Length], "Player " + (players.Count));

        
    }
}
