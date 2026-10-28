/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System.Collections.Generic;
using System.Linq;
using BHC = BH.oM.Adapters.Civil3D.Elements;
using BHG = BH.oM.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

using BH.oM.Base.Attributes;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a Civil 3D Alignment to BH.oM.Adapters.Civil3D.Elements.Alignment, with its plan geometry as one PolyCurve.")]
        [Input("civAlignment", "A Civil 3D Alignment to convert.")]
        [Output("alignment", "The converted BH.oM.Adapters.Civil3D.Elements.Alignment.")]
        public static BHC.Alignment FromCivil3D(this ADC.Alignment civAlignment)
        {
            BHC.Alignment alignment = new BHC.Alignment
            {
                AlignmentType = civAlignment.AlignmentType.FromCivil3D(),
                Description = civAlignment.Description,
                Curve = AlignmentPolyCurve(civAlignment),
                Name = civAlignment.Name,
                ReferencePoint = civAlignment.ReferencePoint.FromCivil3D(),
                ReferencePointStation = civAlignment.ReferencePointStation,
                SiteName = civAlignment.SiteName,
                StartingStation = civAlignment.StartingStation,
            };

            return alignment;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //Joins the alignment's lines, arcs and spirals into one PolyCurve. The Entities collection is in
        //creation order, so the sub-entities are sorted by station to keep the PolyCurve continuous.
        private static BHG.PolyCurve AlignmentPolyCurve(ADC.Alignment civAlignment)
        {
            List<ADC.AlignmentSubEntity> subEntities = new List<ADC.AlignmentSubEntity>();
            foreach (ADC.AlignmentEntity entity in civAlignment.Entities)
            {
                for (int i = 0; i < entity.SubEntityCount; i++)
                    subEntities.Add(entity[i]);
            }

            List<BHG.ICurve> segments = new List<BHG.ICurve>();
            bool spiralApproximated = false;

            foreach (ADC.AlignmentSubEntity subEntity in subEntities.OrderBy(x => x.StartStation))
            {
                BHG.Point start = subEntity.StartPoint.FromCivil3D();
                BHG.Point end = subEntity.EndPoint.FromCivil3D();

                if (subEntity.SubEntityType == ADC.AlignmentSubEntityType.Arc)
                {
                    double easting = 0, northing = 0;
                    civAlignment.PointLocation((subEntity.StartStation + subEntity.EndStation) / 2, 0, ref easting, ref northing);
                    BHG.Point mid = new BHG.Point { X = easting, Y = northing };
                    segments.Add(BH.Engine.Geometry.Create.Arc(start, mid, end));
                }
                else
                {
                    if (subEntity.SubEntityType != ADC.AlignmentSubEntityType.Line)
                        spiralApproximated = true;

                    segments.Add(new BHG.Line { Start = start, End = end });
                }
            }

            if (spiralApproximated)
                BH.Engine.Base.Compute.RecordWarning("Alignment spiral segments are approximated as straight lines.");

            return new BHG.PolyCurve { Curves = segments };
        }

        /***************************************************/
    }
}


