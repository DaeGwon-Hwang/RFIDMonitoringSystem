using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RMSInterface.Structure.Payload.Response
{
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

        [XmlElement("return_list")]
        public ReturnList ReturnList { get; set; }
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

    public class ReturnList
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("return_row")]
        public List<ReturnRow> Rows { get; set; } = new List<ReturnRow>();
    }

    public class ReturnRow
    {
        [XmlElement("return_col")]
        public List<ReturnCol> Columns { get; set; } = new List<ReturnCol>();
    }

    public class ReturnCol
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
