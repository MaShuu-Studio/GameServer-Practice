using System;
using System.IO;
using System.Xml;

namespace Packet_Generator
{
    class Program
    {
        static string genPackets;
        static ushort packetId = 0;
        static string packetEnums;
        static void Main(string[] args)
        {
            string pdlPath = "../PDL.xml";
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            // argument 받아서 Path로 활용
            if (args.Length >= 1) pdlPath = args[0];

            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                r.MoveToContent();
                while(r.Read())
                {
                    // Depth가 1이고 정보가 시작될때 Parsing 진행
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element) ParsePacket(r);
                }

                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText);
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

            Tuple<string, string, string> t = ParseMembers(r);
            if (t != null)
            {
                genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);
                packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, packetId++) + "\n\t";
            }
        }

        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];
            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            // packet의 변수들을 Parsing해주는 작업.
            int depth = r.Depth + 1;
            while (r.Read())
            {
                if (r.Depth != depth) break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                {
                    memberCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(readCode) == false)
                {
                    readCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(writeCode) == false)
                {
                    writeCode += Environment.NewLine;
                }

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                        break;
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "short":
                    case "ushort":
                    case "int":
                    case "long": 
                    case "float": 
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.stringReadFormat, memberName);
                        writeCode += string.Format(PacketFormat.stringWriteFormat, memberName, memberType);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];

            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("Member without name");
                return null;
            }

            Tuple<string,string,string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.memberListFormat, FirstCharToUpper(listName), FirstCharToLower(listName), t.Item1, t.Item2, t.Item3);
            string readCode = string.Format(PacketFormat.listReadFormat, FirstCharToUpper(listName), FirstCharToLower(listName));
            string writeCode = string.Format(PacketFormat.listWriteFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }
        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
            }
            return "";
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }
        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}
