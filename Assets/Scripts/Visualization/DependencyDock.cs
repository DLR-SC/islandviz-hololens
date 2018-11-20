using HoloIslandVis.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DependencyDock : MonoBehaviour
{
    private GameObject dependencyContainer;
    private GameObject visualizationContainer;
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
        dependencyContainer = RuntimeCache.Instance.DependencyContainer;
        visualizationContainer = RuntimeCache.Instance.VisualizationContainer;

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
        foreach (DependencyDock dock in connectedDocks)
        {

            //Check if Arrow already exists
            IDPair pair = new IDPair(this.GetInstanceID(), dock.GetInstanceID());
            GameObject conArrow = connectionPool.getConnection(pair);
            if (conArrow == null)
            {
                GameObject arrowBody;
                if (dockType == DockType.Import)
                    arrowBody = Instantiate(importArrowPrefab, transform.position, Quaternion.identity);
                else
                    arrowBody = Instantiate(exportArrowPrefab, transform.position, Quaternion.identity);

                GameObject arrowHead = Instantiate(arrowHeadPrefab, transform.position, Quaternion.identity);
                conArrow = new GameObject();
                conArrow.name = "Connection To " + dock.gameObject.name;

                #region adjust transform
                Vector3 dirVec = dock.transform.position - transform.position;
                //Vector3 dirVecNorm = dirVec.normalized;
                //float distance = dirVec.magnitude;

                //arrowBody.transform.position = arrowBody.transform.position + dirVecNorm * (distance / 2);
                //Vector3 newScale = new Vector3(distance, distance, 0.02f * 20f * dockWeights[cc]);
                //arrowBody.transform.localScale = newScale;

                //float angle = Vector3.Angle(visualizationContainer.transform.right, dirVecNorm);
                //Vector3 cross = Vector3.Cross(visualizationContainer.transform.right, dirVecNorm);
                //if (cross.y < 0)
                //    angle = -angle;

                //arrowBody.transform.Rotate(visualizationContainer.transform.up, angle);

                //arrowHead.transform.parent = conArrow.transform;
                //arrowBody.transform.parent = conArrow.transform;
                //conArrow.transform.parent = dependencyContainer.transform;

                dirVec.y = 0;
                float distance = dirVec.magnitude;
                float sDWidth = gameObject.GetComponent<Collider>().bounds.extents.x;
                float tDWidth = dock.gameObject.GetComponent<Collider>().bounds.extents.x;
                float aWidth = 0.02f * 20f * dockWeights[cc]; // Do not hardcode arrow width
                float connectionLength = distance - (sDWidth + tDWidth);
                Vector3 newScale = new Vector3(connectionLength, connectionLength, 0.02f * 20f * dockWeights[cc]); // Do not hardcode arrow width
                arrowBody.transform.localScale = newScale;

                #region Arrowhead
                newScale.x = 0.02f * 20f * dockWeights[cc];
                newScale.y = 0.01f;
                arrowHead.transform.localScale = newScale;
                if (dockType == DockType.Import)
                {
                    arrowHead.transform.position += new Vector3(-connectionLength * 0.5f, 0f, 0f);
                    arrowHead.transform.localEulerAngles = new Vector3(0f, 180f, -39f);
                }
                else
                {
                    arrowHead.transform.position += new Vector3(connectionLength * 0.5f, 0f, 0f);
                    arrowHead.transform.localEulerAngles = new Vector3(0f, 0f, -39f);
                }

                arrowHead.transform.parent = conArrow.transform;
                #endregion

                arrowBody.transform.parent = conArrow.transform;
                float maxHeight = Mathf.Max(gameObject.GetComponent<Collider>().bounds.extents.y, dock.gameObject.GetComponent<Collider>().bounds.extents.y);
                conArrow.transform.position += new Vector3((connectionLength / 2f), maxHeight, 0);

                conArrow.transform.parent = rotPivot.transform;
                float angle = Vector3.Angle(Vector3.right, dirVec / distance);
                Vector3 cross = Vector3.Cross(Vector3.right, dirVec / distance);
                if (cross.y < 0) angle = -angle;
                rotPivot.transform.Rotate(Vector3.up, angle);
                conArrow.transform.parent = null;
                conArrow.transform.parent = dependencyContainer.transform;
                rotPivot.transform.Rotate(Vector3.up, -angle);
                #endregion
                conArrow.SetActive(false);
                connectionPool.AddConnection(pair, conArrow);
            }
            connectionArrows.Add(conArrow);
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
}
