using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Sharing;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using UnityEngine;

namespace HoloIslandVis.Core.Builders
{
    public class SyncObjectBuilder : SingletonComponent<SyncObjectBuilder>
    {
        public delegate void SyncObjectsBuiltHandler();
        public event SyncObjectsBuiltHandler SyncObjectsBuilt = delegate { };

        public IEnumerator BuildSyncObjects()
        {
            //Visualization visualization = UIManager.Instance.Visualization;
            //visualization.gameObject.AddComponent<ObjectStateSynchronizer>();

            DependencyContainer dependencyContainer = UIManager.Instance.DependencyContainer;
            var dependencies = dependencyContainer.GetComponentsInChildren<DependencyArrow>();
            yield return null;

            for (int i = 0; i < dependencies.Length; i++)
            {
                //dependencies[i].gameObject.AddComponent<ObjectEnableSynchronizer>();
                dependencies[i].gameObject.SetActive(false);
            }

            SyncObjectsBuilt();
            yield return null;
        }
    }
}
