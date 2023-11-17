using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerManager : MonoBehaviour
{
    List<PlayerController> players = new();

    List<PlayerController> deadPlayers = new();

    [SerializeField] Transform[] spawnPoints;

    [SerializeField] Color[] playerColors;

    [SerializeField] Transform playerHealthParent;
    [SerializeField] GameObject healthPrefab;
    [SerializeField] GameObject objectFollowerPrefab;

    [SerializeField] GameObject winCanvas;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] float timeScaleReduceSpeed;

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        winCanvas.SetActive(false);
    }

    public Color AddPlayer(PlayerController player, out string pName)
    {
        players.Add(player);
        GameObject newObject = Instantiate(objectFollowerPrefab);

        Color playerColor = playerColors[(players.Count - 1) % playerColors.Length];

        pName = "Player " + (players.Count);

        newObject.GetComponent<ObjectFollow>().Init(player, playerColor, pName);
        

        GameObject health = Instantiate(healthPrefab, playerHealthParent);
        health.GetComponent<PlayerStats>().Init(player, pName, playerColor);

        foreach (var item in players)
        {
            if(item != player)
            {
                item.EnableCollisions(player.plat.box);
                player.EnableCollisions(item.plat.box);
            }
        }

        Replace(player);

        return playerColor;
    }

    public void Replace(PlayerController pc)
    {
        pc.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }

    public void Die(PlayerController pc)
    {
        players.Remove(pc);
        deadPlayers.Add(pc);

        if (players.Count == 1)
            Win(players[0]);
    }

    private void Win(PlayerController pc)
    {
        StartCoroutine(EWin());
        winCanvas.SetActive(true);
        playerNameText.text = pc.playerName;
        playerNameText.color = pc.playerColor;
    }

    IEnumerator EWin()
    {
        while(Time.timeScale > 0f)
        {
            float newTimeScale = Mathf.Clamp(Time.timeScale - (timeScaleReduceSpeed * Time.deltaTime), 0f, 1f);
            Time.timeScale = newTimeScale;

            yield return null;
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
