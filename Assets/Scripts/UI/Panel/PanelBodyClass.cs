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
    public class PanelBodyClass : MonoBehaviour
    {
        private const float INIT_HEAD_POS = -60;
        private const float BODY_SEP_MARGIN = 50;
        private const float SECT_SEP_MARGIN = 5;

        private float _head;
        private Island _bundle;
        private Region _package;
        private Building _class;

        public Font SelawkBold;
        public Font SelawkLight;

        public Text BundleSectionHead;
        public Text BundleSectionBody;

        public Text PackageSectionHead;
        public Text PackageSectionBody;

        public Text ClassSectionHead;
        public Text ClassSectionBody;

        public void Build(Interactable compilationUnit)
        {
            _head = INIT_HEAD_POS;
            _class = compilationUnit.GetComponent<Building>();
            _package = _class.Region;
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
            classSectionHead.text = "Compilation Unit";
            classSectionHead.transform.localPosition = new Vector3(0, _head, 0);
            _head -= classSectionHead.rectTransform.rect.height;

            var compilationUnit = _class.CompilationUnit;
            var uiText = classSectionBody.GetComponentsInChildren<Text>();

            uiText[1].text = compilationUnit.Name;
            uiText[1].transform.localPosition = new Vector3(0, _head, 0);
            _head -= uiText[1].rectTransform.rect.height + 100;

            uiText[3].text = "Lines Of Code";
            uiText[4].text = compilationUnit.LinesOfCode.ToString();
            uiText[2].transform.localPosition = new Vector3(0, _head, 0);
            _head -= uiText[2].rectTransform.rect.height;

            uiText[6].text = "Access Modifier";
            uiText[7].text = compilationUnit.AccessModifier.ToString();
            uiText[5].transform.localPosition = new Vector3(0, _head, 0);
            _head -= uiText[5].rectTransform.rect.height;

            uiText[9].text = "Type";
            uiText[10].text = compilationUnit.Type.ToString();
            uiText[8].transform.localPosition = new Vector3(0, _head, 0);
            _head -= uiText[8].rectTransform.rect.height;
        }

        public void DestroyContent()
        {

        }
    }
}