using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HoloIslandVis.Component.UI
{
    public class UserInterface : SingletonComponent<UserInterface>
    {
        public GameObject ContentSurface { get; private set; }
        public GameObject ScanInstructionText { get; private set; }
        public GameObject ScanProgressBar { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            ContentSurface = GameObject.Find("ContentSurface");
            ScanInstructionText = GameObject.Find("ScanInstructionText");
            ScanProgressBar = GameObject.Find("ScanProgressBar");

            ContentSurface.SetActive(false);
            ScanInstructionText.SetActive(false);
            ScanProgressBar.SetActive(false);

        }
    }
}
