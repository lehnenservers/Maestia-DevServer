using System;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Core;

namespace MaestiaDevServer.Handlers
{
    public static class ServerList
    {
        [Packet(12, 1)]
        public static void SPKUL_SERVER_RQ_GETLIST(Packet packetData, Client packetSender)
        {
            // === Concept of Serverlist ===
            // First packet server sends to client is DPKUL_SERVER_RS_LISTBEGIN to start serverlist transfer
            // Second packet is DPKUL_SERVER_RS_CHILD which tells the servername. This can be repeated multiple times for more servers
            // Third packet is DPKUL_SERVER_RS_LISTEND to indicate end of serverlist transfer
            // Fourth packet is DPKUL_SERVER_RS_ACTIVE to activate server and set playercount

            // DPKUL_SERVER_RS_LISTBEGIN
            var listBeginPacket = new Packet(12, 2);
            packetSender.SendPacket(listBeginPacket);

            // DPKUL_SERVER_RS_CHILD
            var childPacket = new Packet(12, 3);
            var serverName = "<GE> Delphi @ maestia.net";
            childPacket.WriteInt(0);//server ID starting from 0
            childPacket.WriteBytes(Encoding.Unicode.GetBytes(serverName));// ServerName
            childPacket.WriteBytes(new byte[160 - serverName.Length * 2]);// ServerName (remaining bytes)   server name must be 160 bytes long (80 unicode chars)
            childPacket.WriteInt(0);//??
            childPacket.WriteByte(0);//??
            childPacket.WriteByte(4);//num characters on this server -> max 4 // Replace later with SQL check
            packetSender.SendPacket(childPacket);

            var childPacket2 = new Packet(12, 3);
            var serverName2 = "<FR> Pectus @ maestia.net";
            childPacket2.WriteInt(1);
            childPacket2.WriteBytes(Encoding.Unicode.GetBytes(serverName2));
            childPacket2.WriteBytes(new byte[160 - serverName2.Length * 2]);
            childPacket2.WriteInt(0);
            childPacket2.WriteByte(0);
            childPacket2.WriteByte(0);
            packetSender.SendPacket(childPacket2);

            var childPacket3 = new Packet(12, 3);
            var serverName3 = "<EU> Papyrus @ maestia.net";
            childPacket3.WriteInt(2);
            childPacket3.WriteBytes(Encoding.Unicode.GetBytes(serverName3));
            childPacket3.WriteBytes(new byte[160 - serverName3.Length * 2]);
            childPacket3.WriteInt(0);
            childPacket3.WriteByte(0);
            childPacket3.WriteByte(0);
            packetSender.SendPacket(childPacket3);

            // DPKUL_SERVER_RS_LISTEND
            packetSender.SendPacket(new Packet(12, 5));

            // DPKUL_SERVER_RS_ACTIVE
            var activePacket = new Packet(12, 4);
            activePacket.WriteInt(0); //server id -> 0 = first one in list
            activePacket.WriteInt(0); //server condition: 0 to 25 - Normal, 26 to 80 - Busy, 81 or over - Full
            activePacket.WriteInt(1);  //boolean -> server active?
            packetSender.SendPacket(activePacket);

            var activePacket2 = new Packet(12, 4);
            activePacket2.WriteInt(1); //server id -> 0 = first one in list
            activePacket2.WriteInt(26); //server condition: 0 to 25 - Normal, 26 to 80 - Busy, 81 or over - Full
            activePacket2.WriteInt(1);  //boolean -> server active?
            packetSender.SendPacket(activePacket2);

            var activePacket3 = new Packet(12, 4);
            activePacket3.WriteInt(2); //server id -> 0 = first one in list
            activePacket3.WriteInt(81); //server condition: 0 to 25 - Normal, 26 to 80 - Busy, 81 or over - Full
            activePacket3.WriteInt(1);  //boolean -> server active?
            packetSender.SendPacket(activePacket3);
        }

