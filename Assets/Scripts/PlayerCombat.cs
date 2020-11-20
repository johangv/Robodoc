using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    //Code to the enemy

    [SerializeField]
    private bool combatEnabled;
    [SerializeField]
    private float inputTimer, attack1Radius, attack1Damage;
    [SerializeField]
    private Transform attack1HitBoxPos;
    [SerializeField]
    private LayerMask whatIsDamageable;

    private bool gotInput, isAttacking, isFirstAttack;

    private float lastInputTime = Mathf.NegativeInfinity;

    private Animator anim;

    private float[] attackDetails = new float[2];

    private CharacterController2D pc;

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("canAttack", combatEnabled);

        pc = GetComponent<CharacterController2D>();
    }

    private void CheckCombatInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (combatEnabled)
            {
                //Attempt combat
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            //Perform Attack1
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                isFirstAttack = true;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
            }
        }
        if (Time.time >= lastInputTime + inputTimer)
        {
            //Wait for new input
            gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position,
            attack1Radius, whatIsDamageable);

        attackDetails[0] = attack1Damage;
        attackDetails[1] = transform.position.x;

        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", attackDetails);
            //Instantiate hit particle
        }

    }
    private void FinishAttack1()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    //Player Function for the player----------------------------------------------------------------------------------------------------
    private void Damage(float[] attackDetails)
    {
            int direction;
            //Damage player here using attackDetails[0]

            if (attackDetails[1] < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }

        pc.Knockback(direction);
        pc.UpdateDamage(attackDetails[0]);
        
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }

}
