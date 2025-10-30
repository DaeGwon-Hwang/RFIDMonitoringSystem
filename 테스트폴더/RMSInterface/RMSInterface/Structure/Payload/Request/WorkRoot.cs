using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace RMSInterface.Structure.Payload.Request
{
    /*
     * <?xml version="1.0" encoding="UTF-8"?>
     * <work_root>
     * ~~
     * </work_root>
     */
    
    [XmlRoot("work_root")]
    public class WorkRoot
    {
        [XmlElement("work_info")]
        public WorkInfo WorkInfo { get; set; }

        [XmlElement("send_info")]
        public SendInfo SendInfo { get; set; }

        [XmlElement("receive_info")]
        public ReceiveInfo ReceiveInfo { get; set; }

        [XmlElement("param_list")]
        public ParamList ParamList { get; set; }
    }

    public class WorkInfo
    {
        [XmlElement("work_key")]
        public string WorkKey { get; set; }

        [XmlElement("work_div")]
        public string WorkDiv { get; set; }

        [XmlElement("work_code")]
        public string WorkCode { get; set; }
    }

    public class SendInfo
    {
        [XmlAttribute("ip")]
        public string Ip { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }
    }

    public class ReceiveInfo
    {
        [XmlAttribute("ip")]
        public string Ip { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }
    }

    public class ParamList
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("param")]
        public List<Param> Params { get; set; } = new List<Param>();
    }

    public class Param
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
