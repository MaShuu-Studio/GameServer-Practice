using Sample_Server_Core;
using System;
using System.Collections.Generic;
using System.Text;

// 핸들러를 만들때에는 용도를 명확히 구분하는 것이 좋음.
// 클라 -> 서버, 서버 -> 클라 혹은 A서버 -> B서버 등등
class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;

        Console.WriteLine($"PlaerInfoReq : {p.playerId}, {p.name}");

        foreach (C_PlayerInfoReq.Skill skill in p.skills)
        {
            Console.WriteLine(($"Skill : {skill.id}, {skill.level}, {skill.duration}"));
            if (skill.attrubutes.Count != 0) Console.WriteLine($"Attributes: {skill.attrubutes[0].attName}");
        }
    }
}
