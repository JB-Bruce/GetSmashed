using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTopAnimationEvents : MonoBehaviour
{
    public UnityEvent startAttackEvent;
    public UnityEvent endAttackEvent;

    public void StartAttack()
    {
        startAttackEvent.Invoke();
    }

    public void EndAttack()
    {
        endAttackEvent.Invoke();
    }
}
