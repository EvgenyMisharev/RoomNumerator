using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace RoomNumerator
{
    class CoordinatesBasedComparerXYUp : IComparer<XYZ>
    {
        public int Compare(XYZ a, XYZ b)
        {
            if ((Math.Round(a.X, 6) == Math.Round(b.X, 6)) && (Math.Round(a.Y, 6) == Math.Round(b.Y, 6)))
                return 0;
            if ((Math.Round(a.Y, 6) < Math.Round(b.Y, 6)) || (Math.Round(a.Y, 6) == Math.Round(b.Y, 6) && (Math.Round(a.X, 6) < Math.Round(b.X, 6))))
            {
                return 1;
            }
            return -1;
        }
    }
}
