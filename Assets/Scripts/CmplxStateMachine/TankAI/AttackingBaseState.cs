﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace cmplx_statemachine
{
    public class AttackingBaseState : BaseState
    {
        Transform selfTransform;
        TankAIScript3 tankAIScript;
        NewTankScript tankController;
        Path path;//For generation of local path
        Transform localTarget;
        int localCurrWaypoint;
        bool hasReachedEndOfLocalPath;
        float nextWPDistance;

        Vector2 dirToTarget;

        bool isLocalTargetCoroutineRunning;

        public AttackingBaseState(CStateMachine sTM,TankAIScript3 tankAI):base(sTM)
        {
            stateName = "ATTK_BASE";
            tankAIScript = tankAI;
            tankController = tankAI.GetComponent<NewTankScript>();
            selfTransform = tankAI.GetComponent<Transform>();
            isLocalTargetCoroutineRunning = false;
        }

        public override void OnEnter()
        {
            //Destroy Watchtowers
            //Destroy Artilery if any
            //Finally Destroy Command Center
            tankAIScript.StartCoroutine(DestroyWatchTowers());
            
            
        }

        //------------------------
        //Complex Coroutines
        //------------------------

        IEnumerator DestroyWatchTowers()
        {
            List<WatchTowerAIScript> watchTowers = ((NewArmyBaseScript)tankAIScript.targetBase).watchTowers;

            while (watchTowers.Count != 0)
            {
                watchTowers = PickAliveWatchTowers(watchTowers);

                var nearest = FindNearestWatchTower(watchTowers);

                dirToTarget = (nearest.transform.position - selfTransform.position).normalized;

                TryFaceTowardsDirection();

                if (IsFacingTarget(nearest.transform))
                {
                    tankController.Shoot();
                }
               
                yield return null;//Pause here.
            }

            yield return null;
        }

        IEnumerator DestroyCommCenter()
        {
            yield return null;
        }

        WatchTowerAIScript FindNearestWatchTower(List<WatchTowerAIScript> list)
        {
            if (list.Count == 0) return null;
            WatchTowerAIScript nearest = null;

            nearest = list[0];

            for (int i = 1; i < list.Count; i++)
            {
                float dist = Vector2.Distance(nearest.transform.position, list[i].transform.position);//Find nearest distance
                float distA = Vector2.Distance(selfTransform.position, list[i].transform.position);//Find current distance

                if (distA < dist)
                {
                    nearest = list[i];
                }

            }
            return nearest;
        }

        void TryFaceTowardsDirection()
        {
            float angle = Vector2.SignedAngle(dirToTarget, tankController.muzzleTransform.up);
            if (Mathf.Abs(angle) > 2)
            {
                if (angle > 0)
                { tankController.MuzzleRotate(1); }
                else { tankController.MuzzleRotate(-1); }
            }
        }

        bool IsFacingTarget(Transform target)
        {
            Vector2 dirToT = (target.position - selfTransform.position).normalized;
            float angle = Vector2.Angle(dirToT, tankController.muzzleTransform.up);
            if (angle < 2)
            { return true; }
            return false;
        }

        List<WatchTowerAIScript> PickAliveWatchTowers(List<WatchTowerAIScript> list)
        {
            List<WatchTowerAIScript> output = new List<WatchTowerAIScript>();

            foreach (var item in list)
            {
                if (item.GetComponent<HealthScript>().currentHP > 0)
                {
                    output.Add(item);
                }
            }
            return output;
        }


    }
}