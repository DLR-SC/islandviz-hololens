using HoloIslandVis.Controller;
using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.UI.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Info
{
    public enum PanelItem
    {
        None,
        BodyBundle,
        BodyPackage,
        BodyClass
    }

    public class InfoPanel : UIComponent
    {
        private const float THRESHOLD = 0.005f;
        private const float TILT_THRES = 30.0f;
        private const float INTERP_FAC = 0.25f;
        private const float ADJUST_TIME = 1.00f;

        private Vector3 _frameSide;
        private bool _isReorienting;

        public Canvas Canvas;
        public PanelBoard PanelBoard;
        public PanelHeader PanelHeader;
        public PanelBody PanelBody;

        void Start()
        {
            InvokeRepeating("CheckUserAngle", 0.3f, 0.3f);
            Canvas.gameObject.SetActive(false);
            _isReorienting = false;
        }

        public override IEnumerator Activate()
        {
            gameObject.SetActive(true);
            yield break;
        }

        public override IEnumerator Deactivate()
        {
            gameObject.SetActive(false);
            yield break;
        }

        public IEnumerator Show(Vector3 panelPosition, Quaternion panelRotation, float panelScale, Vector3 frameSide)
        {
            _frameSide = frameSide;
            yield return EnlargePanel(panelPosition, panelRotation, panelScale);
            yield return ShowContent(panelScale);
        }

        public IEnumerator Hide()
        {
            yield return HideContent();
            yield return ShrinkPanel();
        }

        private IEnumerator EnlargePanel(Vector3 panelPosition, Quaternion panelRotation, float panelScale)
        {
            transform.localPosition = panelPosition;
            transform.localRotation = panelRotation;

            gameObject.SetActive(true);
            float currentScale = 0.0f;
            while((panelScale - currentScale) > THRESHOLD)
            {
                currentScale = Mathf.Lerp(currentScale, panelScale, INTERP_FAC);
                PanelBoard.transform.localScale = new Vector3(1.0f, currentScale, 1.0f);
                yield return new WaitForSeconds(0.01f);
            }

            PanelBoard.transform.localScale = new Vector3(1.0f, panelScale, 1.0f);
        }

        private IEnumerator ShrinkPanel()
        {
            float currentScale = PanelBoard.transform.localScale.y;
            while (currentScale > THRESHOLD)
            {
                currentScale = Mathf.Lerp(currentScale, 0.0f, INTERP_FAC);
                PanelBoard.transform.localScale = new Vector3(1.0f, currentScale, 1.0f);
                yield return new WaitForSeconds(0.01f);
            }

            PanelBoard.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);

            CancelInvoke("CheckUserAngle");
            _isReorienting = false;

            gameObject.SetActive(false);
        }

        private IEnumerator ShowContent(float panelScale)
        {
            Canvas.transform.localPosition = GetCanvasPosition(panelScale);

            if(PanelBody.ActiveBody != PanelItem.None)
                PanelBody.Activate(PanelBody.ActiveBody);

            InvokeRepeating("CheckUserAngle", 0.3f, 0.3f);

            Context context = ContextManager.Instance.Context;
            PanelBody.Build(PanelBody.ActiveBody, context.Selected);
            Canvas.gameObject.SetActive(true);
            yield return null;
        }

        private IEnumerator HideContent()
        {
            if (PanelBody.ActiveBody != PanelItem.None)
                PanelBody.Deactivate(PanelBody.ActiveBody);

            Canvas.gameObject.SetActive(false);
            DestroyAllContent();
            yield return null;
        }

        private void CheckUserAngle()
        {
            if (_isReorienting)
                return;

            Vector3 gazePosition = ToolkitHelper.Instance.GazePosition;
            Vector3 panelDirection = (transform.position - gazePosition).normalized;
            Vector3 userDirection = -panelDirection;

            Vector3 planeNormal = transform.right;
            float dist = Vector3.Dot(userDirection, planeNormal);
            userDirection = userDirection - planeNormal * dist;
            userDirection = userDirection.normalized;

            // TODO Fix caliculation of reorientation.
            float deltaAngle = Vector3.Angle(userDirection, transform.forward) - 15.0f;
            if (Mathf.Abs(deltaAngle) > TILT_THRES)
                StartCoroutine(Reorient(userDirection));
        }

        private IEnumerator Reorient(Vector3 userDirection)
        {
            _isReorienting = true;

            float tiltAngle = Vector3.Angle(userDirection, _frameSide);
            tiltAngle = Mathf.Clamp(tiltAngle, 0, 90);

            Quaternion temp = transform.localRotation;
            Quaternion target = Quaternion.LookRotation(_frameSide, Vector3.up);
            target = target * Quaternion.Euler(-tiltAngle, 0, 0);

            while(Mathf.Abs(Quaternion.Angle(temp, target)) > 0.1f)
            {
                float currentTime = (Time.deltaTime / ADJUST_TIME);
                temp = Quaternion.SlerpUnclamped(temp, target, currentTime);
                transform.localRotation = temp;
                yield return null;
            }

            _isReorienting = false;
        }

        private void DestroyAllContent()
        {
            PanelBody.PanelBodyBundle.DestroyContent();
            PanelBody.PanelBodyPackage.DestroyContent();
            PanelBody.PanelBodyClass.DestroyContent();
        }

        private Vector3 GetCanvasPosition(float panelScale)
            => new Vector3(0.0f, panelScale - 0.5f, 0.01f);

        public void SetHeaderTextTop(string text)
        {
            PanelHeader.HeaderTextTop.text = text;
        }

        public void SetHeaderTextBottom(string text)
        {
            PanelHeader.HeaderTextBottom.text = text;
        }
    }
}
