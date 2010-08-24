﻿/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2006, 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/
namespace MIConvexHullPluginNameSpace
{
    using System;
    using System.Collections.Generic;
    using StarMathLib;
    using System.Linq;

    /// <summary>
    /// functions called from Find for the 3D case. 
    /// </summary>
    public static partial class ConvexHull
    {

        private static void determineDimension(List<IVertexConvHull> vertices)
        {
            var r = new Random();
            var VCount = vertices.Count;
            var dimensions = new List<int>();
            for (int i = 0; i < 10; i++)
                dimensions.Add(vertices[r.Next(VCount)].location.GetLength(0));
            dimension = dimensions.Min();
            if (dimensions.Min() != dimensions.Max())
                Console.WriteLine("\n\n\n*******************************************\n" +
                    "Differing dimensions to vertex locations." +
                    "\nBased on a small sample, a value of " +
                    dimension.ToString() + "  will be used." +
                    "\n*******************************************\n\n\n");
        }
        #region Make functions





        private static SortedList<double, FaceData> initiateFaceDatabase()
        {
            for (int i = 0; i < dimension + 1; i++)
            {
                var vertices = new List<IVertexConvHull>(convexHull);
                vertices.RemoveAt(i);
                var newFace = MakeFace(vertices);
                /* the next line initialization of "verticesBeyond" is just to allow the line of
                 * code in updateFaces ("edge.Item2.verticesBeyond.Values).ToList());")
                 * to not crash when filling out the initial polygon. */
                newFace.verticesBeyond = new SortedList<double, IVertexConvHull>();
                convexFaces.Add(0.0, newFace);
            }
            for (int i = 0; i < dimension; i++)
                for (int j = i + 1; j < dimension + 1; j++)
                {
                    var edge = new List<IVertexConvHull>(convexHull);
                    edge.RemoveAt(j);
                    edge.RemoveAt(i);
                    var betweenFaces = (from f in convexFaces.Values
                                        where f.vertices.Intersect(edge).Count() == edge.Count()
                                        select f).ToList();
                    recordAdjacentFaces(betweenFaces[0], betweenFaces[1], edge);
                }
            return convexFaces;
        }

        private static void recordAdjacentFaces(FaceData face1, FaceData face2, List<IVertexConvHull> edge)
        {
            var vertexIndexNotOnEdge = (from v in face1.vertices
                                        where (!edge.Contains(v))
                                        select Array.IndexOf(face1.vertices, v)).FirstOrDefault();
            face1.adjacentFaces[vertexIndexNotOnEdge] = face2;

            vertexIndexNotOnEdge = (from v in face2.vertices
                                    where (!edge.Contains(v))
                                    select Array.IndexOf(face2.vertices, v)).FirstOrDefault();
            face2.adjacentFaces[vertexIndexNotOnEdge] = face1;
        }


        static FaceData MakeFace(IVertexConvHull currentVertex, List<IVertexConvHull> edge)
        {
            var vertices = new List<IVertexConvHull>(edge);
            vertices.Insert(0, currentVertex);
            return MakeFace(vertices);
        }
        static FaceData MakeFace(List<IVertexConvHull> vertices)
        {
            var outDir = new double[dimension];
            foreach (var v in vertices)
                outDir = StarMath.add(outDir, v.location);
            outDir = StarMath.divide(outDir, dimension);
            outDir = StarMath.subtract(outDir, center);
            double[] normal = findNormalVector(vertices);
            if (StarMath.multiplyDot(normal, outDir) < 0)
                normal = StarMath.subtract(StarMath.makeZeroVector(dimension), normal);
            FaceData newFace = new FaceData(dimension)
            {
                normal = normal,
                vertices = vertices.ToArray()
            };
            return newFace;
        }


        #endregion
        #region Find, Get and Update functions

        static List<FaceData> findFacesBeneathInitialVertices(SortedList<double, FaceData> convexFaces, IVertexConvHull currentVertex)
        {
            var facesUnder = new List<FaceData>();
            foreach (var face in convexFaces.Values)
            {
                double dummy;
                if (isVertexOverFace(currentVertex, face, out dummy))
                    facesUnder.Add(face);
            }
            return facesUnder;
        }


