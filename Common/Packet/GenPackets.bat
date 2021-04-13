start ../../"Packet Generator"/bin/"PacketGenerator.exe" ../../"Packet Generator"/PDL.xml

timeout 1

xcopy /Y GenPackets.cs "../../Dummy Client/Packet"
xcopy /Y GenPackets.cs "../../Sample Server/Packet"
xcopy /Y ClientPacketManager.cs "../../Dummy Client/Packet"
xcopy /Y ServerPacketManager.cs "../../Sample Server/Packet"


xcopy /Y GenPackets.cs "D:\Users\Unity\2D\Dungeon-is-Self\Assets\Scripts\Network\Packet"
xcopy /Y ClientPacketManager.cs "D:\Users\Unity\2D\Dungeon-is-Self\Assets\Scripts\Network\Packet"