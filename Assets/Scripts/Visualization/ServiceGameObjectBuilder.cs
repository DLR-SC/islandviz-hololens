using HoloIslandVis.OSGiParser;
using HoloIslandVis.Visualization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceGameObjectBuilder : MonoBehaviour
{

    //private ServiceVolume serviceVolume;

    private Material defaultMaterial;
    private GameObject VisualizationContainer;
    private GameObject ServiceSliceContainer;

    private GameObject interfacePrefab;
    private GameObject referencePrefab;
    private GameObject implementationPrefab;

    // Use this for initialization
    //void start()
    //{
        
    //    defaultmaterial = (material)resources.load("materials/diffuse_white");
    //    interfaceprefab = (gameobject)resources.load("prefabs/serviceinterfacenode");
    //    referenceprefab = (gameobject)resources.load("prefabs/servicereferencenode");
    //    implementationprefab = (gameobject)resources.load("prefabs/serviceimplementationnode");

    //    serviceslicecontainer = gameobject.find("datamanager").getcomponent<globalcontainerholder>().serviceslicecontainer;
    //    visualizationcontainer = gameobject.find("datamanager").getcomponent<globalcontainerholder>().visualizationcontainer;
    //}

    //public void construct(list<service> services, list<island> islandgos)
    //{
    //    servicevolume = new servicevolume();
    //    debug.log("started with service-gameobject construction!");
    //    startcoroutine(constructall(services, island));
    //}

    //private dictionary<serviceslice, list<service>> distributeservicestoslices(list<service> services)
    //{
    //    dictionary<serviceslice, list<service>> result = new dictionary<serviceslice, list<service>>();

    //    int slicenumber = services.count / globalvar.groupsperslice;

    //    for (int i = 0; i < slicenumber; i++)
    //    {
    //        gameobject serviceslice = new gameobject("serviceslice");
    //        float height = globalvar.startingheight + globalvar.heightstep * i;
    //        serviceslice.transform.position = new vector3(0f, height, 0f);
    //        serviceslice.transform.setparent(serviceslicecontainer.transform);
    //        serviceslice slicecomponent = serviceslice.addcomponent<serviceslice>();
    //        slicecomponent.height = height;

    //        list<service> currentservicelist = new list<service>();
    //        for (int s = 0; s < globalvar.groupsperslice; s++)
    //        {
    //            int sidx = globalvar.groupsperslice * i + s;
    //            if (sidx < services.count)
    //            {
    //                currentservicelist.add(services[sidx]);
    //            }
    //        }
    //        result.add(slicecomponent, currentservicelist);
    //    }

    //    return result;
    //}

    //ienumerator constructall(list<service> services, list<islandgo> islandgos)
    //{
    //    dictionary<serviceslice, list<service>> serviceslicemap = distributeservicestoslices(services);
    //    foreach (keyvaluepair<serviceslice, list<service>> kvp in serviceslicemap)
    //    {
    //        constructservicesandcomponents(kvp.value, kvp.key);
    //        yield return null;
    //    }

    //    foreach (islandgo islandgo in islandgos)
    //    {
    //        if (islandgo == null)
    //            continue;
    //        else
    //        {
    //            if (!islandgo.isislandempty())
    //            {
    //                foreach (region region in islandgo.getregions())
    //                    foreach (building b in region.getbuildings())
    //                    {
    //                        servicelayergo servicelayer = b.getcomponent<servicelayergo>();
    //                        if (servicelayer != null)
    //                        {
    //                            servicelayer.createdownwardconnections();

    //                            list<servicenodescript> snslist = servicelayer.getservicenodes();
    //                            foreach (servicenodescript sns in snslist)
    //                                sns.constructserviceconnections();
    //                        }
    //                    }
    //            }
    //        }
    //    }


    //    debug.log("finished with service-gameobject construction!");
    //}

    //private void constructservicesandcomponents(list<service> services, serviceslice serviceslice)
    //{
    //    foreach (service service in services)
    //    {
    //        compilationunit servicecu = service.getservicecu();
    //        if (servicecu != null && servicecu.getgameobject() != null)
    //        {

    //            gameobject servicego = instantiate(interfaceprefab, transform.position, quaternion.identity);
    //            servicego.layer = layermask.nametolayer("interactionsystemlayer");
    //            servicego.name = service.getname();
    //            vector3 cuposition = servicecu.getgameobject().transform.position;
    //            cuposition.y = 0f;
    //            servicego.transform.position = cuposition + new vector3(0f, serviceslice.height, 0f);
    //            servicego.transform.localscale = new vector3(globalvar.servicenodesize, globalvar.servicenodesize, globalvar.servicenodesize);
    //            servicego.transform.setparent(serviceslice.transform);
    //            servicenodescript sns = servicego.addcomponent<servicenodescript>();
    //            servicego.addcomponent<textlabelcomponent>();
    //            servicelayergo slgo = servicecu.getgameobject().getcomponent<servicelayergo>();
    //            slgo.addservicenode(sns);
    //            #region construct servicecomponents
    //            list<servicecomponent> implementingcomponents = service.getimplementingcomponents();
    //            list<servicecomponent> referencingcomponents = service.getreferencingcomponents();
    //            foreach (servicecomponent sc in implementingcomponents)
    //            {
    //                compilationunit componentcu = sc.getimplementationcu();
    //                gameobject scgo = instantiate(implementationprefab, transform.position, quaternion.identity);
    //                scgo.layer = layermask.nametolayer("interactionsystemlayer");
    //                scgo.name = sc.getname();
    //                cuposition = componentcu.getgameobject().transform.position;
    //                cuposition.y = 0f;
    //                scgo.transform.position = cuposition + new vector3(0f, serviceslice.height, 0f);
    //                scgo.transform.localscale = new vector3(globalvar.servicenodesize, globalvar.servicenodesize, globalvar.servicenodesize);
    //                scgo.transform.setparent(serviceslice.transform);
    //                servicenodescript scgocomponent = scgo.addcomponent<servicenodescript>();
    //                scgo.addcomponent<textlabelcomponent>();
    //                scgocomponent.addconnectedservicenode(sns);
    //                sns.addconnectedservicenode(scgocomponent);
    //                scgocomponent.disableservicenode();
    //                servicelayergo sl = componentcu.getgameobject().getcomponent<servicelayergo>();
    //                sl.addservicenode(scgocomponent);
    //            }
    //            foreach (servicecomponent sc in referencingcomponents)
    //            {
    //                compilationunit componentcu = sc.getimplementationcu();
    //                gameobject scgo = instantiate(referenceprefab, transform.position, quaternion.identity);
    //                scgo.layer = layermask.nametolayer("interactionsystemlayer");
    //                scgo.name = sc.getname();
    //                cuposition = componentcu.getgameobject().transform.position;
    //                cuposition.y = 0f;
    //                scgo.transform.position = cuposition + new vector3(0f, serviceslice.height, 0f);
    //                scgo.transform.localscale = new vector3(globalvar.servicenodesize, globalvar.servicenodesize, globalvar.servicenodesize);
    //                scgo.transform.setparent(serviceslice.transform);
    //                servicenodescript scgocomponent = scgo.addcomponent<servicenodescript>();
    //                scgo.addcomponent<textlabelcomponent>();
    //                scgocomponent.addconnectedservicenode(sns);
    //                sns.addconnectedservicenode(scgocomponent);

    //                scgocomponent.disableservicenode();
    //                servicelayergo sl = componentcu.getgameobject().getcomponent<servicelayergo>();
    //                sl.addservicenode(scgocomponent);
    //            }
    //            #endregion
    //            sns.disableservicenode();
    //        }
    //    }
    //}
}
