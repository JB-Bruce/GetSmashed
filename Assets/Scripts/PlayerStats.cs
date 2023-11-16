using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI damage;

    [SerializeField] Transform lifeParent;
    [SerializeField] GameObject newLifePrefab;

    List<Image> allLifes = new();

    [SerializeField] Sprite fullLife;
    [SerializeField] Color fullLifeColor;
    [SerializeField] Sprite lostLife;
    [SerializeField] Color lostLifeColor;

    [SerializeField] Gradient gradiant;
    [SerializeField] float maxValueForGradiant;

    float lastHealth = 0f;

    Animator animator;

    PlayerController player;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Init(PlayerController newPlayer, string playerName, Color playerColor)
    {
        player = newPlayer;
        
        nameText.text = playerName;
        nameText.color = playerColor;

        SetDamage();

        for (int i = 0; i < player.lifes; i++)
        {
            var newLife = Instantiate(newLifePrefab, lifeParent);
            Image tmpImg = newLife.GetComponent<Image>();
            tmpImg.color = fullLifeColor;
            allLifes.Add(tmpImg);
        }

        player.takeDamageEvent.AddListener(OnTakeDamage);
        player.dieEvent.AddListener(OnDie);
    }

    private void SetDamage()
    {
        float damages = player.damageTaken;
        Color newColor = gradiant.Evaluate(damages * 1f / maxValueForGradiant);

        damage.color = newColor;

        damage.text = damages.ToString() + " " + "<size=35>%</size>";
    }

    public void OnTakeDamage()
    {
        SetDamage();

        if(player.damageTaken - lastHealth > 10f)
            animator.Play("GigaShake", 0, 0);
        else
            animator.Play("Shake", 0, 0);

        lastHealth = player.damageTaken;
    }

    public void OnDie()
    {
        SetDamage();
        Image tmpImg = allLifes[player.lifes];
        tmpImg.sprite = lostLife;
        tmpImg.color = lostLifeColor;
    }
}
