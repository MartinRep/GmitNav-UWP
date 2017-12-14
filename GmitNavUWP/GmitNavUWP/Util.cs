using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmitNavUWP
{
    public class Util
    {
        public static float MAX_ZOOM = 20f;
        public static float MIN_ZOOM = 15f;
        public static String DATAFILE = "rooms.json";

        public class Neo4j
        {
            public static String uri = "http://hobby-nbpcbkgcjildgbkecajnbbpl.dbs.graphenedb.com:24789/db/data/transaction/commit";
            public static String username = "my-app-development";
            public static String password = "b.1aPt1K5V9QuX.cBsrnJPUmsTDMbos";
        }

        public class Gmit
        {
            public static double LAT = 53.2785;
            public static double LNG = -9.01;
        }

        public class CameraBoundries
        {
            public static double NORTH = 53.277;
            public static double EAST = -9.0125;
            public static double SOUTH = 53.28;
            public static double WEST = -9.0082;
        }
        public class Building
        {
            public class Old
            {
                public static double NORTH = 53.278038;
                public static double EAST = -9.01287;
                public static double SOUTH = 53.27959;
                public static double WEST = -9.00876;
            }

            public class New
            {
                public static double SOUTH = 53.277276;
                public static double WEST = -9.01195;
                public static double NORTH = 53.278235;
                public static double EAST = -9.00911;
            }
        }
    }
}
