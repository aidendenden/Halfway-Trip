﻿#if POMINI

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Pinwheel.PolarisMini
{
    //[BurstCompile(CompileSynchronously = false)]
    public struct StitchSeamLODJob : IJob
    {
        public NativeArray<SubdivNode> nodes;
        public NativeArray<byte> creationState;
        public NativeArray<int> metadata;

        public NativeArray<bool> vertexMarker;

        [ReadOnly]
        public NativeArray<bool> vertexMarker_LOD0;

        public int meshBaseResolution;
        public int meshResolution;
        public int lod;

        public void Execute()
        {
            int startIndex = 0;
            int endIndex = 0;
            int leftNodeIndex = 0;
            int rightNodeIndex = 0;
            bool mark0 = false;
            bool mark1 = false;
            bool mark2 = false;
            bool mark3 = false;
            bool mark4 = false;
            bool mark5 = false;
            Vector2 v12 = Vector2.zero;

            SubdivNode currentNode;
            SubdivNode leftNode = new SubdivNode();
            SubdivNode rightNode = new SubdivNode();

            meshBaseResolution = Mathf.Max(0, meshBaseResolution - lod);

            metadata[GeometryJobUtilities.METADATA_NEW_VERTEX_CREATED] = 0;
            for (int res = meshBaseResolution; res < meshResolution; ++res)
            {
                startIndex = GeometryJobUtilities.GetStartIndex(ref res);
                endIndex = startIndex + GeometryJobUtilities.GetElementCountForSubdivLevel(ref res) - 1;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    GeometryJobUtilities.GetChildrenNodeIndex(ref i, ref leftNodeIndex, ref rightNodeIndex);
                    if (creationState[leftNodeIndex] != GeometryJobUtilities.STATE_NOT_CREATED ||
                        creationState[rightNodeIndex] != GeometryJobUtilities.STATE_NOT_CREATED)
                        continue;
                    currentNode = nodes[i];
                    mark0 = GeometryJobUtilities.GetVertexMark(vertexMarker, currentNode.v01);
                    mark1 = GeometryJobUtilities.GetVertexMark(vertexMarker, currentNode.v12);
                    mark2 = GeometryJobUtilities.GetVertexMark(vertexMarker, currentNode.v20);
                    mark3 = CheckMarkLOD0(currentNode.v01);
                    mark4 = CheckMarkLOD0(currentNode.v12);
                    mark5 = CheckMarkLOD0(currentNode.v20);

                    if (mark0 || mark1 || mark2 ||
                        mark3 || mark4 || mark5)
                    {
                        currentNode.Split(ref leftNode, ref rightNode);
                        nodes[leftNodeIndex] = leftNode;
                        nodes[rightNodeIndex] = rightNode;
                        creationState[leftNodeIndex] = GeometryJobUtilities.STATE_CREATED;
                        creationState[rightNodeIndex] = GeometryJobUtilities.STATE_CREATED;
                        GeometryJobUtilities.MarkVertices(ref vertexMarker, ref leftNode);
                        GeometryJobUtilities.MarkVertices(ref vertexMarker, ref rightNode);

                        metadata[GeometryJobUtilities.METADATA_NEW_VERTEX_CREATED] = 1;
                    }
                }
            }
        }

        private bool CheckMarkLOD0(Vector2 p)
        {
            if (p.x > 0 && p.x < 1 && p.y > 0 && p.y < 1)
            {
                return false;
            }
            else
            {
                return GeometryJobUtilities.GetVertexMark(vertexMarker_LOD0, p);
            }
        }
    }
}

#endif