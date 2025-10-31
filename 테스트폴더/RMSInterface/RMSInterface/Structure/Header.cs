using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSInterface.Structure
{
    public class Header
    {
        /// <summary>
        /// Command Type	Request	Response	Remark
        /// Welcome Message	0x01	0x81	    1
        /// Send Data	    0x02	0x82	
        /// 
        /// 	Request Command의 범위는 0x00 ~ 0x7F 까지이다
        /// 	Response Command의 범위는 0x80 ~ oxFF 까지 이다. 
        /// 	Response Command의 값은 Request Command 의 값에 0x80 을 OR 연산한 값이다
        /// </summary>
        public ushort CommanType { get; set; }

        /// <summary>
        /// Status Type                                 	Request	Remark
        /// 성공	                                        0	
        /// 헤더의 바디 크기 정보와 실제 바디 크기가 다름	-1	
        /// 파일 시스템 IO 실패	                            -2	
        /// 배열 인덱스 값 초과	                            -3	
        /// XML 파싱 오류	                                -4	
        /// 기타 오류	                                    -5	
        /// </summary>
        public short Status { get; set; }

        public ushort Reserved1 { get; set; }
        public ushort Reserved2 { get; set; }
        public int TitleLength { get; set; }
        public int BodyLength { get; set; }

    }
}
