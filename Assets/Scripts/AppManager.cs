using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Mapping;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis
{
    public class AppManager : SingletonComponent<AppManager>
    {

        // Use this for initialization
        void Start()
        {
            SpatialScan.Instance.RequestBeginScanning();
            //stateMachineDebug();
            //inputListenerDebug();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void stateMachineDebug()
        {
            BaseState state1 = new BaseState("State1");
            Command commandA = new Command(GestureType.Tap, KeywordType.Tap, InteractableType.Island);
            Command commandB = new Command(GestureType.DoubleTap, KeywordType.Tap, InteractableType.Panel);

            BaseState state2 = new BaseState("State2");
            BaseState state3 = new BaseState("State3");

            state1.AddStateTransition(commandA, state2);
            state1.StateOpened += (object sender, EventArgs args) =>
            {
                SpatialScan.Instance.RequestBeginScanning();
            };

            StateMachine stateMachine = new StateMachine(state1);
            stateMachine.AddState(state2);
            stateMachine.AddState(state3);

            Debug.Log("Current state: " + stateMachine.CurrentState.Name);
            stateMachine.IssueCommand(commandA);
            Debug.Log("Current state: " + stateMachine.CurrentState.Name);
        }

        public void inputListenerDebug()
        {
            //GestureInputListener.Instance.OneHandTap += (BaseInputEventData eventData) => Debug.Log("OneHandTap");
            //GestureInputListener.Instance.TwoHandTap += (BaseInputEventData eventData) => Debug.Log("TwoHandTap");
            //GestureInputListener.Instance.OneHandDoubleTap += (BaseInputEventData eventData) => Debug.Log("OneHandDoubleTap");
            //GestureInputListener.Instance.TwoHandDoubleTap += (BaseInputEventData eventData) => Debug.Log("TwoHandDoubleTap");
            //GestureInputListener.Instance.OneHandManipulationStart += (BaseInputEventData eventData) => Debug.Log("OneHandManipulationStart");
            //GestureInputListener.Instance.TwoHandManipulationStart += (BaseInputEventData eventData) => Debug.Log("TwoHandManipulationStart");
            //GestureInputListener.Instance.ManipulationEnd += (BaseInputEventData eventData) => Debug.Log("OneHandManipulationEnd");
            //GestureInputListener.Instance.ManipulationEnd += (BaseInputEventData eventData) => Debug.Log("TwoHandManipulationEnd");
        }
    }

}