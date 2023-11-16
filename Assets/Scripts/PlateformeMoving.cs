using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlateformeMoving : MonoBehaviour
{

    [SerializeField] Transform[] WayPoints;
    Transform target;
    [SerializeField] float speed;
    private int destPoint = 0;
    private bool canMove = false;


    private void Start()
    {
        if (WayPoints.Count() >= 1)
        {
            canMove = true;
            target = WayPoints[0];
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(canMove) 
        {
            Vector3 dir = target.position - transform.position;
            transform.Translate(speed * Time.deltaTime * dir.normalized, Space.World);

            if (Vector3.Distance(transform.position, target.position) < 0.03f)
            {
                destPoint = (destPoint + 1) % WayPoints.Length;
                target = WayPoints[destPoint];
            }
        }
    }
}
