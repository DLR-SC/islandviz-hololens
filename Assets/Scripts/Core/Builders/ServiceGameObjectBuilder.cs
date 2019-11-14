using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Model.OSGi;
using HoloIslandVis.Model.OSGi.Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HoloIslandVis.Core.Builders
{
    public class ServiceGameObjectBuilder : SingletonComponent<ServiceGameObjectBuilder>
    {
        public const float SERVICE_NODE_SIZE = 0.025f * IslandStructureBuilder.CELL_SCALE;
        public const int GROUPS_PER_SLICE = 4;
        public const float STARTING_HEIGHT = 1.20f;
        public const float HEIGHT_STEP = 0.02f;

        public delegate void ServiceGameObjectsBuiltHandler(ServiceGameObjectsBuiltEventArgs eventArgs);
        public event ServiceGameObjectsBuiltHandler ServiceGameObjectsBuilt = delegate { };

        private ServiceVolume serviceVolume;
        public GameObject ServiceSliceContainer;

        private Material defaultMaterial;
        private GameObject VisualizationContainer;

        private GameObject interfacePrefab;
        private GameObject referencePrefab;
        private GameObject implementationPrefab;

        // Use this for initialization
        void Start()
        {
            defaultMaterial = (Material)Resources.Load("Materials/Diffuse_White");
            interfacePrefab = (GameObject)Resources.Load("Prefabs/ServiceInterfaceNode");
            referencePrefab = (GameObject)Resources.Load("Prefabs/ServiceReferenceNode");
            implementationPrefab = (GameObject)Resources.Load("Prefabs/ServiceImplementationNode");
            ServiceSliceContainer.gameObject.SetActive(true);
        }

        public IEnumerator Construct(List<Service> services, List<Island> Islands)
        {
            //serviceVolume = new ServiceVolume();
            //yield return StartCoroutine(constructAll(services, Islands));
            ServiceGameObjectsBuilt(new ServiceGameObjectsBuiltEventArgs(Islands));
            yield return null;
        }

        private Dictionary<ServiceSlice, List<Service>> distributeServicesToSlices(List<Service> services)
        {
            Dictionary<ServiceSlice, List<Service>> result = new Dictionary<ServiceSlice, List<Service>>();

            int sliceNumber = services.Count / GROUPS_PER_SLICE;

            for (int i = 0; i < sliceNumber; i++)
            {
                GameObject serviceSlice = new GameObject("ServiceSlice");
                float height = STARTING_HEIGHT + HEIGHT_STEP * i;
                serviceSlice.transform.position = new Vector3(0f, height, 0f);
                serviceSlice.transform.SetParent(ServiceSliceContainer.transform);
                ServiceSlice sliceComponent = serviceSlice.AddComponent<ServiceSlice>();
                sliceComponent.height = height;

                List<Service> currentServiceList = new List<Service>();
                for (int s = 0; s < GROUPS_PER_SLICE; s++)
                {
                    int sIdx = GROUPS_PER_SLICE * i + s;
                    if (sIdx < services.Count)
                    {
                        currentServiceList.Add(services[sIdx]);
                    }
                }
                result.Add(sliceComponent, currentServiceList);
            }

            return result;
        }

        IEnumerator constructAll(List<Service> services, List<Island> Islands)
        {
            Dictionary<ServiceSlice, List<Service>> serviceSliceMap = distributeServicesToSlices(services);
            foreach (KeyValuePair<ServiceSlice, List<Service>> kvp in serviceSliceMap)
            {
                constructServicesAndComponents(kvp.Value, kvp.Key);
                yield return null;
            }

            foreach (Island Island in Islands)
            {
                if (Island == null)
                    continue;
                else
                {
                    if (!Island.IsIslandEmpty())
                    {
                        foreach (Region region in Island.Regions)
                            foreach (Building b in region.Buildings)
                            {
                                ServiceLayerGO serviceLayer = b.GetComponent<ServiceLayerGO>();
                                if (serviceLayer != null)
                                {
                                    serviceLayer.createDownwardConnections();

                                    List<ServiceNodeScript> snsList = serviceLayer.getServiceNodes();
                                    foreach (ServiceNodeScript sns in snsList)
                                        sns.constructServiceConnections();
                                }
                            }
                    }
                }
            }
            ServiceGameObjectsBuilt(new ServiceGameObjectsBuiltEventArgs(Islands));
        }

        private void constructServicesAndComponents(List<Service> services, ServiceSlice serviceSlice)
        {
            foreach (Service service in services)
            {
                CompilationUnit serviceCU = service.LinkedCompilationUnit;
                if (serviceCU != null && serviceCU.GameObject != null)
                {

                    GameObject serviceGO = Instantiate(interfacePrefab, transform.position, Quaternion.identity);
                    //serviceGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                    serviceGO.name = service.Name;
                    Vector3 cuPosition = serviceCU.GameObject.transform.position;
                    cuPosition.y = 0f;
                    serviceGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                    serviceGO.transform.localScale = new Vector3(SERVICE_NODE_SIZE, SERVICE_NODE_SIZE, SERVICE_NODE_SIZE);
                    serviceGO.transform.SetParent(serviceSlice.transform);
                    ServiceNodeScript sns = serviceGO.AddComponent<ServiceNodeScript>();
                    //serviceGO.AddComponent<TextLabelComponent>();
                    ServiceLayerGO slGO = serviceCU.GameObject.GetComponent<ServiceLayerGO>();
                    slGO.addServiceNode(sns);
                    #region construct ServiceComponents
                    List<ServiceComponent> implementingComponents = service.ImplementingComponents;
                    List<ServiceComponent> referencingComponents = service.ReferencingComponents;
                    foreach (ServiceComponent sc in implementingComponents)
                    {
                        CompilationUnit componentCU = sc.ImplCompilationUnit;
                        GameObject scGO = Instantiate(implementationPrefab, transform.position, Quaternion.identity);
                        //scGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                        scGO.name = sc.Name;
                        cuPosition = componentCU.GameObject.transform.position;
                        cuPosition.y = 0f;
                        scGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                        scGO.transform.localScale = new Vector3(SERVICE_NODE_SIZE, SERVICE_NODE_SIZE, SERVICE_NODE_SIZE);
                        scGO.transform.SetParent(serviceSlice.transform);
                        ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                        //scGO.AddComponent<TextLabelComponent>();
                        scGOcomponent.addConnectedServiceNode(sns);
                        sns.addConnectedServiceNode(scGOcomponent);
                        scGOcomponent.disableServiceNode();
                        ServiceLayerGO sl = componentCU.GameObject.GetComponent<ServiceLayerGO>();
                        sl.addServiceNode(scGOcomponent);
                    }
                    foreach (ServiceComponent sc in referencingComponents)
                    {
                        CompilationUnit componentCU = sc.ImplCompilationUnit;
                        GameObject scGO = Instantiate(referencePrefab, transform.position, Quaternion.identity);
                        //scGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                        scGO.name = sc.Name;
                        cuPosition = componentCU.GameObject.transform.position;
                        cuPosition.y = 0f;
                        scGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                        scGO.transform.localScale = new Vector3(SERVICE_NODE_SIZE, SERVICE_NODE_SIZE, SERVICE_NODE_SIZE);
                        scGO.transform.SetParent(serviceSlice.transform);
                        ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                        //scGO.AddComponent<TextLabelComponent>();
                        scGOcomponent.addConnectedServiceNode(sns);
                        sns.addConnectedServiceNode(scGOcomponent);

                        scGOcomponent.disableServiceNode();
                        ServiceLayerGO sl = componentCU.GameObject.GetComponent<ServiceLayerGO>();
                        sl.addServiceNode(scGOcomponent);
                    }
                    #endregion
                    sns.disableServiceNode();
                }
            }
        }
    }
}