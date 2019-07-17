using System.Collections;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskDragPhysics : ContinuousInteractionTask
    {
        private Interactable _target;
        private Rigidbody _rigidbody;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;

        public override IEnumerator StartInteraction(GestureInteractionEventArgs eventArgs)
        {
            Debug.Log("Start Interaction: " + eventArgs.Focused.Type);
            _target = eventArgs.Focused;
            _rigidbody = _target.GetComponentInChildren<Rigidbody>();
            _rigidbody.isKinematic = false;

            _lastPosition = eventArgs.HandOnePos;
            _currentPosition = eventArgs.HandOnePos;

            yield break;
        }

        public override IEnumerator UpdateInteraction(GestureInteractionEventArgs eventArgs)
        {
            Debug.Log("Update Interaction: " + eventArgs.Focused.Type);
            _currentPosition = eventArgs.HandOnePos;
            Vector3 velocity = _currentPosition - _lastPosition;
            _rigidbody.velocity = velocity * 100;
            _lastPosition = _currentPosition;

            yield break;
        }

        public override IEnumerator EndInteraction(GestureInteractionEventArgs eventArgs)
        {
            Debug.Log("End Interaction: " + eventArgs.Focused.Type);
            _rigidbody.isKinematic = true;
            yield break;
        }
    }
}