        [Packet(12, 10)]
        public static void DPKUL_SERVER_RS_SELECT(Packet packetData, Client packetSender)
        {
            var serverID = packetData.ReadInt();  //server ID which the clients wants to connect to

            // TODO: check which serverID is being selected to transfer propper character information!!!
            // For now keep it static

            //DPK_HELO_RS_AGREE - Accept connection to zoneserver
            var serverSelectPacket = new Packet(12, 11);
            packetSender.SendPacket(serverSelectPacket);

            //DPK_HELO_RS_FORWARD - redirect to different address if necessary
            var serverForwardPacket = new Packet(1, 30);
            serverForwardPacket.WriteBytes(Encoding.Unicode.GetBytes("127.0.0.1:21001"));
            //packetSender.SendPacket(serverForwardPacket);

            // Do we need this anyway??
            //DSERVER_TYPE_ZONE - Actual zone server | Will throw error if login and zone server are same. Still works though.....
            var serverZonePacket = new Packet(1, 20);
            serverZonePacket.WriteByte(2);         // 2 = ZONE:Login
            serverZonePacket.WriteInt(0x07151257); // Engine Version
            serverZonePacket.WriteInt(0x01020000); // Client Version
            serverZonePacket.WriteInt(0x07041700); // Server Version
            serverZonePacket.WriteInt(1);
            packetSender.SendPacket(serverZonePacket);

            //DPKUZ_USER_RS_JOIN - Community server (zone)
            var zoneAckPacket = new Packet(51, 2);
            packetSender.SendPacket(zoneAckPacket);

            //DPKUZ_USER_RS_CHARLIST_BEGIN - Begin to transfer charlist data
            var characterListBeginPacket = new Packet(51, 12);
            packetSender.SendPacket(characterListBeginPacket);

            //###########################################################################################################################################################################

            //DPKUZ_USER_RS_CHARLIST_DATA - Actual Charlist data for 1 up to 4 chars sent in a row closed by the end packet
            var charNameX = "Plasma";
            var guildName = "Test Guild";

            var characterListDataPacket = new Packet(51, 13);

            characterListDataPacket.WriteInt(1);   // Char ID

            characterListDataPacket.WriteByte(8);  // class and gender | 1 warrior male | 2 warrior female | 3 mage male | 4 mage female | 5 archer male | 6 archer female | 7 priest male | 8 priest female
            characterListDataPacket.WriteByte(0);  // Must be 0
            characterListDataPacket.WriteByte(1);  // Must be 1
            characterListDataPacket.WriteByte(5);  // Must be 5

            characterListDataPacket.WriteShort(1); // guild rank 1 - 5 | 1 = guildmanager | 5 = guildnewcomer
            characterListDataPacket.WriteByte(0);  // Must be 0
            characterListDataPacket.WriteByte(10); // Must be 10

            characterListDataPacket.WriteBytes(Encoding.Unicode.GetBytes(charNameX));  //character name (40 chars)
            characterListDataPacket.WriteBytes(new byte[80 - charNameX.Length * 2]);

            characterListDataPacket.WriteBytes(Encoding.Unicode.GetBytes(guildName));  //guild name (40 chars)
            characterListDataPacket.WriteBytes(new byte[80 - guildName.Length * 2]);

            characterListDataPacket.WriteShort(241); // Face and Tatoo ID: (Find out all values later) 150 - 249 warrior male | 162 - 253 warrior female | 174 - 257 mage male | 186 - 1281? mage female | 198 - 145 archer male | 210 - 209 archer female | 222 - 273 priest male | 234 - 337 priest female
            characterListDataPacket.WriteByte(119);  // Hair ID: warrior male: 1 - 8 | warrior female: 17 - 24 | mage male: 33 - 40 | mage female: 49 - 56 | archer male: 65 - 72 | archer female: 81 - 88 | priest male: 97 - 104 | priest female: 113 - 120
            characterListDataPacket.WriteByte(0);    // 00 (placeholder)

            characterListDataPacket.WriteByte(255); // Hair Color B (reversed!)
            characterListDataPacket.WriteByte(255); // Hair Color G (reversed!)
            characterListDataPacket.WriteByte(255); // Hair Color R (reversed!)
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)

