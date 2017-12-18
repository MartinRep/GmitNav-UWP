using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmitNavUWP
{
    public class Util
    {
        public static Double MAX_ZOOM = 19.9;
        public static Double MIN_ZOOM = 18.3;
        public static String DATAFILE = "rooms.json";

        public class Neo4j
        {
            public static String uri = "http://hobby-nbpcbkgcjildgbkecajnbbpl.dbs.graphenedb.com:24789/db/data/transaction/commit";
            public static String username = "my-app-development";
            public static String password = "b.1aPt1K5V9QuX.cBsrnJPUmsTDMbos";
        }

        public class Gmit
        {
            public static double LAT = 53.2786283857855;
            public static double LNG = -9.01045782475004;
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
                public static double SOUTH = 53.278038;
                public static double WEST = -9.01295;
                public static double NORTH = 53.279488;
                public static double EAST = -9.00876;
            }

            public class New
            {
                public static double SOUTH = 53.277276;
                public static double WEST = -9.01195;
                public static double NORTH = 53.278235;
                public static double EAST = -9.00911;
            }

        }

        public class Buttons
        {
            public static double LAT = 53.2795008798393;
            public static double LNG = -9.01174496872789;
        }
    }
}
