using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.TransitiveMiddleware.Comm
{
    class ID
    {
        //Public Element정의

        //좌표변환관련
        public const double BESSEL_MAJOR = 6377397.155;
        public const double WGS84_MAJOR = 6378137;
        public const double BESSEL_MINOR = 6356078.96325;
        public const double WGS84_MINOR = 6356752.3142;

        public const double TM_SOUTH_FACTOR = 1;
        public const double TM_CENTER_FACTOR = 1;
        public const double TM_EASTH_FACTOR = 1;
        public const double KATEC_FACTOR = 0.9999;
        public const double UTM_Z52_FACTOR = 0.9996;
        public const double UTM_Z51_FACTOR = 0.9996;

        public const double TM_SOUTH_LONG_CEN = 2.18171200985643;
        public const double TM_CENTER_LONG_CEN = 2.21661859489632;
        public const double TM_EASTH_LONG_CEN = 2.2515251799362;
        public const double KATEC_LONG_CEN = 2.23402144255274;
        public const double UTM_Z52_LONG_CEN = 2.25147473507269;
        public const double UTM_Z51_LONG_CEN = 2.14675497995303;

        public const double TM_SOUTH_LAT_CEN = 0.663225115757845;
        public const double TM_CENTER_LAT_CEN = 0.663225115757845;
        public const double TM_EASTH_LAT_CEN = 0.663225115757845;
        public const double KATEC_LAT_CEN = 0.663225115757845;
        public const double UTM_Z52_LAT_CEN = 0;

        public const double TM_SOUTH_FALSE_N = 500000;
        public const double TM_CENTER_FALSE_N = 500000;
        public const double TM_EASTH_FALSE_N = 500000;
        public const double KATEC_FALSE_N = 600000;
        public const double UTM_Z52_FALSE_N = 0;
        public const double UTM_Z51_FALSE_N = 0;

        public const double TM_SOUTH_FALSE_E = 200000;
        public const double TM_CENTER_FALSE_E = 200000;
        public const double TM_EASTH_FALSE_E = 200000;
        public const double KATEC_FALSE_E = 400000;
        public const double UTM_Z52_FALSE_E = 500000;
        public const double UTM_Z51_FALSE_E = 500000;

        public const double EPSLN = 0.0000000001;

        public const int TM_SOUTH = 1;	//서부원점
        public const int TM_CENTER = 2;  //중부원점
        public const int TM_EAST = 3;	//동부원점       
    }
}
