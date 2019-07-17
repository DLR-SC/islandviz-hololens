using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Core.Metaphor
{ 
    public class DependencyDock : MonoBehaviour
    {
        private DependencyContainer dependencyContainer;
        private ConnectionPool connectionPool;
        private GameObject rotPivot;
        private GameObject importArrowPrefab;
        private GameObject exportArrowPrefab;
        private GameObject arrowHeadPrefab;
        private List<GameObject> connectionArrows;
        private List<DependencyDock> connectedDocks;
        private List<float> dockWeights;

        //If not set, defaults to ImportDock
        private DockType dockType;
        public bool expanded;

        public DockType DockType { get; set; }

        void Awake()
        {
            connectionArrows = new List<GameObject>();
            connectedDocks = new List<DependencyDock>();
            dockWeights = new List<float>();

            dependencyContainer = UIManager.Instance.DependencyContainer;

            expanded = false;
            dockType = DockType.Import;
            importArrowPrefab = (GameObject)Resources.Load("Prefabs/ImportArrow");
            exportArrowPrefab = (GameObject)Resources.Load("Prefabs/ExportArrow");
            arrowHeadPrefab = (GameObject)Resources.Load("Prefabs/ArrowHead");
            rotPivot = new GameObject("Rotation Pivot");
            rotPivot.transform.position = transform.position;
            rotPivot.transform.SetParent(transform);

            ConnectionPool[] pools = FindObjectsOfType(typeof(ConnectionPool)) as ConnectionPool[];
            if (pools.Length == 1)
                connectionPool = pools[0];
            else
                throw new Exception("No connection pool component found, or too many connection pools! There can only be one.");
        }

        public void setDockType(DockType type)
        {
            dockType = type;
        }

        public void addDockConnection(DependencyDock dock, float w)
        {
            connectedDocks.Add(dock);
            dockWeights.Add(w);
        }

        public void constructConnectionArrows()
        {
            //Construct new Arrows
            int cc = 0;
            foreach (DependencyDock otherDock in connectedDocks)
            {
                //Check if Arrow already exists
                IDPair pair = new IDPair(this.GetInstanceID(), otherDock.GetInstanceID());
                GameObject connectionArrow = connectionPool.GetConnection(pair);

                if (connectionArrow == null)
                {
                    GameObject arrowBody;
                    if (dockType == DockType.Import)
                    {
                        arrowBody = Instantiate(importArrowPrefab, transform.position, Quaternion.identity);
                        //var renderer = arrowBody.GetComponent<MeshRenderer>();
                        //renderer.sharedMaterial = VisualizationLoader.Instance.ImportArrowMaterial;
                    }
                    else
                    {
                        arrowBody = Instantiate(exportArrowPrefab, transform.position, Quaternion.identity);
                        //var renderer = arrowBody.GetComponent<MeshRenderer>();
                        //renderer.sharedMaterial = VisualizationLoader.Instance.ExportArrowMaterial;
                    }

                    GameObject arrowHead = Instantiate(arrowHeadPrefab, transform.position, Quaternion.identity);
                    //var headRenderer = arrowBody.GetComponent<MeshRenderer>();
                    //headRenderer.sharedMaterial = VisualizationLoader.Instance.ArrowHeadMaterial;

                    connectionArrow = new GameObject();
                    connectionArrow.name = "Connection from " + gameObject.name + " to " + otherDock.gameObject.name;
                    connectionArrow.gameObject.AddComponent<DependencyArrow>();
                    arrowHead.name = connectionArrow.name + "_ArrowHead";
                    arrowBody.name = connectionArrow.name + "_ArrowBody";

                    connectionArrow.transform.parent = dependencyContainer.transform;
                    arrowHead.transform.parent = connectionArrow.transform;
                    arrowHead.transform.localPosition = Vector3.zero;

                    arrowBody.transform.parent = connectionArrow.transform;
                    arrowBody.transform.localPosition = Vector3.zero;

                    connectionArrow.transform.localScale = Vector3.one;
                    connectionArrow.transform.rotation = Quaternion.identity;

                    #region adjust transform

                    Vector3 dockLocalPos = transform.localPosition;
                    Vector3 otherDockLocalPos = otherDock.transform.localPosition;
                    Vector3 islandPos = transform.parent.localPosition;
                    Vector3 otherIslandPos = otherDock.transform.parent.localPosition;

                    Vector3 dockPos = islandPos + dockLocalPos;
                    Vector3 otherDockPos = otherIslandPos + otherDockLocalPos;

                    Vector3 direction = otherDockPos - dockPos;

                    direction.y = 0.0f;
                    double distance = direction.magnitude;

                    Vector3 connectionArrowPos = dockPos + (direction / 2);
                    connectionArrow.transform.localPosition = connectionArrowPos;

                    double angle = Vector3.Angle(Vector3.right, direction.normalized);
                    Vector3 cross = Vector3.Cross(Vector3.right, direction.normalized);
                    if (cross.y < 0) angle = -angle;

                    connectionArrow.transform.localRotation = Quaternion.Euler(0, (float)angle, 0);

                    float sDWidth = gameObject.GetComponent<Collider>().bounds.extents.magnitude;
                    float tDWidth = otherDock.gameObject.GetComponent<Collider>().bounds.extents.magnitude;
                    float cumulativeWidth = (sDWidth + tDWidth) * 30;
                    float conLength = ((float)distance - cumulativeWidth) - 0.5f;

                    Vector3 arrowBodyScale = new Vector3(conLength, conLength, arrowBody.transform.localScale.z);
                    arrowBody.transform.localScale = arrowBodyScale;

                    float maxHeight = Mathf.Max(gameObject.GetComponent<Collider>().bounds.extents.y, otherDock.gameObject.GetComponent<Collider>().bounds.extents.y);
                    connectionArrow.transform.localPosition += new Vector3(0, maxHeight + cumulativeWidth + 0.7f, 0);

                    #region Arrowhead
                    arrowBodyScale.x = 0.02f * 30f * dockWeights[cc];
                    arrowBodyScale.y = 0.01f;
                    arrowHead.transform.localScale = arrowBodyScale;
                    if (dockType == DockType.Import)
                    {
                        arrowHead.transform.localPosition += new Vector3(-conLength * 0.5f, 0f, 0f);
                        arrowHead.transform.localEulerAngles = new Vector3(0f, 180f, -39f);
                    }
                    else
                    {
                        arrowHead.transform.localPosition += new Vector3(conLength * 0.5f, 0f, 0f);
                        arrowHead.transform.localEulerAngles = new Vector3(0f, 0f, -39f);
                    }
                    #endregion
                    #endregion


                    //connectionArrow.AddComponent<ObjectStateSynchronizer>().SyncTransform = true;
                    //arrowhead.addcomponent<objectstatesynchronizer>().synctransform = true;
                    //arrowbody.addcomponent<objectstatesynchronizer>().synctransform = true;

                    connectionPool.AddConnection(pair, connectionArrow);
                    connectionArrow.SetActive(false);
                }
                connectionArrows.Add(connectionArrow);
                cc++;
            }

        }

        public void hideAllDependencies()
        {
            expanded = false;
            foreach (GameObject arrow in connectionArrows)
                arrow.SetActive(false);
        }

        public void showAllDependencies()
        {
            expanded = true;
            foreach (GameObject arrow in connectionArrows)
                arrow.SetActive(true);
        }

        public List<DependencyDock> GetConnectedDocks()
            => connectedDocks;
    }
}
