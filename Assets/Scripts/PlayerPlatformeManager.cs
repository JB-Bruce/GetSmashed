using UnityEngine;

public class PlayerPlatformeManager : MonoBehaviour
{
    public BoxCollider2D box;

    [SerializeField] PlayerController playerController;

    Platforme touchedPlat;

    [SerializeField] LayerMask mask;

    [SerializeField] Transform feet;
    [SerializeField] BoxCollider2D feetCol;

    bool block = false;

    private void Update()
    {
        if (block)
            return;

        RaycastHit2D hit = Physics2D.Raycast(feet.position, Vector2.down, .5f, mask);

        if(hit.transform != null)
        {
            if (hit.transform.TryGetComponent<Platforme>(out Platforme plat) && hit.transform != transform)
            {
                if(transform.parent != plat.transform && playerController.rb.velocity.y <= 0f && !hit.collider.IsTouching(feetCol))
                {
                    touchedPlat = plat;
                    transform.parent = touchedPlat.transform;
                    transform.localPosition = Vector2.zero;
                    transform.localScale = Vector2.one;
                    box.size = touchedPlat.actualBox.size;

                    playerController.transform.parent = touchedPlat.transform;

                    return;
                }
            }
            return;
        }

        if(transform.parent != transform)
        {
            playerController.transform.parent = null;

            transform.parent = playerController.transform;
            box.size = Vector2.zero;
        }
        
    }

    public void GoDown()
    {
        block = true;
        Invoke("StopBlock", .2f);

        playerController.transform.parent = null;

        transform.parent = playerController.transform;
        box.size = Vector2.zero;
    }

    private void StopBlock()
    {
        block = false;
    }
}
