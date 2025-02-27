﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour {

	public float distanceToOpponent;
	private int health;
	private float power;
	private int ennemyHealth_;
	private Genome genome;
	private RobotStateMachine stateMachine;
	private PlayerHealth robotHealth;
	private PlayerPower robotPower;
	private PlayerHealth ennemyHealth;
	private InputManager inputManager;
	private TargetManager targetManager;
	private PlayerController pc ;
	private AIFocus aiFocus;
	//local registers
	private bool allowAction = true;
	private int r, r1,i;
	private float f,f1,rand;

	// Use this for initialization
	void Start () {
		pc = gameObject.GetComponent<PlayerController> ();
		if (!pc.isAI)
			Destroy (this);
		
		genome = new Genome();
		aiFocus = this.GetComponent<AIFocus> ();

		stateMachine = pc.RobotStateMachine;
		robotHealth = pc.PlayerHealth;
		robotPower = pc.PlayerPower;
		ennemyHealth = pc.PlayerHealth;
		targetManager = pc.TargetManager;
		inputManager = pc.inputManager;

		health = robotHealth.Health;
		power = robotHealth.Health;
		r = health;
		ennemyHealth_ = ennemyHealth.Health;
		r1 = ennemyHealth_;

		targetManager.updateNearestOpponent ();

	}
	
	void Learn (bool b) {
		rand = Random.Range(0f,1f);
		if(b){
			if(r<0.75f){
				genome.dna[3].SetRecordTable(distanceToOpponent);
			}
			else{
				genome.dna[2].SetRecordTable(distanceToOpponent);
			}
		}
		else{
			if(r<0.75f){
				genome.dna[2].SetRecordTable(distanceToOpponent);
			}
			else{
				genome.dna[3].SetRecordTable(distanceToOpponent);
			}
		}
	}
	
	//might be more interesting if unique for each state
	float SetActionForce (int action,float a) {
		f1 = genome.dna[action].GetClosest(a);
		if(action == 2 || action == 3){
			return ( 1f - (f1/(genome.dna[action].GetBorderUp()-genome.dna[action].GetBorderLow())) - Mathf.Sqrt((100f-power)/100f) );
		}
		else{
			return ( 1f - (f1/(genome.dna[action].GetBorderUp()-genome.dna[action].GetBorderLow())) );
		}
	}
	
	void StopButtonAttack(){
		inputManager.attackButtonAI = false;
	}
	
	void StopButtonBlock() {
		inputManager.blockButtonAI = false;
	}

	void SetLatency(){
		allowAction = true;
	}
	
	void StopMove(){
		inputManager.moveForwardSpeedAI = 0f;
		inputManager.moveSideSpeedAI = 0f;
	}
		
	private float calculateDistanteToTarget()
	{
		Vector3 src = transform.position;
		Vector3 target = targetManager.currentTarget.transform.position;
		src.y = 0;
		target.z = 0;
		return Vector2.Distance (src,target);
	}

	private int count = 0;
	// Update is called once per frame
	void Update () {
		if (!PhotonNetwork.isMasterClient)
			return;

		if (aiFocus.targetUnreachable) {
			StopMove ();
			targetManager.updateNearestOpponent ();
		}
		
		if (targetManager.currentTarget != null) {
			
			//updating environment
			r = robotHealth.Health;
			power = robotPower.Power;
			r1 = ennemyHealth.Health;
			if (health != r) {
				Learn (true);
				health = r;
			}
			if (ennemyHealth_ != r1) {
				Learn (false);
				ennemyHealth_ = r1;
			}
			distanceToOpponent = calculateDistanteToTarget ();
			//chose action
			//1st action priority : attack
			if (distanceToOpponent > genome.dna [2].GetBorderLow () && distanceToOpponent < genome.dna [2].GetBorderUp ()) {
				StopMove ();
				f = SetActionForce (2, distanceToOpponent);
				rand = Random.Range (0f, 1f);
				if (f > rand) {
					if (allowAction) {
						inputManager.attackButtonAI = true;
						Invoke ("StopButtonAttack", 0.1f);
						allowAction = false;
						Invoke ("SetLatency", 0.2f);
						return;
					}
				} else {
					//2nd action priority : block
					if (distanceToOpponent > genome.dna [3].GetBorderLow () && distanceToOpponent < genome.dna [3].GetBorderUp ()) {
						f = SetActionForce (3, distanceToOpponent);
						rand = Random.Range (0f, 1f);
						if (f > rand) {
							if (allowAction) {
								if (!inputManager.blockButtonAI) {
									inputManager.blockButtonAI = true;
									Invoke ("StopButtonBlock", 1f);
									allowAction = false;
									Invoke ("SetLatency", 0.2f);
									return;
								}
							}
						} else {
							inputManager.moveForwardSpeedAI = -1.0f;
							//Invoke ("StopMove", 1.8f);
						}
					}
				}
			}
				//3rd action priority : walk
			else if (distanceToOpponent > genome.dna [1].GetBorderLow () && distanceToOpponent < genome.dna [1].GetBorderUp ()) {
				f = SetActionForce (1, distanceToOpponent);
				rand = Random.Range (0f, 1f);
				if (rand < f) {
					rand = Random.Range (0f, 1f);
					if (rand > Mathf.Sqrt ((100f - power) / 100f)) {
						inputManager.moveForwardSpeedAI = -1.0f;
						//Invoke ("StopMove", 1.8f);
					}/* else {
						if (distanceToOpponent < 3) {
							inputManager.moveForwardSpeedAI = 1.5f;
						}
						rand = Random.Range (0f, 1f);
						if (rand > 0.5f) {
							inputManager.moveSideSpeedAI = 1.5f;
						} else {
							inputManager.moveSideSpeedAI = -1.5f;
						}
						//Invoke ("StopMove", 1.8f);
					}*/
					return;
				} else {
					// 4th action priority : idle
					// if(distanceToOpponent > genome.dna[0].GetBorderLow() && distanceToOpponent < genome.dna[0].GetBorderUp()){
					// f = SetActionForce(distanceToOpponent);
					// rand = Random.Range(0f,1f);
					// if (rand < f){
					// pc.RobotStateMachine.SetState(new RobotIdleState());
					// }
					// }
				}
			}

			if(distanceToOpponent<=1.6f)
				StopMove ();
		} else {
			targetManager.updateNearestOpponent ();
		}
	
	}
}
