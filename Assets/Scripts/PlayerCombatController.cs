using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField]
    private bool combatEnabled;
    [SerializeField]
    private float inputTimer;

    private bool InputState, isAttacking;

    private float lastInputTime;

    private Animator anim;


    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("CanAttack", combatEnabled);
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (combatEnabled)
        {
            //Atemp combat
            InputState = true;
            lastInputTime = Time.time;
        }
    }

    private void CheckAttacks()
    {
        if (InputState)
        {
            //Perform attack
            if (!isAttacking)
            {
                InputState = false;
                isAttacking = true;
                anim.SetBool("Attacking", isAttacking);
            }
        }

        if (Time.time >= lastInputTime + inputTimer)
        {
            //Wait for new inputs
            InputState = false;
        }
    }


}
