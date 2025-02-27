﻿using System;
using UnityEngine;
using UnityEngine.Networking;

public class RobotStateMachine : StateMachine {
    public Animator Animator = null;
    public PlayerController PlayerController = null;
    [HideInInspector] public FixedSizedQueue<string> StateHistory;
    public int MaxHistorySize = 12;
	[HideInInspector] public InputManager inputManager;

    // to be changed in a child class, if necessary
    public override string DefaultState {
        get { return "RobotIdleState"; }
    }

    void Update() {
        this.HandleInput();
    }

    void Start() {
        this.Initialize();
    }

    protected override void Initialize(string startingState = null) {
        base.Initialize();

        this.Animator = this.GetComponent<Animator>();
        this.PlayerController = this.GetComponent<PlayerController>();

        Type stateType = this.CheckStartingState(startingState);

        if (stateType == null) return;

        this.CurrentState = (RobotState) Activator.CreateInstance(stateType);
        this.StateHistory = new FixedSizedQueue<string>(this.MaxHistorySize);
		
		inputManager = PlayerController.inputManager;
    }

    protected override void SwitchState() {
        if (!(this.NextState is RobotState)) {
            return;
        }

        base.SwitchState();
        this.PlayerController.UpdateAnimations(
            this.CurrentState.GetType().Name);
    }
}
