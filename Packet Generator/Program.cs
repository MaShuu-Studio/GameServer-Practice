using System;
using System.Xml;

namespace Packet_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent();
                while(r.Read())
                {
                    // Depth가 1이고 정보가 시작될때 Parsing 진행
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element) ParsePacket(r);
                }
            }            
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement) return;
            if (r.Name.ToLower() != "packet") return;

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            ParseMembers(r);
        }

        public static void ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            // packet의 변수들을 Parsing해주는 작업.
            int depth = r.Depth + 1;
            while (r.Read())
            {
                if (r.Depth != depth) break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                string memeberType = r.Name.ToLower();
                
                switch (memeberType)
                {
                    case "bool": break;
                    case "byte": break;
                    case "short": break;
                    case "ushort": break;
                    case "int": break;
                    case "long": break;
                    case "float": break;
                    case "double": break;
                    case "string": break;
                    case "list": break;
                }
            }
        }
    }
}
