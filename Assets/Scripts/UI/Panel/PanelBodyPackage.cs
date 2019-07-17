using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HoloIslandVis.UI.Info
{
    public class PanelBodyPackage : MonoBehaviour
    {
        private const float INIT_HEAD_POS = -60;
        private const float BODY_SEP_MARGIN = 50;
        private const float SECT_SEP_MARGIN = 5;

        private float _head;
        private Island _bundle;
        private Region _package;

        public Font SelawkBold;
        public Font SelawkLight;

        public Text BundleSectionHead;
        public Text BundleSectionBody;

        public Text PackageSectionHead;
        public Text PackageSectionBody;

        public Text ClassSectionHead;
        public Text ClassSectionBody;

        public void Build(Interactable package)
        {
            _head = INIT_HEAD_POS;
            _package = package.GetComponent<Region>();
            _bundle = _package.Island;

            BuildBundleSection(BundleSectionHead, BundleSectionBody);
            BuildPackageSection(PackageSectionHead, PackageSectionBody);
            BuildClassSection(ClassSectionHead, ClassSectionBody);
        }

        private void BuildBundleSection(Text bundleSectionHead, Text bundleSectionBody)
        {
            bundleSectionHead.text = "Bundle";
            bundleSectionHead.transform.localPosition = new Vector3(0, _head, 0);
            _head -= bundleSectionHead.rectTransform.rect.height;

            bundleSectionBody.text = _bundle.name;
            bundleSectionBody.transform.localPosition = new Vector3(0, _head, 0);
            _head -= bundleSectionBody.rectTransform.rect.height;
            _head -= BODY_SEP_MARGIN;
        }

        private void BuildPackageSection(Text packageSectionHead, Text packageSectionBody)
        {
            packageSectionHead.text = "Package";
            packageSectionHead.transform.localPosition = new Vector3(0, _head, 0);
            _head -= packageSectionHead.rectTransform.rect.height;

            packageSectionBody.text = _package.name;
            packageSectionBody.transform.localPosition = new Vector3(0, _head, 0);
            _head -= packageSectionBody.rectTransform.rect.height;
            _head -= BODY_SEP_MARGIN;
        }

        private void BuildClassSection(Text classSectionHead, Text classSectionBody)
        {
            var compilationUnits = _package.Buildings;
            var text = classSectionHead.GetComponentsInChildren<Text>();

            text[1].text = "Compilation Units";
            text[2].text = compilationUnits.Count.ToString();

            classSectionHead.transform.localPosition = new Vector3(0, _head, 0);
            _head -= classSectionHead.rectTransform.rect.height;

            foreach (Building compilationUnit in compilationUnits)
            {
                GameObject classTextContainer = new GameObject("Text_" + compilationUnit.name);
                Text classText = classTextContainer.AddComponent<Text>();
                classText.transform.SetParent(classSectionBody.transform);

                classText.rectTransform.sizeDelta = new Vector2(920, 42);
                classText.rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
                classText.rectTransform.anchorMin = new Vector2(0.5f, 1.0f);
                classText.rectTransform.pivot = new Vector2(0.5f, 1.0f);
                classText.rectTransform.localRotation = Quaternion.identity;
                classText.rectTransform.localScale = Vector3.one;
                classText.text = compilationUnit.name;
                classText.font = SelawkLight;
                classText.color = Color.white;
                classText.fontSize = 35;

                classText.transform.localPosition = new Vector3(0, _head, 0);
                _head -= classText.rectTransform.rect.height;
            }
        }

        public void DestroyContent()
        {
            var classSectionContent = ClassSectionBody.GetComponentsInChildren<Text>();

            for (int i = 1; i < classSectionContent.Length; i++)
                Destroy(classSectionContent[i].gameObject);
        }
    }
}
