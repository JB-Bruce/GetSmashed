using TMPro;
using UnityEngine;

public class ObjectFollow : MonoBehaviour
{
    PlayerController followedPlayer;
    Color followedPlayerColor;
    string followedPlayerName;

    [SerializeField] SpriteRenderer triangleSprite;
    [SerializeField] TextMeshProUGUI nameText;

    public void Init(PlayerController playerController, Color color, string text)
    {
        followedPlayer = playerController;
        followedPlayerColor = color;
        followedPlayerName = text;

        triangleSprite.color = followedPlayerColor;
        nameText.color = followedPlayerColor;
        nameText.text = followedPlayerName;
    }

    private void Update()
    {
        transform.position = followedPlayer.transform.position;
    }
}
