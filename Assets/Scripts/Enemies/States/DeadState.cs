using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : State
{
    protected D_DeadState stateData;

    public DeadState(Entity etity, FiniteStateMachine stateMachine, string animBoolName, D_DeadState stateData) : base(etity, stateMachine, animBoolName)
    {
        this.stateData = stateData;
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        //Switch dead animation  
        entity.simpleBotGO.layer = 10;
        entity.anim.SetTrigger("IsDead");
        Vector2 movement = new Vector2(0.0f, 0.0f);
        entity.rb.velocity = movement;
       
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
