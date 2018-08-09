using HoloIslandVis;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public class InteractionDriver : SingletonComponent<InteractionDriver>
    {
        protected override void Awake()
        {
            base.Awake();
            //GestureInputListener.Instance.OneHandTap += onOneHandTap;
            //GestureInputListener.Instance.TwoHandTap += onTwoHandTap;
            //GestureInputListener.Instance.OneHandDoubleTap += onOneHandDoubleTap;
            //GestureInputListener.Instance.TwoHandDoubleTap += onTwoHandDoubleTap;
            //GestureInputListener.Instance.OneHandManipulationStart += onOneHandManipulationStart;
            //GestureInputListener.Instance.TwoHandManipulationStart += onTwoHandManipulationUpdate;
            //GestureInputListener.Instance.OneHandManipulationUpdate += onOneHandManipulationUpdate;
            //GestureInputListener.Instance.TwoHandManipulationUpdate += onTwoHandManipulationUpdate;
            //GestureInputListener.Instance.ManipulationEnd += onManipulationEnd;
            //GestureInputListener.Instance.ManipulationTap += onManipulationTap;
            //GestureInputListener.Instance.ManipulationDoubleTap += onManipulationDoubleTap;
        }

        private void onOneHandTap(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onTwoHandTap(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onOneHandDoubleTap(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onTwoHandDoubleTap(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onOneHandManipulationStart(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onOneHandManipulationUpdate(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onTwoHandManipulationUpdate(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onManipulationEnd(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onManipulationTap(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }

        private void onManipulationDoubleTap(BaseInputEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}
