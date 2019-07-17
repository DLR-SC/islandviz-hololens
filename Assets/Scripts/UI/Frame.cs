using HoloIslandVis.UI.Info;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{ 
    public class Frame : MonoBehaviour
    {
        private const float THRESHOLD   = 0.005f;
        private const float INTERP_FAC  = 0.5f;

        private enum FrameSide
        {
            Forward,
            Backward,
            Right,
            Left
        }

        private float _bias;
        private float _thickness;
        private float _rollOutAmount;

        private Dictionary<FrameSide, Vector3> _frameSides;
        private FrameSide _unrolledSide;
        private bool _unrolled;
        private bool _rollingOut;
        private bool _rollingIn;

        private Vector3 _baseScale;
        private Vector3 _basePosition;

        private Vector3 _forward;
        private Vector3 _backward;
        private Vector3 _right;
        private Vector3 _left;

        [Range(0.0f, 1.0f)] public float Bias;
        [Range(0.0f, 1.0f)] public float Thickness;
        [Range(0.0f, 1.0f)] public float RollOutAmount;
        public Toolbar Toolbar;
        public InfoPanel Panel;

        public bool Activating {
            get; private set;
        }

        void Start ()
        {
            _bias = Bias / 2.0f;
            _thickness = Thickness * 0.1f;
            _rollOutAmount = RollOutAmount / 2;

            _frameSides = new Dictionary<FrameSide, Vector3>();
            _frameSides.Add(FrameSide.Forward, Vector3.forward);
            _frameSides.Add(FrameSide.Backward, -Vector3.forward);
            _frameSides.Add(FrameSide.Right, Vector3.right);
            _frameSides.Add(FrameSide.Left, -Vector3.right);

            float factor = 1 + _thickness * 2;
            _baseScale = new Vector3(factor, transform.localScale.y, factor);
            transform.localScale = _baseScale;
            _basePosition = transform.localPosition;
        }

        public void Activate()
        {
            Activating = true;
            Toolbar.Rearrange();
            InvokeRepeating("Refresh", 0, 0.2f);
        }

        public void Deactivate()
        {
            Activating = true;
            StartCoroutine(RollIn(_unrolledSide));
            CancelInvoke("Refresh");
        }

        private void Refresh()
        {
            if (_unrolled && !_rollingIn)
            {
                Vector3 temp = -ProjectedNormalizedGaze(transform.up);
                float dot = Vector3.Dot(_frameSides[_unrolledSide], temp);

                if (dot + _bias > 0.5f)
                    return;

                StartCoroutine(RollIn(_unrolledSide));
            }

            if(!_unrolled && !_rollingOut)
            {
                Vector3 temp = -ProjectedNormalizedGaze(transform.up);
                FrameSide side = DetermineSideToUnroll(temp);
                StartCoroutine(RollOut(side));
            }
        }

        private FrameSide DetermineSideToUnroll(Vector3 other)
        {
            float dotFwd = Vector3.Dot(_frameSides[FrameSide.Forward], other);
            float dotRight = Vector3.Dot(_frameSides[FrameSide.Right], other);
 
            if (dotFwd > 0.5f)          return FrameSide.Forward;
            else if (dotFwd < -0.5f)    return FrameSide.Backward;
            else if (dotRight > 0.5f)   return FrameSide.Right;
            else                        return FrameSide.Left;
        }

        private Vector3 ProjectedNormalizedGaze(Vector3 normal)
        {
            Vector3 gazePosition = ToolkitHelper.Instance.GazePosition;
            Vector3 frameDirection = (transform.position - gazePosition).normalized;
            frameDirection = Quaternion.Euler(-transform.rotation.eulerAngles) * frameDirection;
            Vector3 projectedGaze = Vector3.ProjectOnPlane(frameDirection, normal);
            return projectedGaze = projectedGaze.normalized;
        }

        private Vector3 GetTargetScale(Vector3 scale, FrameSide side)
        {
            switch (side)
            {
                case FrameSide.Forward:     scale.z += _rollOutAmount; break;
                case FrameSide.Backward:    scale.z += _rollOutAmount; break;
                case FrameSide.Right:       scale.x += _rollOutAmount; break;
                case FrameSide.Left:        scale.x += _rollOutAmount; break;
            }

            return scale;
        }

        private Vector3 GetTargetPosition(Vector3 position, FrameSide side)
        {
            switch (side)
            {
                case FrameSide.Forward:     position.z += _rollOutAmount/2; break;
                case FrameSide.Backward:    position.z -= _rollOutAmount/2; break;
                case FrameSide.Right:       position.x += _rollOutAmount/2; break;
                case FrameSide.Left:        position.x -= _rollOutAmount/2; break;
            }

            return position;
        }

        private float GetPanelScale()
        {
            PanelItem panelBody = PanelBody.ActiveBody;
            if (PanelItem.None == panelBody) return 0.18f;
            else return 1.00f;
        }

        private Vector3 GetPanelPosition(FrameSide side)
        {
            Vector3 result = Vector3.zero;
            result = _frameSides[side] * (-0.53f);
            return result;
        }

        private Quaternion GetPanelRotation(FrameSide side)
        {
            Vector3 gazePosition = ToolkitHelper.Instance.GazePosition;
            Vector3 panelDirection = (Panel.transform.position - gazePosition).normalized;
            Vector3 userDirection = -panelDirection;

            Vector3 planeNormal = Vector3.Cross(transform.up, _frameSides[side]);
            float dist = Vector3.Dot(userDirection, planeNormal);
            userDirection = userDirection - planeNormal * dist;
            userDirection = userDirection.normalized;

            float tiltAngle = Vector3.Angle(userDirection, _frameSides[side]) - 15;
            tiltAngle = Mathf.Clamp(tiltAngle, 0, 90);

            Quaternion result = Quaternion.LookRotation(_frameSides[side], Vector3.up);
            result = result * Quaternion.Euler(-tiltAngle, 0, 0);
            return result;
        }

        private Vector3 GetToolbarScale()
        {
            Vector3 result = new Vector3(1.0f, 1.0f, 0.5f);
            if (_rollOutAmount < 0.125f)
                result *= _rollOutAmount * 8.0f;

            return result;
        }

        private Vector3 GetToolbarPosition(FrameSide side)
        {
            Vector3 result = Vector3.zero;
            float factor = (_rollOutAmount / 2.0f) + 0.5f + _thickness;
            result = _frameSides[side] * factor;
            return result;
        }

        private Quaternion GetToolbarRotation(FrameSide side)
        {
            return Quaternion.LookRotation(-Vector3.up, -_frameSides[side]);
        }

        private IEnumerator RollOut(FrameSide side)
        {
            _rollingOut = true;

            Vector3 tempScale = transform.localScale;
            Vector3 tempPosition = transform.localPosition;
            Vector3 targetScale = GetTargetScale(tempScale, side);
            Vector3 targetPosition = GetTargetPosition(tempPosition, side);

            while(Vector3.Distance(tempScale, targetScale) > THRESHOLD)
            {
                tempScale = Vector3.Lerp(tempScale, targetScale, INTERP_FAC);
                tempPosition = Vector3.Lerp(tempPosition, targetPosition, INTERP_FAC);

                transform.localScale = tempScale;
                transform.localPosition = tempPosition;

                yield return new WaitForSeconds(0.01f);
            }

            transform.localScale = targetScale;
            transform.localPosition = targetPosition;

            Vector3 tbScale = GetToolbarScale();
            Vector3 tbPosition = GetToolbarPosition(side);
            Quaternion tbRotation = GetToolbarRotation(side);
            Toolbar.Refresh(tbPosition, tbScale, tbRotation);
            yield return Toolbar.Activate();

            float panelScale = GetPanelScale();
            Vector3 panelPosition = GetPanelPosition(side);
            Quaternion panelRotation = GetPanelRotation(side);
            yield return Panel.Show(panelPosition, panelRotation, panelScale, _frameSides[side]);

            Activating = false;
            _unrolledSide = side;
            _rollingOut = false;
            _unrolled = true;
        }

        private IEnumerator RollIn(FrameSide side)
        {
            _rollingIn = true;
            Vector3 tempScale = transform.localScale;
            Vector3 tempPosition = transform.localPosition;

            yield return Panel.Hide();
            yield return Toolbar.Deactivate();

            while (Vector3.Distance(tempScale, _baseScale) > THRESHOLD)
            {
                tempScale = Vector3.Lerp(tempScale, _baseScale, INTERP_FAC);
                tempPosition = Vector3.Lerp(tempPosition, _basePosition, INTERP_FAC);

                transform.localScale = tempScale;
                transform.localPosition = tempPosition;

                yield return new WaitForSeconds(0.01f);
            }

            Activating = false;
            transform.localScale = _baseScale;
            transform.localPosition = _basePosition;
            _rollingIn = false;
            _unrolled = false;
        }
    }
}