        private static List<FaceData> findAffectedFaces(FaceData currentFaceData, IVertexConvHull currentVertex,
    List<FaceData> primaryFaces = null)
        {
            if (primaryFaces == null) return findAffectedFaces(currentFaceData, currentVertex, new List<FaceData>() { currentFaceData });
            else
            {
                foreach (var adjFace in currentFaceData.adjacentFaces)
                    if (!primaryFaces.Contains(adjFace) &&
                        (adjFace.verticesBeyond.Values.Contains(currentVertex)))
                    {
                        primaryFaces.Add(adjFace);
                        findAffectedFaces(adjFace, currentVertex, primaryFaces);
                    }
                return primaryFaces;
            }
        }

        private static void updateFaces(List<FaceData> oldFaces, IVertexConvHull currentVertex)
        {
            var newFaces = new List<FaceData>();
            var affectedVertices = new List<IVertexConvHull>();
            var freeEdges = new List<Tuple<List<IVertexConvHull>, FaceData>>();
            foreach (var oldFace in oldFaces)
            {
                affectedVertices = affectedVertices.Union(oldFace.verticesBeyond.Values).ToList();
                convexFaces.RemoveAt(convexFaces.IndexOfValue(oldFace));
                for (int i = 0; i < oldFace.adjacentFaces.GetLength(0); i++)
                    if (!oldFaces.Contains(oldFace.adjacentFaces[i]))
                    {
                        var freeEdge = new List<IVertexConvHull>(oldFace.vertices);
                        freeEdge.RemoveAt(i);
                        freeEdges.Add(Tuple.Create(freeEdge, oldFace.adjacentFaces[i]));
                    }
            }
            affectedVertices.Remove(currentVertex);
            foreach (var edge in freeEdges)
            {
                var newFace = MakeFace(currentVertex, edge.Item1);
                recordAdjacentFaces(newFace, edge.Item2, edge.Item1);
                newFace.adjacentFaces[0] = edge.Item2;
                newFace.verticesBeyond = findBeyondVertices(newFace,
                    affectedVertices.Union(edge.Item2.verticesBeyond.Values).ToList());
                newFaces.Add(newFace);
            }
            for (int i = 0; i < newFaces.Count - 1; i++)
            {
                for (int j = i + 1; j < newFaces.Count; j++)
                {
                    var edge = newFaces[i].vertices.Intersect(newFaces[j].vertices).ToList();
                    if (edge.Count == dimension - 1)
                        recordAdjacentFaces(newFaces[i], newFaces[j], edge);
                }
            }
            foreach (var newFace in newFaces)
                if (newFace.verticesBeyond.Count == 0)
                    convexFaces.Add(-1.0, newFace);
                else convexFaces.Add(newFace.verticesBeyond.Keys[0], newFace);
        }

        private static double[] findNormalVector(List<IVertexConvHull> vertices)
        {
            double[] normal;
            if (dimension == 3 || dimension == 7)
                normal = StarMath.multiplyCross(StarMath.subtract(vertices[1].location, vertices[0].location),
                    StarMath.subtract(vertices[2].location, vertices[1].location));
            else
            {
                var b = new double[dimension];
            }
            return StarMath.normalize(normal);
        }


        private static SortedList<double, IVertexConvHull> findBeyondVertices(FaceData face, List<IVertexConvHull> vertices)
        {
            var beyondVertices = new SortedList<double, IVertexConvHull>(new noEqualSortMaxtoMinDouble());
            foreach (var v in vertices)
            {
                double dotP;
                if (isVertexOverFace(v, face, out dotP)) beyondVertices.Add(dotP, v);
            }
            return beyondVertices;
        }




        static void updateCenter(List<IVertexConvHull> convexHull, IVertexConvHull currentVertex)
        {
            center = StarMath.divide(StarMath.add(
                StarMath.multiply(convexHull.Count - 1, center),
                currentVertex.location),
                convexHull.Count);
        }
        #endregion

        static Boolean isVertexOverFace(IVertexConvHull v, FaceData f, out double dotP)
        {
            dotP = StarMath.multiplyDot(f.normal, StarMath.subtract(v.location, f.vertices[0].location));
            return (dotP >= 0);
        }

    }
}