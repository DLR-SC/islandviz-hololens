using HoloIslandVis.Core;
using HoloIslandVis.Model.Graph;
using HoloIslandVis.Model.OSGi.Services;
using Neo4j.Driver.V1;
using Neo4JDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis.Model.OSGi
{
    public class GraphDatabaseParser : Singleton<GraphDatabaseParser>
    {
        public delegate void ProjectModelParsedHandler(OSGiProject project);
        public event ProjectModelParsedHandler ProjectModelParsed = delegate { };

        private string _redundantString = "http://www.example.org/OSGiApplicationModel#//";
        private ReferenceResolver _referenceResolver;
        private Dictionary<string, Type> _types;
        private Dictionary<string, AccessModifier> _accessModifiers;

        private GraphDatabaseParser()
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

        public OSGiProject Parse(string ip, string user, string pass)
        {
            Neo4J database = new Neo4J("bolt://" + ip + ":7687", user, pass);
            OSGiProject osgiProject = new OSGiProject("Database at " + ip);
            //OSGiProject osgiProject = new OSGiProject(modelData.GetField("name").str);
            //_referenceResolver = new ReferenceResolver(modelData.GetField("packages"),
            //    modelData.GetField("services"));

            ParseBundles(osgiProject, database);
            //ParseServices(osgiProject, modelData.GetField("services"));
            //ParseDependencies(osgiProject, database);

            // TODO: Refactor?
            osgiProject.Bundles.Sort((x, y) => x.Packages.Count.CompareTo(y.Packages.Count));
            osgiProject.Bundles.Reverse();

            foreach (Bundle bundle in osgiProject.Bundles)
            {
                foreach (Package package in bundle.Packages)
                {
                    package.CompilationUnits.Sort((x, y) => x.LinesOfCode.CompareTo(y.LinesOfCode));
                    package.CompilationUnits.Reverse();
                }

                bundle.Packages.Sort((x, y) => x.CompilationUnitCount.CompareTo(y.CompilationUnitCount));
                bundle.Packages.Reverse();
            }

            Debug.Log("Done parsing OSGi project model.");
            ProjectModelParsed(osgiProject);
            return osgiProject;
        }

        private List<Bundle> ParseBundles(OSGiProject osgiProject, Neo4J database)
        {
            IStatementResult result = database.Transaction("MATCH (b:Bundle) RETURN b.bundleSymbolicName as symbolicName");
            List<string> bundleSymbolicNameList = result.Select(record => record["symbolicName"].As<string>()).ToList();

            result = database.Transaction("MATCH (b:Bundle) RETURN b.name as name");
            List<string> bundleNameList = result.Select(record => record["name"].As<string>()).ToList();

            for (int i = 0; i < bundleNameList.Count; i++)
            {
                string fieldBundleName = bundleNameList[i];
                string fieldBundleSymbolicName = bundleSymbolicNameList[i];

                IStatementResult packages = database.Transaction("MATCH (b:Bundle {bundleSymbolicName: '" + fieldBundleSymbolicName + "'})-[h:CONTAINS]->(p:PackageFragment) RETURN p.fileName as name"); // EXPORTS?
                List<string> packageFileNameList = packages.Select(record => record["name"].As<string>()).ToList();

                if (packageFileNameList.Count > 1 || packageFileNameList.Count == 0)
                    continue;

                Bundle bundle = new Bundle(osgiProject, fieldBundleName, fieldBundleSymbolicName);
                ParsePackageFragments(bundle, database);
                osgiProject.Bundles.Add(bundle);
                GraphVertex vertex = new GraphVertex(bundle.Name);
                osgiProject.DependencyGraph.AddVertex(vertex);
            }


            return osgiProject.Bundles;
        }

        private List<Package> ParsePackageFragments(Bundle bundle, Neo4J database)
        {
            IStatementResult result = database.Transaction("MATCH (b:Bundle {bundleSymbolicName: '" + bundle.SymbolicName + "'})-[h:CONTAINS]->(p:PackageFragment) RETURN p.fileName as name"); // EXPORTS?
            List<string> packageFileNameList = result.Select(record => record["name"].As<string>()).ToList();

            if (packageFileNameList == null)
                return null;

            foreach (string packageFileName in packageFileNameList)
            {
                //JSONObject fieldPackageReference = fieldPackageFragment.GetField("package");
                //JSONObject fieldPackage = _referenceResolver.ResolvePackageReference(fieldPackageReference);
                Package package = new Package(bundle, packageFileName);
                //JSONObject fieldCompilationUnits = fieldPackageFragment.GetField("compilationUnits");

                ParseCompilationUnits(package, database);

                bundle.Packages.Add(package);
            }

            return bundle.Packages;
        }

        private List<CompilationUnit> ParseCompilationUnits(Package package, Neo4J database)
        {
            IStatementResult result = database.Transaction("MATCH (p:Package{fileName: '" + package.Name + "'})-[h:CONTAINS]->(c:Class) " + "RETURN c.name as className");
            List<string> classNameList = result.Select(record => record["className"].As<string>()).ToList();

            result = database.Transaction("MATCH (p:Package{fileName: '" + package.Name + "'})-[h:CONTAINS]->(c:Class) " + "RETURN c.visibility as classModifier");
            List<string> classModifier = result.Select(record => record["classModifier"].As<string>()).ToList();

            result = database.Transaction("MATCH (p:Package{fileName: '" + package.Name + "'})-[h:CONTAINS]->(c:Class) " + "RETURN c.linesOfCode as classLOC");
            List<string> classLOC = result.Select(record => record["classLOC"].As<string>()).ToList();

            if (classNameList == null || classModifier == null || classLOC == null)
                return null;

            for (int i = 0; i < classNameList.Count; i++)
            {
                if (classLOC[i] == null || classLOC[i] == "Null")
                    classLOC[i] = "0";

                //JSONObject fieldTopLevelType = fieldCompilationUnit.GetField("topLevelType");
                string compilationUnitName = classNameList[i];
                string compilationUnitModifier = classModifier[i];
                long fieldLinesOfCode = long.Parse(classLOC[i]);

                if (package.Bundle.OSGiProject.MaximalLinesOfCode < fieldLinesOfCode)
                    package.Bundle.OSGiProject.MaximalLinesOfCode = fieldLinesOfCode;

                //compilationUnitType = compilationUnitType.Replace(_redundantString, "");
                //compilationUnitModifier = compilationUnitModifier.Replace(_redundantString, "");

                Type type = Type.Unknown;
                AccessModifier modifier = AccessModifier.Default;
                _types.TryGetValue("Class", out type);
                _accessModifiers.TryGetValue(compilationUnitModifier, out modifier);

                CompilationUnit compilationUnit = new CompilationUnit(package, compilationUnitName, fieldLinesOfCode, type, modifier);
                package.CompilationUnits.Add(compilationUnit);
            }

            return package.CompilationUnits;
        }

        private List<Service> ParseServices(OSGiProject osgiProject, JSONObject fieldServices)
        {
            if (fieldServices == null)
                return null;

            foreach (JSONObject fieldService in fieldServices)
            {
                string serviceName = fieldService.GetField("interfaceName").str;
                JSONObject fieldInterface = fieldService.GetField("interface");
                CompilationUnit serviceCompilationUnit = null;
                if (fieldInterface != null)
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

        private void ParseDependencies(OSGiProject osgiProject, JSONObject fieldBundles)
        {
            int bundleIndex = 0;
            foreach (JSONObject fieldBundle in fieldBundles)
            {
                ParseExports(osgiProject, fieldBundle.GetField("exports"), bundleIndex);
                ParseImports(osgiProject, fieldBundle.GetField("imports"), bundleIndex);
                //ParseComponents(osgiProject, fieldBundle.GetField("components"), bundleIndex);

                bundleIndex++;
            }
        }

        private void ParseExports(OSGiProject osgiProject, JSONObject fieldExports, int bundleIndex)
        {
            if (fieldExports == null)
                return;

            List<Vector2> exportIndices =
                _referenceResolver.ResolvePackageFragmentReferenceList(fieldExports.list);

            foreach (Vector2 index in exportIndices)
            {
                Package resolvedFragment = osgiProject.GetPackage(index);
                resolvedFragment.IsExported = true;
                osgiProject.Bundles[bundleIndex].ExportedPackages.Add(resolvedFragment);
            }
        }

        private void ParseImports(OSGiProject osgiProject, JSONObject fieldImports, int bundleIndex)
        {
            if (fieldImports == null)
                return;

            List<Vector2> importIndices =
                _referenceResolver.ResolvePackageFragmentReferenceList(fieldImports.list);

            foreach (Vector2 index in importIndices)
            {
                Package resolvedFragment = osgiProject.GetPackage(index);
                if (string.Compare(osgiProject.Bundles[bundleIndex].Name, resolvedFragment.Bundle.Name) != 0)
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
                    if (edgePresent && osgiProject.DependencyGraph.AllowParallelEdges)
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

        private void ParseComponents(OSGiProject osgiProject, JSONObject fieldComponents, int bundleIndex)
        {
            if (fieldComponents == null)
                return;

            foreach (JSONObject fieldComponent in fieldComponents.list)
            {
                Vector3 implIndex = _referenceResolver.
                    ResolveCompilationUnitReference(fieldComponent.GetField("implementation"));

                CompilationUnit resolvedCompilationUnit = osgiProject.GetCompilationUnit(implIndex);
                resolvedCompilationUnit.IsServiceComponent = true;

                ServiceComponent serviceComponent
                    = new ServiceComponent(fieldComponent.GetField("name").str, resolvedCompilationUnit);

                ParseProvidedServices(osgiProject, serviceComponent, fieldComponent.GetField("providedServices"));
                ParseReferencedServices(osgiProject, serviceComponent, fieldComponent.GetField("referencedServices"));
                osgiProject.Bundles[bundleIndex].ServiceComponents.Add(serviceComponent);
            }
        }

        private void ParseProvidedServices(OSGiProject osgiProject, ServiceComponent serviceComponent,
            JSONObject fieldProvidedServices)
        {
            if (fieldProvidedServices == null)
                return;

            List<int> serviceReferences = _referenceResolver.ResolveServiceReferenceList(fieldProvidedServices);
            foreach (int serviceReference in serviceReferences)
            {
                serviceComponent.ProvidedServices.Add(osgiProject.Services[serviceReference]);
                osgiProject.Services[serviceReference].ImplementingComponents.Add(serviceComponent);
            }
        }

        private void ParseReferencedServices(OSGiProject osgiProject, ServiceComponent serviceComponent,
            JSONObject fieldReferencedServices)
        {
            if (fieldReferencedServices == null)
                return;

            List<int> serviceReferences = _referenceResolver.ResolveServiceReferenceList(fieldReferencedServices);
            foreach (int serviceReference in serviceReferences)
            {
                serviceComponent.ReferencedServices.Add(osgiProject.Services[serviceReference]);
                osgiProject.Services[serviceReference].ReferencingComponents.Add(serviceComponent);
            }
        }
    }
}