            characterListDataPacket.WriteByte(148); // Skin Color B (reversed!)
            characterListDataPacket.WriteByte(205); // Skin Color G (reversed!)
            characterListDataPacket.WriteByte(255); // Skin Color R (reversed!)
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)

            characterListDataPacket.WriteByte(5);   // Char Size from 0 to 10
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)

            characterListDataPacket.WriteByte(5);   // Char Weigth from 0 to 10
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)
            characterListDataPacket.WriteByte(0);   // 00 (placeholder)

            // 3D Model Torso
            characterListDataPacket.WriteByte(97); // warrior male = 4 | warrior female = 40 | mage male = 15 | mage female = 189 | archer male = 220 | archer female = 0 | priest male = 168 | priest female = 97
            characterListDataPacket.WriteByte(2);  // warrior both & archer male = 0 | mage female & archer female = 1 | mage male & priest both = 2
            characterListDataPacket.WriteByte(0);  // 00 (placeholder)
            characterListDataPacket.WriteByte(113);// placeholder

            // 3D Model hands/wrists
            characterListDataPacket.WriteByte(95); // warrior male = 2 | warrior female = 38 | mage male = 13 | mage female = 187 | archer male = 218 | archer female = 254 | priest male = 166 | priest female = 95
            characterListDataPacket.WriteByte(2);  // warrior both & archer male = 0 | mage female & archer female = 1 | mage male & priest both = 2
            characterListDataPacket.WriteByte(0);  // 00 (placeholder)
            characterListDataPacket.WriteByte(113);// placeholder

            // 3D Model Shoes
            characterListDataPacket.WriteByte(94); // warrior male = 1 | warrior female = 37 | mage male = 12 | mage female = 186 | archer male = 217 | archer female = 253 | priest male = 165 | priest female = 94
            characterListDataPacket.WriteByte(2);  // warrior both & archer male = 0 | mage female & archer female = 1 | mage male & priest both = 2
            characterListDataPacket.WriteByte(0);  // 00 (placeholder)
            characterListDataPacket.WriteByte(113);// placeholder

            // 3D Model Legs
            characterListDataPacket.WriteByte(96); // warrior male = 3 | warrior female = 39 | mage male = 14 | mage female = 188 | archer male = 219 | archer female = 255 | priest male = 167 | priest female = 96
            characterListDataPacket.WriteByte(2);  // warrior both & archer male = 0 | mage female & archer female = 1 | mage male & priest both = 2
            characterListDataPacket.WriteByte(0);  // 00 (placeholder)
            characterListDataPacket.WriteByte(113);// placeholder

            characterListDataPacket.WriteByte(100); // Char Level
            characterListDataPacket.WriteByte(0);   // Slot from 0 to 3
            characterListDataPacket.WriteByte(0);
            characterListDataPacket.WriteByte(0);

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // byte GAP[1];
            characterListDataPacket.WriteByte(0);   // Char locked? 0 = off / 1 = on

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteBytes(new byte[2]); //byte GAP_2[2];
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteShort(-1);  // -1 = no char deletion in progress | delete time in minutes - 72 hours = 4320

            characterListDataPacket.WriteByte(2);   // faction | 1 = SG | 2 = TK
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??
            characterListDataPacket.WriteByte(0);   // ??

            characterListDataPacket.WriteByte(0);   // ??
            packetSender.SendPacket(characterListDataPacket);

            //###########################################################################################################################################################################

            //Received DPKUZ_USER_RS_CHARLIST_END - End to transfer charlist data
            var characterListEndPacket = new Packet(51, 14);
            packetSender.SendPacket(characterListEndPacket);
        }
    }
}
