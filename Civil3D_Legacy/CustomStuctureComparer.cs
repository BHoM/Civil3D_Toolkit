using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Civil3D_Toolkit
{
    public class CustomStuctureComparer: IComparer<string>, IComparer<Structure>
    {
        public int Compare(string x, string y)
        {
            if (x == y || x == null || y == null)
            {
                return 0;
            }

            Regex rgx = new Regex(@"\d+(?:\.\d+)?");

            if (rgx.IsMatch(x) && rgx.IsMatch(y))
            {
                decimal xNumber = Convert.ToDecimal(rgx.Match(x).Value);
                decimal yNumber = Convert.ToDecimal(rgx.Match(y).Value);

                if (xNumber > yNumber)
                {
                    return 1;
                }

                if (xNumber < yNumber)
                {
                    return -1;
                }
            }

            return 0;
        }
        public int Compare(Structure x, Structure y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return Compare(x.Name, y.Name);
        }
    }
}
