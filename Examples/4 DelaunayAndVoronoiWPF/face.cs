﻿/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
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
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ExampleWithGraphics
{
    using MIConvexHull;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class face : TriangulationCell<vertex, face>
    {
        public class FaceVisual : Shape
        {
            face f;

            protected override Geometry DefiningGeometry
            {
                get
                {
                    var myPathGeometry = new PathGeometry();
                    var pathFigure1 = new PathFigure
                    {
                        StartPoint = new Point(f.Vertices[0].Position[0], f.Vertices[0].Position[1])
                    };
                    for (int i = 1; i < f.Vertices.GetLength(0); i++)
                        pathFigure1.Segments.Add(
                            new LineSegment(
                                new Point(f.Vertices[i].Position[0],
                                          f.Vertices[i].Position[1]), true));
                    pathFigure1.IsClosed = true;
                    myPathGeometry.Figures.Add(pathFigure1);


                    return myPathGeometry;
                }
            }

            public FaceVisual(face f)
            {
                Stroke = Brushes.Blue;
                StrokeThickness = 1.0;
                Opacity = 0.5;
                this.f = f;
            }
        }

        public Shape Visual { get; private set; }

        public face()
        {
            Visual = new FaceVisual(this);
        }      
    }
}
