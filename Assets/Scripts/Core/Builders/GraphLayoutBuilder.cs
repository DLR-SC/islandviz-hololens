using HoloIslandVis.Model.Graph;
using HoloIslandVis.Model.OSGi;
using HoloIslandVis.Sharing;
using HoloIslandVis.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloIslandVis.Core.Builders
{
    public class GraphLayoutBuilder : SingletonComponent<GraphLayoutBuilder>
    {
        public float StepSize;
        public int SimulationSteps;

        private struct VertexPositionData
        {
            public Vector2 position;
            public Vector2 oldPosition;

            public VertexPositionData(Vector2 pos, Vector2 oldPos)
            {
                position = pos;
                oldPosition = oldPos;
            }
        }

        private System.Random _random;
        private OSGiProject _project;
        private Dictionary<GraphVertex, VertexPositionData> _simulationData;

        void Start()
        {
            _random = new System.Random(0);
        }

        public void BuildForceDirectedLayout(OSGiProject project)
        {
            _simulationData = new Dictionary<GraphVertex, VertexPositionData>();
            _project = project;

            // TODO: Refactor
            float attraction = 10.0f;
            float spring = 1.0f;
            float repulsion = 5.0f;
            float attractToCenter = 0.005f;
            float friction = 0.105f;
            float timestep = StepSize;

            foreach (GraphVertex vertex in project.DependencyGraph.Vertices)
            {
                float rr = 20f;

                float x = (SyncDataStorage.Instance.GetRandomNumber(_random) - 0.5f) * rr;
                float y = (SyncDataStorage.Instance.GetRandomNumber(_random) - 0.5f) * rr;

                Vector2 startPos = new Vector2(x, y);
                VertexPositionData vertexData = new VertexPositionData(startPos, startPos);
                _simulationData.Add(vertex, vertexData);
            }

            int stepCounter = 0;
            while(stepCounter < SimulationSteps)
            {
                foreach(GraphVertex vertex in project.DependencyGraph.Vertices)
                {
                    Vector2 netForce = Vector2.zero;

                    netForce += ForceDirectedAttraction(vertex, attraction, spring);
                    netForce += ForceDirectedRepulsion(vertex, repulsion);
                    netForce -= ForceDirectedAttractToCenter(vertex, attractToCenter);
                    VerletIntegration(vertex, netForce, friction, timestep);

                    netForce = new Vector2((float)Math.Truncate(netForce.x * 10) / 10, (float)Math.Truncate(netForce.y * 10) /10);
                    stepCounter++;
                }
            }

            foreach (GraphVertex vertex in project.DependencyGraph.Vertices)
            {
                VertexPositionData vertexData = _simulationData[vertex];
                Vector3 pos = new Vector3(vertexData.position.x, 0, vertexData.position.y);
                vertex.Position = pos;
            }
        }

        // TODO: Refactor.
        public void BuildRandomGraphLayout(OSGiProject project, Vector3 min, Vector3 max, float minDist, int maxIter)
        {
            _project = project;
            int overlappingBundles = 0;
            foreach (GraphVertex vertex in project.DependencyGraph.Vertices)
            {
                Debug.Log("Vertex position for " + vertex.Name);
                Vector3 diagonalVector = min - max;

                float randomX = diagonalVector.x * SyncDataStorage.Instance.GetRandomNumber(_random);
                float randomY = diagonalVector.y * SyncDataStorage.Instance.GetRandomNumber(_random);
                float randomZ = diagonalVector.z * SyncDataStorage.Instance.GetRandomNumber(_random);
                Vector3 randomVector = min + new Vector3(randomX, randomY, randomZ);

                int iteration = 0;
                while (!CheckOverlap(randomVector, vertex.Island, minDist) && iteration <= maxIter)
                {
                    randomX = diagonalVector.x * SyncDataStorage.Instance.GetRandomNumber(_random);
                    randomY = diagonalVector.y * SyncDataStorage.Instance.GetRandomNumber(_random);
                    randomZ = diagonalVector.z * SyncDataStorage.Instance.GetRandomNumber(_random);

                    randomVector = min + new Vector3(randomX, randomY, randomZ);
                    iteration++;
                    Debug.Log("Vertex position for " + vertex.Name + ": iteration " + iteration);
                }

                vertex.Position = randomVector;
                if (iteration >= maxIter)
                    overlappingBundles++;
            }

            Debug.Log("Graph layout is computed! Number of overlapping bundles: " + overlappingBundles);
        }

        private Vector2 ForceDirectedAttraction(GraphVertex vertex, float attraction, float spring)
        {
            IEnumerable<GraphEdge> outEdges;
            _project.DependencyGraph.TryGetOutEdges(vertex, out outEdges);
            List<GraphEdge> edgeList = outEdges.ToList();
            Vector2 springForce = Vector2.zero;

            foreach (GraphEdge importEdge in edgeList)
            {
                GraphVertex targetVertex = importEdge.Target;
                Vector2 direction = _simulationData[targetVertex].position - _simulationData[vertex].position;

                float combinedRadius = (vertex.Island.Radius + targetVertex.Island.Radius);
                float springEquilibriumLength = combinedRadius + spring * (_project.MaximalImportCount) / importEdge.Weight;
                springForce += attraction * direction.normalized * (float)Math.Log((direction.magnitude / springEquilibriumLength));
            }

            return springForce;
        }

        private Vector2 ForceDirectedRepulsion(GraphVertex vertex, float repulsion)
        {
            IEnumerable<GraphEdge> inEdges;
            _project.DependencyGraph.TryGetInEdges(vertex, out inEdges);
            List<GraphEdge> edgeList = inEdges.ToList();
            Vector2 force = Vector2.zero;

            foreach (GraphVertex other in _project.DependencyGraph.Vertices)
            {

                GraphEdge edge = edgeList.Find(x => (x.Source == other) || (x.Target == other));
                if (other == vertex || edge != null)
                    continue;

                Vector2 direction = _simulationData[vertex].position - _simulationData[other].position;

                float distanceToBounds = direction.magnitude - (vertex.Island.Radius + other.Island.Radius);

                if (distanceToBounds < 0.0f)
                {
                    Vector2 constrainedPosition = _simulationData[vertex].position - direction.normalized * distanceToBounds;
                    _simulationData[other] = new VertexPositionData(constrainedPosition, constrainedPosition);
                    distanceToBounds = 0f;
                }

                float divisor = distanceToBounds + 0.1f;
                force += (direction.normalized * repulsion) / (divisor * divisor);
            }

            return force;
        }

        private Vector2 ForceDirectedAttractToCenter(GraphVertex vertex, float attractToCenter)
        {
            return _simulationData[vertex].position * attractToCenter;
        }

        private void VerletIntegration(GraphVertex vertex, Vector2 force, float friction, float timestep)
        {
            Vector2 posDiff = _simulationData[vertex].position - _simulationData[vertex].oldPosition;
            Vector2 velocity = posDiff / timestep;
            Vector2 resistance = velocity * timestep * friction;

            Vector2 oldPosition = _simulationData[vertex].position;
            Vector2 newPosition = 2.0f * _simulationData[vertex].position - _simulationData[vertex].oldPosition 
                + timestep * timestep * force;

            newPosition -= resistance;

            VertexPositionData vertexData = new VertexPositionData(newPosition, oldPosition);
            _simulationData[vertex] = vertexData;
        }

        private bool CheckOverlap(Vector3 newPosition, CartographicIsland newIsland, float minDist)
        {
            var vertices = _project.DependencyGraph.Vertices;
            foreach (GraphVertex existingVertex in vertices)
            {
                Vector3 existingPos = existingVertex.Position;
                float distance = Vector3.Distance(existingPos, newPosition);
                float existingRadius = existingVertex.Island.Radius;
                float newRadius = newIsland.Radius;
                if (distance <= (minDist + (existingRadius + newRadius)))
                    return false;
            }

            return true;
        }
    }
}
