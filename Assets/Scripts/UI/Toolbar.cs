using HoloIslandVis.Interaction;
using HoloToolkit.Unity.Buttons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public class Toolbar : UIComponent
    {
        private const float THRESHOLD = 0.001f;
        private const float INTERP_FAC = 0.5f;
        private const float ITER = 5;

        [Range(0.0f, 1.0f)] public float SlotWidth;

        public Dictionary<StaticItem, CompoundButton> ButtonDictionary;
        public List<CompoundButton> AllButtons;
        public List<CompoundButton> ActiveButtons;

        void Start()
        {
            ButtonDictionary = new Dictionary<StaticItem, CompoundButton>();
            ButtonDictionary.Add(StaticItem.Done,           AllButtons[0]);
            ButtonDictionary.Add(StaticItem.Adjust,         AllButtons[1]);
            ButtonDictionary.Add(StaticItem.Panel,          AllButtons[2]);
            ButtonDictionary.Add(StaticItem.Fit,            AllButtons[3]);
            ButtonDictionary.Add(StaticItem.Dependencies,   AllButtons[4]);
            Rearrange();
        }

        public void Refresh(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            transform.localScale = scale;
            transform.localRotation = rotation;
            transform.localPosition = position;
        }

        public void SetActiveButtons(params StaticItem[] activeButtons)
        {
            ActiveButtons.Clear();
            foreach (var button in ButtonDictionary.Values)
                button.gameObject.SetActive(false);

            foreach (StaticItem button in activeButtons)
            {
                ButtonDictionary[button].gameObject.SetActive(true);
                ActiveButtons.Add(ButtonDictionary[button]);
            }
        }

        public void Rearrange()
        {
            float menuWidth = SlotWidth * ActiveButtons.Count;
            float menuEdgeLeft = -(menuWidth / 2.0f);
            menuEdgeLeft += (SlotWidth / 2.0f);

            for (int i = 0; i < ActiveButtons.Count; i++)
            {
                CompoundButton button = ActiveButtons[i];
                Vector3 temp = button.transform.localPosition;
                temp.x = menuEdgeLeft + SlotWidth * i;
                button.transform.localPosition = temp;
            }
        }

        public override IEnumerator Activate()
        {
            yield return EnlargeButtons();
            yield return UnfoldButtons();
        }

        public override IEnumerator Deactivate()
        {
            yield return FoldButtons();
            yield return ShrinkButtons();
        }

        private IEnumerator UnfoldButtons()
        {
            float menuWidth = SlotWidth * ActiveButtons.Count;
            float menuEdgeLeft = -(menuWidth / 2.0f);
            menuEdgeLeft += (SlotWidth / 2.0f);

            List<Vector3> _targetPositions = new List<Vector3>();
            for (int i = 0; i < ActiveButtons.Count; i++)
            {
                Vector3 temp = Vector3.zero;
                temp.x = menuEdgeLeft + SlotWidth * i;
                _targetPositions.Add(temp);
            }

            for (int i = 0; i < ITER; i++)
            {
                for (int j = 0; j < ActiveButtons.Count; j++)
                {
                    CompoundButton button = ActiveButtons[j];
                    Vector3 tempPos = button.transform.localPosition;
                    tempPos = Vector3.Lerp(tempPos, _targetPositions[j], 0.5f);
                    button.transform.localPosition = tempPos;
                }

                yield return new WaitForSeconds(0.01f);
            }

            for (int i = 0; i < ActiveButtons.Count; i++)
                ActiveButtons[i].transform.localPosition = _targetPositions[i];
        }

        private IEnumerator EnlargeButtons()
        {
            float targetComp = 1.0f;

            gameObject.SetActive(true);

            for (int i = 0; i < ITER; i++)
            {
                foreach (CompoundButton button in ActiveButtons)
                {
                    Vector3 tempScale = button.transform.localScale;
                    float tempComp = button.transform.localScale.y;
                    tempComp = Mathf.Lerp(tempComp, targetComp, 0.5f);
                    tempScale = new Vector3(tempScale.x, tempComp, tempScale.z);
                    button.transform.localScale = tempScale;
                }

                yield return new WaitForSeconds(0.01f);
            }

            foreach (CompoundButton button in ActiveButtons)
                button.transform.localScale = Vector3.one;
        }

        private IEnumerator FoldButtons()
        {
            Vector3 targetPos = Vector3.zero;

            for (int i = 0; i < ITER; i++)
            {
                foreach (CompoundButton button in ActiveButtons)
                {
                    Vector3 tempPos = button.transform.localPosition;
                    tempPos = Vector3.Lerp(tempPos, targetPos, 0.5f);
                    button.transform.localPosition = tempPos;
                }

                yield return new WaitForSeconds(0.01f);
            }

            foreach (CompoundButton button in ActiveButtons)
                button.transform.localPosition = targetPos;
        }

        private IEnumerator ShrinkButtons()
        {
            float targetComp = 0.0f;

            for (int i = 0; i < ITER; i++)
            {
                foreach (CompoundButton button in ActiveButtons)
                {
                    Vector3 tempScale = button.transform.localScale;
                    float tempComp = button.transform.localScale.y;
                    tempComp = Mathf.Lerp(tempComp, targetComp, 0.5f);
                    tempScale = new Vector3(tempScale.x, tempComp, tempScale.z);
                    button.transform.localScale = tempScale;
                }

                yield return new WaitForSeconds(0.01f);
            }

            gameObject.SetActive(false);
        }
    }
}
