using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloIslandVis.Core.Builders;

namespace HoloIslandVis.Model.OSGi.Services
{
    public class ServiceLayerGO : MonoBehaviour {

        public static float TABLE_HEIGHT = 1.25f;

        public bool simulateClickWithLongTouch = false;

        public List<ServiceNodeScript> serviceNodes;
        public List<GameObject> downwardConnections;
        private bool expanded = false;

        private GameObject DownwardConnectionContainer;
        private GameObject connectionPrefab;

        void Awake()
        {
            serviceNodes = new List<ServiceNodeScript>();
            downwardConnections = new List<GameObject>();
            connectionPrefab = (GameObject)Resources.Load("Prefabs/ServiceConnection");
            DownwardConnectionContainer = GameObject.Find("DownwardConnectionContainer");
        }

        void Start()
        {
            //Create Spieß

        }

        //Should be called after all serviceNodes are added
        public void createDownwardConnections()
        {
            if (serviceNodes.Count > 0)
            {
                //Order ascending... no need as it is implicitly given through construction apparantely
                //serviceNodes.OrderBy(x => x.height);
                for (int i = 0; i < serviceNodes.Count; i++)
                {
                    float length;
                    if (i == 0)
                        length = serviceNodes[i].transform.parent.gameObject.GetComponent<ServiceSlice>().height - TABLE_HEIGHT;
                    else
                    {
                        float sliceHeightPrevious = serviceNodes[i - 1].transform.parent.gameObject.GetComponent<ServiceSlice>().height - TABLE_HEIGHT;
                        float sliceHeightCurrent = serviceNodes[i].transform.parent.gameObject.GetComponent<ServiceSlice>().height - TABLE_HEIGHT;
                        length = sliceHeightCurrent - sliceHeightPrevious ;
                    }

                    GameObject connectionGO = Instantiate(connectionPrefab, serviceNodes[i].transform.position, Quaternion.identity);
                    connectionGO.name = "Downward Connection";
                    #region adjust transform
                    Vector3 newScale = new Vector3(ServiceGameObjectBuilder.SERVICE_NODE_SIZE * 0.25f, length, ServiceGameObjectBuilder.SERVICE_NODE_SIZE * 0.25f);
                    connectionGO.transform.localScale = newScale;
                    connectionGO.transform.position += new Vector3(0, -length / 2f, 0);
                    connectionGO.transform.parent = DownwardConnectionContainer.transform;
                    #endregion
                    connectionGO.SetActive(false);
                    downwardConnections.Add(connectionGO);
                }
            }
        }
        
        public void addServiceNode(ServiceNodeScript node)
        {
            serviceNodes.Add(node);
            node.setEmitterParent(this);
        }

        public List<ServiceNodeScript> getServiceNodes()
        {
            return serviceNodes;
        }

        public void expandNodes()
        {
            expanded = true;
            foreach (ServiceNodeScript node in serviceNodes)
                node.enableServiceNode();
            foreach (GameObject downConnection in downwardConnections)
                downConnection.SetActive(true);
        }
        /*
        public void expandNodesWithActivation()
        {
            expanded = true;
            foreach (ServiceNodeScript node in serviceNodes)
            {
                node.enableServiceNode();
                node.expandAll();
            }
            foreach (GameObject downConnection in downwardConnections)
                downConnection.SetActive(true);
            
        }
        */
        public void contractNodes()
        {
            expanded = false;
            foreach (ServiceNodeScript node in serviceNodes)
            {
                node.disableServiceNode();
            }
            foreach (GameObject downConnection in downwardConnections)
                downConnection.SetActive(false);
        }

        //private void handleActivationDeactivation(Hand hand)
        //{
        //    if (!expanded)
        //        expandNodes();
        //    else
        //        contractNodes();
        //}
    }
}
