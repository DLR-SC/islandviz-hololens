﻿using HoloIslandVis.OSGiParser.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloIslandVis.OSGiParser
{
    public class OSGiProjectParser
    {
        private static OSGiProjectParser _instance;

        public static OSGiProjectParser Instance {
            get {
                if (_instance == null)
                    _instance = new OSGiProjectParser();

                return _instance;
            }

            private set { }
        }

        private string _redundantString = "http://www.example.org/OSGiApplicationModel#//";
        private ReferenceResolver _referenceResolver;
        private Dictionary<string, Type> _types;
        private Dictionary<string, AccessModifier> _accessModifiers;

        private OSGiProjectParser()
        {
            _types = new Dictionary<string, Type>()
            {
                { "Class", Type.Class },
                { "AbstractClass", Type.AbstractClass },
                { "Interface", Type.Interface },
                { "Enum", Type.Enum },
                { "Unknown", Type.Unknown }
            };

            _accessModifiers = new Dictionary<string, AccessModifier>()
            {
                { "PUBLIC", AccessModifier.Public },
                { "PRIVATE", AccessModifier.Private },
                { "PROTECTED", AccessModifier.Protected },
                { "STATIC", AccessModifier.Static },
                { "FINAL", AccessModifier.Final },
                { "DEFAULT", AccessModifier.Default }
            };
        }

        public OSGiProject Parse(JSONObject modelData)
        {
            OSGiProject osgiProject = new OSGiProject(modelData.GetField("name").str);
            _referenceResolver = new ReferenceResolver(modelData.GetField("packages"), 
                modelData.GetField("services"));

            parseBundles(osgiProject, modelData.GetField("bundles"));
            parseServices(osgiProject, modelData.GetField("services"));
            parseDependencies(osgiProject, modelData.GetField("bundles"));

            // TODO: Refactor?
            osgiProject.Bundles.Sort((x, y) => x.Packages.Count.CompareTo(y.Packages.Count));
            osgiProject.Bundles.Reverse();

            foreach(Bundle bundle in osgiProject.Bundles)
            {
                foreach(Package package in bundle.Packages)
                {
                    package.CompilationUnits.Sort((x, y) => x.LinesOfCode.CompareTo(y.LinesOfCode));
                    package.CompilationUnits.Reverse();
                }

                bundle.Packages.Sort((x, y) => x.CompilationUnitCount.CompareTo(y.CompilationUnitCount));
                bundle.Packages.Reverse();
            }

            return osgiProject;
        }

        private List<Bundle> parseBundles(OSGiProject osgiProject, JSONObject fieldBundles)
        {
            foreach(JSONObject fieldBundle in fieldBundles.list)
            {
                string fieldBundleName = fieldBundle.GetField("name").str;
                string fieldBundleSymbolicName = fieldBundle.GetField("symbolicName").str;
                Bundle bundle = new Bundle(osgiProject, fieldBundleName, fieldBundleSymbolicName);
                parsePackageFragments(bundle, fieldBundle.GetField("packageFragments"));
                osgiProject.Bundles.Add(bundle);

                //Debug.Log(bundle.Name);
            }


            return osgiProject.Bundles;
        }

        private List<Package> parsePackageFragments(Bundle bundle, JSONObject fieldPackageFragments)
        {
            foreach(JSONObject fieldPackageFragment in fieldPackageFragments.list)
            {
                JSONObject fieldPackageReference = fieldPackageFragment.GetField("package");
                JSONObject fieldPackage = _referenceResolver.ResolvePackageReference(fieldPackageReference);
                Package package = new Package(bundle, fieldPackage.GetField("qualifiedName").str);

                JSONObject fieldCompilationUnits = fieldPackageFragment.GetField("compilationUnits");
                if (fieldCompilationUnits != null)
                    parseCompilationUnits(package, fieldCompilationUnits);
                bundle.Packages.Add(package);
            }

            return bundle.Packages;
        }

        private List<CompilationUnit> parseCompilationUnits(Package package, JSONObject fieldCompilationUnits)
        {
            foreach(JSONObject fieldCompilationUnit in fieldCompilationUnits.list)
            {
                JSONObject fieldTopLevelType = fieldCompilationUnit.GetField("topLevelType");
                string compilationUnitType = fieldTopLevelType.GetField("eClass").str;
                string compilationUnitModifier = fieldTopLevelType.GetField("visibility").str;
                long fieldLinesOfCode = fieldCompilationUnit.GetField("LOC").i;

                if (package.Bundle.OSGiProject.MaximalLinesOfCode < fieldLinesOfCode)
                    package.Bundle.OSGiProject.MaximalLinesOfCode = fieldLinesOfCode;

                compilationUnitType = compilationUnitType.Replace(_redundantString, "");
                compilationUnitModifier = compilationUnitModifier.Replace(_redundantString, "");

                Type type = Type.Unknown;
                AccessModifier modifier = AccessModifier.Default;
                _types.TryGetValue(compilationUnitType, out type);
                _accessModifiers.TryGetValue(compilationUnitType, out modifier);

                CompilationUnit compilationUnit = new CompilationUnit(package, 
                    fieldTopLevelType.GetField("name").str, fieldLinesOfCode, type, modifier);
                package.CompilationUnits.Add(compilationUnit);
            }

            return package.CompilationUnits;
        }

        private List<Service> parseServices(OSGiProject osgiProject, JSONObject fieldServices)
        {
            if(fieldServices == null)
                return null;

            foreach(JSONObject fieldService in fieldServices)
            {
                string serviceName = fieldService.GetField("interfaceName").str;
                JSONObject fieldInterface = fieldService.GetField("interface");
                CompilationUnit serviceCompilationUnit = null;
                if(fieldInterface != null)
                {
                    Vector3 compilationUnitIndex =
                        _referenceResolver.ResolveCompilationUnitReference(fieldInterface);

                    serviceCompilationUnit = osgiProject.GetCompilationUnit(compilationUnitIndex);
                    serviceCompilationUnit.IsService = true;
                }

                Service service = new Service(serviceName, serviceCompilationUnit);
                osgiProject.Services.Add(service);
            }

            return osgiProject.Services;
        }

        private void parseDependencies(OSGiProject osgiProject, JSONObject fieldBundles)
        {
            int bundleIndex = 0;
            foreach(JSONObject fieldBundle in fieldBundles)
            {
                parseExports(osgiProject, fieldBundle.GetField("exports"), bundleIndex);
                parseImports(osgiProject, fieldBundle.GetField("imports"), bundleIndex);
                parseComponents(osgiProject, fieldBundle.GetField("components"), bundleIndex);

                bundleIndex++;
            }
        }

        private void parseExports(OSGiProject osgiProject, JSONObject fieldExports, int bundleIndex)
        {
            if(fieldExports == null)
                return;

            List<Vector2> exportIndices =
                _referenceResolver.ResolvePackageFragmentReferenceList(fieldExports.list);

            foreach(Vector2 index in exportIndices)
            {
                Package resolvedFragment = osgiProject.GetPackage(index);
                resolvedFragment.IsExported = true;
                osgiProject.Bundles[bundleIndex].ExportedPackages.Add(resolvedFragment);
            }
        }

        private void parseImports(OSGiProject osgiProject, JSONObject fieldImports, int bundleIndex)
        {
            if(fieldImports == null)
                return;

            List<Vector2> importIndices =
                _referenceResolver.ResolvePackageFragmentReferenceList(fieldImports.list);

            foreach(Vector2 index in importIndices)
            {
                Package resolvedFragment = osgiProject.GetPackage(index);
                if(string.Compare(osgiProject.Bundles[bundleIndex].Name, resolvedFragment.Bundle.Name) != 0)
                {
                    osgiProject.Bundles[bundleIndex].ImportedPackages.Add(resolvedFragment);

                    // TODO: Refactor.
                    List<GraphVertex> allVertices = osgiProject.DependencyGraph.Vertices.ToList();
                    Bundle sourceBundle = osgiProject.Bundles[bundleIndex];
                    Bundle targetBundle = osgiProject.Bundles[(int)index.x];

                    GraphVertex vert1 = allVertices.Find(v => (string.Equals(v.Name, sourceBundle.Name)));
                    GraphVertex vert2 = allVertices.Find(v => (string.Equals(v.Name, targetBundle.Name)));

                    if (vert1 == null)
                        vert1 = new GraphVertex(sourceBundle.Name);
                    if (vert2 == null)
                        vert2 = new GraphVertex(targetBundle.Name);

                    osgiProject.DependencyGraph.AddVertex(vert1);
                    osgiProject.DependencyGraph.AddVertex(vert2);

                    GraphEdge edge;
                    bool edgePresent = osgiProject.DependencyGraph.TryGetEdge(vert1, vert2, out edge);
                    if(edgePresent && osgiProject.DependencyGraph.AllowParallelEdges)
                    {
                        edge.Weight++;
                    }
                    else
                    {
                        edge = new GraphEdge(vert1, vert2);
                        osgiProject.DependencyGraph.AddEdge(edge);
                    }

                    GraphEdge opposingEdge;
                    float bidirectionalEdgeWeight = edge.Weight;
                    bool oppEdgePresent = osgiProject.DependencyGraph.TryGetEdge(vert2, vert1, out opposingEdge);
                    if (oppEdgePresent)
                        bidirectionalEdgeWeight += opposingEdge.Weight;

                    if (bidirectionalEdgeWeight > osgiProject.MaximalImportCount)
                        osgiProject.MaximalImportCount = (int)bidirectionalEdgeWeight;

                    if (bidirectionalEdgeWeight > osgiProject.MaximalImportCount)
                        osgiProject.MaximalImportCount = (int)bidirectionalEdgeWeight;
                }
            }
        }

        private void parseComponents(OSGiProject osgiProject, JSONObject fieldComponents, int bundleIndex)
        {
            if (fieldComponents == null)
                return;

            foreach(JSONObject fieldComponent in fieldComponents.list)
            {
                Vector3 implIndex = _referenceResolver.
                    ResolveCompilationUnitReference(fieldComponent.GetField("implementation"));

                CompilationUnit resolvedCompilationUnit = osgiProject.GetCompilationUnit(implIndex);
                resolvedCompilationUnit.IsServiceComponent = true;

                ServiceComponent serviceComponent
                    = new ServiceComponent(fieldComponent.GetField("name").str, resolvedCompilationUnit);

                parseProvidedServices(osgiProject, serviceComponent, fieldComponent.GetField("providedServices"));
                parseReferencedServices(osgiProject, serviceComponent, fieldComponent.GetField("referencedServices"));
                osgiProject.Bundles[bundleIndex].ServiceComponents.Add(serviceComponent);
            }
        }

        private void parseProvidedServices(OSGiProject osgiProject, ServiceComponent serviceComponent, 
            JSONObject fieldProvidedServices)
        {
            if (fieldProvidedServices == null)
                return;

            List<int> serviceReferences = _referenceResolver.ResolveServiceReferenceList(fieldProvidedServices);
            foreach(int serviceReference in serviceReferences)
            {
                serviceComponent.ProvidedServices.Add(osgiProject.Services[serviceReference]);
                osgiProject.Services[serviceReference].ImplementingComponents.Add(serviceComponent);
            }
        }

        private void parseReferencedServices(OSGiProject osgiProject, ServiceComponent serviceComponent,
            JSONObject fieldReferencedServices)
        {
            if (fieldReferencedServices == null)
                return;

            List<int> serviceReferences = _referenceResolver.ResolveServiceReferenceList(fieldReferencedServices);
            foreach(int serviceReference in serviceReferences)
            {
                serviceComponent.ReferencedServices.Add(osgiProject.Services[serviceReference]);
                osgiProject.Services[serviceReference].ReferencingComponents.Add(serviceComponent);
            }
        }
    }
}
