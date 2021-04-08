using Sample_Server_Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample_Server
{
    class PacketHandler
    {
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"PlaerInfoReq : {p.playerId}, {p.name}");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine(($"Skill : {skill.id}, {skill.level}, {skill.duration}"));
                if (skill.attrubutes.Count != 0) Console.WriteLine($"Attributes: {skill.attrubutes[0].attName}");
            }
        }
    }
}
