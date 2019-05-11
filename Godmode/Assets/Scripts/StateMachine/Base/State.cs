using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]

public class State : MonoBehaviour
{
    public StateMachine Machine;

    public static implicit operator bool (State state)
    {
        return state != null;
    }

    public void Initialize(StateMachine machine)
    {
        Machine = machine;
        OnStateInitialize(machine);
    }

    public virtual void OnStateInitialize(StateMachine machine = null)
    {

    }

    public void StateEnter()
    {
        enabled = true;
        OnStateEnter();
    }

    public virtual void OnStateEnter()
    {

    }

    public void StateExit()
    {
        OnStateExit();
        enabled = false;
    }

    public virtual void OnStateExit()
    {

    }

    public void OnEnable()
    {
        if(this != Machine.GetCurrentState)
        {
            enabled = false;
            Debug.LogWarning("Do not enable States directly. Use StateMachine.SetState()!");
        }
    }

    public void OnDisable()
    {
        if (this == Machine.GetCurrentState)
        {
            enabled = true;
            Debug.LogWarning("Do not disable States directly. Use StateMachine.SetState()!");
        }
    }

    private void OnValidate()
    {
        Machine = GetComponent<StateMachine>();
    }
}
