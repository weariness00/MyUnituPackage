namespace Weariness.Util
{
    public partial class ObjectGrid
    {
        private static void GetSortAxis(ObjectGridSortType type, out int xDir, out int yDir, out int zDir)
        {
            switch (type)
            {
                case ObjectGridSortType.LeftUpForward:    xDir = -1; yDir =  1; zDir =  1; break;
                case ObjectGridSortType.LeftDownForward:  xDir = -1; yDir = -1; zDir =  1; break;
                case ObjectGridSortType.LeftUpBack:       xDir = -1; yDir =  1; zDir = -1; break;
                case ObjectGridSortType.LeftDownBack:     xDir = -1; yDir = -1; zDir = -1; break;
        
                case ObjectGridSortType.RightUpForward:   xDir =  1; yDir =  1; zDir =  1; break;
                case ObjectGridSortType.RightDownForward: xDir =  1; yDir = -1; zDir =  1; break;
                case ObjectGridSortType.RightUpBack:      xDir =  1; yDir =  1; zDir = -1; break;
                case ObjectGridSortType.RightDownBack:    xDir =  1; yDir = -1; zDir = -1; break;
        
                default:                                  xDir = -1; yDir =  1; zDir =  1; break;
            }
        }
    }
}