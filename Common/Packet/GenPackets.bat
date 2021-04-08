start ../../"Packet Generator"/bin/"Packet Generator.exe" ../../"Packet Generator"/PDL.xml
xcopy /Y GenPackets.cs "../../Dummy Client/Packet"
xcopy /Y GenPackets.cs "../../Sample Server/Packet"
xcopy /Y ClientPacketManager.cs "../../Dummy Client/Packet"
xcopy /Y ServerPacketManager.cs "../../Sample Server/Packet"