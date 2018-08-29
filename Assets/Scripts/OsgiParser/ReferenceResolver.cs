using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.OSGiParser
{
    public class ReferenceResolver
    {
        private JSONObject _fieldPackages;
        private JSONObject _fieldServices;

        public ReferenceResolver(JSONObject fieldPackages, JSONObject fieldServices)
        {
            _fieldPackages = fieldPackages;
            _fieldServices = fieldServices;
        }

        public JSONObject ResolvePackageReference(JSONObject fieldPackage)
        {
            int packageIndex;
            string rawReference = fieldPackage.GetField("$ref").str;
            string processedString = rawReference.Replace("//@packages.", "");
            packageIndex = int.Parse(processedString);

            return _fieldPackages[packageIndex];
        }

        // IN: A JSONObject containing a $ref string of the format "//@bundles.X/@packageFragments.Y/@compilationUnits.Z..."
        // OUT: A Vector3(X,Y,Z) representing Bundle, PackageFragment and CompilationUnit number.
        public Vector3 ResolveCompilationUnitReference(JSONObject fieldCompilationUnit)
        {
            string rawReference = fieldCompilationUnit.GetField("$ref").str;
            string processedString = rawReference.Replace("//@bundles.", "");
            processedString = processedString.Replace("/@packageFragments.", ",");
            processedString = processedString.Replace("/@compilationUnits.", ",");

            // Cut off after compilation unit, since we dont support nested types yet.
            int index = processedString.LastIndexOf(",");
            processedString = processedString.Remove(index + 2);
            string[] values = processedString.Split(',');

            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }

        // IN: A list of JSONObjects containing a $ref string of the format "//@bundles.X/@packageFragments.Y"
        // OUT: A list of Vector2(X,Y) representing Bundle and Packagefragment number
        public List<Vector2> ResolvePackageFragmentReferenceList(List<JSONObject> fragmentReferenceList)
        {
            List<Vector2> result = new List<Vector2>();
            foreach (JSONObject fragmentReference in fragmentReferenceList)
            {
                string rawReference = fragmentReference.GetField("$ref").str;
                string processedString = rawReference.Replace("//@bundles.", "");
                processedString = processedString.Replace("/@packageFragments.", ",");
                string[] values = processedString.Split(',');
                result.Add(new Vector2(float.Parse(values[0]), float.Parse(values[1])));
            }

            return result;
        }

        //Input: A JSONObject containing a list of $ref strings of the format "//@services.X"
        //Output: A List<int> representing the service numbers X
        public List<int> ResolveServiceReferenceList(JSONObject fieldServiceReference)
        {
            List<int> result = new List<int>();
            foreach (JSONObject listEntry in fieldServiceReference.list)
            {
                string rawReference = listEntry.GetField("$ref").str;
                string processedString = rawReference.Replace("//@services.", "");
                result.Add(int.Parse(processedString));
            }

            return result;
        }
    }

}