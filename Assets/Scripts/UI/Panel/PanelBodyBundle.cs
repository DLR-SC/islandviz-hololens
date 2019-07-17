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
    public class PanelBodyBundle : MonoBehaviour
    {
        private const float INIT_HEAD_POS = -60;
        private const float BODY_SEP_MARGIN = 50;
        private const float SECT_SEP_MARGIN = 5;

        private float _head;
        private Island _bundle;

        public Font SelawkBold;
        public Font SelawkLight;

        public Text BundleSectionHead;
        public Text BundleSectionBody;

        public Text PackageSectionHead;
        public Text PackageSectionBody;

        public Text ExportSectionHead;
        public Text ExportSectionBody;

        public Text ImportSectionHead;
        public Text ImportSectionBody;

        public void Build(Interactable bundle)
        {
            _head = INIT_HEAD_POS;
            _bundle = bundle.GetComponent<Island>();

            // Bundle section
            BuildBundleSection(BundleSectionHead, BundleSectionBody);
            BuildPackageSection(PackageSectionHead, PackageSectionBody);
            BuildExportSection(ExportSectionHead, ExportSectionBody);
            BuildImportSection(ImportSectionHead, ImportSectionBody);
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
            var packages = _bundle.Regions;
            var text = packageSectionHead.GetComponentsInChildren<Text>();

            text[1].text = "Packages";
            text[2].text = packages.Count.ToString();

            packageSectionHead.transform.localPosition = new Vector3(0, _head, 0);
            _head -= packageSectionHead.rectTransform.rect.height;

            foreach (Region package in packages)
            {
                GameObject packageTextContainer = new GameObject("Text_" + package.name);
                Text packageText = packageTextContainer.AddComponent<Text>();
                packageText.transform.SetParent(packageSectionBody.transform);

                packageText.rectTransform.sizeDelta = new Vector2(920, 42);
                packageText.rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
                packageText.rectTransform.anchorMin = new Vector2(0.5f, 1.0f);
                packageText.rectTransform.pivot = new Vector2(0.5f, 1.0f);
                packageText.rectTransform.localRotation = Quaternion.identity;
                packageText.rectTransform.localScale = Vector3.one;
                packageText.text = package.name;
                packageText.font = SelawkLight;
                packageText.color = Color.white;
                packageText.fontSize = 35;

                packageText.transform.localPosition = new Vector3(0, _head, 0);
                _head -= packageText.rectTransform.rect.height;
            }

            _head -= BODY_SEP_MARGIN;
        }

        private void BuildExportSection(Text exportSectionHead, Text exportSectionBody)
        {
            var exportDock = _bundle.ExportDock.GetComponent<DependencyDock>();
            var connectedExportDocks = exportDock.GetConnectedDocks();
            var text = exportSectionHead.GetComponentsInChildren<Text>();

            text[1].text = "Exports To";
            text[2].text = connectedExportDocks.Count.ToString();

            exportSectionHead.transform.localPosition = new Vector3(0, _head, 0);
            _head -= exportSectionHead.rectTransform.rect.height;

            foreach (DependencyDock connectedExportDock in connectedExportDocks)
            {
                var connectedBundle = connectedExportDock.GetComponentInParent<Island>();
                GameObject exportTextContainer = new GameObject("Text_" + connectedBundle.name);
                Text export = exportTextContainer.AddComponent<Text>();
                export.transform.SetParent(exportSectionBody.transform);

                export.rectTransform.sizeDelta = new Vector2(920, 42);
                export.rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
                export.rectTransform.anchorMin = new Vector2(0.5f, 1.0f);
                export.rectTransform.pivot = new Vector2(0.5f, 1.0f);
                export.rectTransform.localRotation = Quaternion.identity;
                export.rectTransform.localScale = Vector3.one;
                export.text = connectedBundle.name;
                export.font = SelawkLight;
                export.color = Color.white;
                export.fontSize = 35;

                export.transform.localPosition = new Vector3(0, _head, 0);
                _head -= export.rectTransform.rect.height;
            }

            _head -= BODY_SEP_MARGIN;
        }

        private void BuildImportSection(Text importSectionHead, Text importSectionBody)
        {
            var importDock = _bundle.ImportDock.GetComponent<DependencyDock>();
            var connectedExportDocks = importDock.GetConnectedDocks();
            var text = importSectionHead.GetComponentsInChildren<Text>();

            text[1].text = "Imports From";
            text[2].text = connectedExportDocks.Count.ToString();

            importSectionHead.transform.localPosition = new Vector3(0, _head, 0);
            _head -= importSectionHead.rectTransform.rect.height;

            foreach (DependencyDock connectedImportDock in connectedExportDocks)
            {
                var connectedBundle = connectedImportDock.GetComponentInParent<Island>();
                GameObject importTextContainer = new GameObject("Text_" + connectedBundle.name);
                Text import = importTextContainer.AddComponent<Text>();
                import.transform.SetParent(importSectionBody.transform);

                import.rectTransform.sizeDelta = new Vector2(920, 42);
                import.rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
                import.rectTransform.anchorMin = new Vector2(0.5f, 1.0f);
                import.rectTransform.pivot = new Vector2(0.5f, 1.0f);
                import.rectTransform.localRotation = Quaternion.identity;
                import.rectTransform.localScale = Vector3.one;
                import.text = connectedBundle.name;
                import.font = SelawkLight;
                import.color = Color.white;
                import.fontSize = 35;

                import.transform.localPosition = new Vector3(0, _head, 0);
                _head -= import.rectTransform.rect.height;
            }
        }

        public void DestroyContent()
        {
            var packageSectionContent = PackageSectionBody.GetComponentsInChildren<Text>();
            var exportSectionContent = ExportSectionBody.GetComponentsInChildren<Text>();
            var importSectionContent = ImportSectionBody.GetComponentsInChildren<Text>();

            for (int i = 1; i < packageSectionContent.Length; i++)
                Destroy(packageSectionContent[i].gameObject);

            for (int i = 1; i < exportSectionContent.Length; i++)
                Destroy(exportSectionContent[i].gameObject);

            for (int i = 1; i < importSectionContent.Length; i++)
                Destroy(importSectionContent[i].gameObject);
        }
    }
}
