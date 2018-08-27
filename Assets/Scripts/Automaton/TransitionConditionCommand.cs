using HoloIslandVis.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public class TransitionConditionCommand : Command
    {
        public delegate void ConditionMetHandler(TransitionConditionCommand command);
        public event ConditionMetHandler ConditionMet;

        public Func<bool> _condition;

        public TransitionConditionCommand(Func<bool> condition)
        {
            _condition = condition;
        }

        public void OnObservableValueChanged()
        {
            bool result = _condition.Invoke();
            if(result)
                ConditionMet(this);
        }
    }
}
