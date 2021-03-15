// Copyright 2014 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.BusinessObjects.
// SharpMap.BusinessObjects is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.BusinessObjects is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap.Rendering.Business
{
    /// <summary>
    /// Interface for business object renderers
    /// </summary>
    /// <typeparam name="T">The type of the business object</typeparam>
    public interface IBusinessObjectRenderer<in T>
    {
        /// <summary>
        /// Method to start the rendering of business objects
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        void StartRendering(Graphics g, MapViewport map);

        /// <summary>
        /// Method to render each individual business object
        /// </summary>
        /// <param name="businessObject">The business object to render</param>
        Rectangle Render(T businessObject);

        /// <summary>
        /// Method to finalize rendering of business objects
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        void EndRendering(Graphics g, MapViewport map);

        /// <summary>
        /// Gets or sets a value indicating the transformation that is to be applied on the geometry prior to rendering
        /// </summary>
        Func<IGeometry, IGeometry> Transformation { get; set; }
    }
}
