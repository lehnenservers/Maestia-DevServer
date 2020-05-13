using System;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Core;

namespace MaestiaDevServer.Handlers
{
    public static class ZoneLogin
    {
        [Packet(51, 31)]
        public static void CLIENT_ZONE_LOGIN_WITH_CHAR(Packet packetData, Client packetSender)
        {
            var iLength = packetData.Length;
            var loginPacket = BitConverter.ToString(packetData.ReadBytes(iLength));

            Console.WriteLine(loginPacket); // raw data output
            Console.WriteLine("Total length: " + iLength);

            // Structure: Length 657
            // Selected Char Slot (0 - 3)
            // ??

            // DPKUZ_USER_RS_BEGIN_PCINFO
            var beginPCInfoPacket = new Packet(51, 22);
            packetSender.SendPacket(beginPCInfoPacket);

            // DPKUZ_USER_RS_END_PCINFO
            var endPCInfoPacket = new Packet(51, 23);
            //endPCInfoPacket.WriteByte(196);
            endPCInfoPacket.WriteInt(50397185);
            packetSender.SendPacket(endPCInfoPacket);

            //[AvaGameZone::MoveZone] Move Zone Start
            var moveZonePacket = new Packet(63, 2);
            moveZonePacket.WriteInt(50397185);
            //packetSender.SendPacket(moveZonePacket);

            //DPKUZ_USER_RS_START_PERMISSION
            var permissionMoveZonePacket = new Packet(51, 35);
            //packetSender.SendPacket(permissionMoveZonePacket);

            //DPKUZ_USER_RS_START_GAME
            var enterWorldPacket2 = new Packet(51, 26);
            //packetSender.SendPacket(enterWorldPacket2);
        }
    }
}
