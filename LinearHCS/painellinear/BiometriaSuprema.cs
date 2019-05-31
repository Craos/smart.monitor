using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearHCS
{
    class BiometriaSuprema
    {
        //---------------------------------------------------------------
        // Constantes
        //---------------------------------------------------------------
        // Appendix E. Error Code Lis
        public const int
            // command
            CMD_SW = 0x01,            // FS - Free Scan  *
            CMD_ST = 0x21,            // ST - Scan Template
            CMD_IS = 0x11,            // IS - Identify by Scan
            CMD_DT = 0x16,            // DT - Delete Template
            CMD_RT = 0x14,            // RT - Read Template
            CMD_ET = 0x07,            // ET - Enroll by Template
            CMD_IT = 0x13,            // IT - Identify by Template
            CMD_CA = 0x60,            // CA - Cancel
            CMD_DA = 0x17,            // DA - Delete All Templates
            CMD_ID = 0x85,			// ID - Get the module ID
            CMD_ES = 0x05,            // ES - Enroll by Scan
            CMD_SR = 0x03,            // SR - System Parameter Read
            CMD_LT = 0x18,            // LT - List User ID
            CMD_RTX = 0x89,            // RTX - Read Template with Extended Protocol
            CMD_RTX_DATA_OK = 0x86,            // RTX DATA OK - Sinaliza ao Suprema o recebimento correto dos bytes  
            CMD_ETX = 0x87,            // ETX - Enroll By Template with extended protocol
            CMD_RS = 0xD0,            // RS  - Reset Module
            CMD_LIGHTINING = 0x90,            // 
            CMD_SENSIBILIDADE = 0x80,            // 
            CMD_WR = 0x02,            // 
            // resp
            SFM_SCAN_SUCCESS = 0x62,
            SFM_SCAN_FAIL = 0x63,
            SFM_TIME_OUT = 0x6C,
            SFM_SUCCESS = 0x61,
            SFM_TRY_AGAIN = 0x6B,
            SFM_NOT_FOUND = 0x69,
            SFM_TIMEOUT_MATCH = 0x7A,
            SFM_REJECTED_ID = 0x90,
            SFM_DURESS_FINGER = 0x91,
            SFM_ENTRANCE_LIMIT = 0x94,
            SFM_CONTINUE = 0x74,
            SFM_MEM_FULL = 0x6D,
            SFM_FINGER_LIMIT = 0x72,
            SFM_INVALID_ID = 0x76,
            SFM_EXIST_ID = 0x6E,
            SFM_EXIST_FINGER = 0x86,
            SFM_BUSY = 0x80,
            // cfg parameter
            PARAMETER_TIMEOUT = 0x62,
            PARAMETER_TEMPLATE_SIZE = 0x64,
            PARAMETER_ENROLL_MODE = 0x65,
            PARAMETER_SECURITY_LEVEL = 0x66,
            PARAMETER_ENCRYPTION_MODE = 0x67,
            PARAMETER_SENSOR_TYPE = 0x68,
            PARAMETER_IMAGE_FORMAT = 0x6C,
            PARAMETER_MODULE_ID = 0x6D,
            PARAMETER_FIRMWARE_VERSION = 0x6E,
            PARAMETER_SERIAL_NUMBER = 0x6F,
            PARAMETER_BAUDRATE = 0x71,
            PARAMETER_BAUDRATE2 = 0x72,
            PARAMETER_ENROLLED_FINGER = 0x73,
            PARAMETER_AVAILABLE_FINGER = 0x74,
            PARAMETER_SEND_SCAN_SUCCESS = 0x75,
            PARAMETER_ASCII_PACKET = 0x76,
            PARAMETER_ROTATE_IMAGE = 0x77,
            PARAMETER_ROTATION = 0x78,
            PARAMETER_SENSITIVITY = 0x80,
            PARAMETER_IMAGE_QUALITY = 0x81,
            PARAMETER_AUTO_RESPONSE = 0x82,
            PARAMETER_NETWORK_MODE = 0x83,
            PARAMETER_FREE_SCAN = 0x84,
            PARAMETER_PROVISIONAL_ENROLL = 0x85,
            PARAMETER_PASS_WHEN_EMPTY = 0x86,
            PARAMETER_RESPONSE_DELAY = 0x87,
            PARAMETER_MATCHING_TIMEOUT = 0x88,
            PARAMETER_BUILD_NUMBER = 0x89,
            PARAMETER_ENROLL_DISPLACEMENT = 0x8A,
            PARAMETER_LIGHTING_CONDITION = 0x90,
            PARAMETER_FREE_SCAN_DELAY = 0x91,
            PARAMETER_FAST_MODE = 0x93,
            PARAMETER_WATCHDOG = 0x94,
            PARAMETER_TEMPLATE_TYPE = 0x95,
            // enroll options
            ENROLL_NONE = 0x00, // sobrescreve
            ENROLL_ADD_NEW = 0x71,
            ENROLL_AUTO_ID = 0x79,
            ENROLL_CONTINUE = 0x74,
            ENROLL_CHECK_ID = 0x70,
            ENROLL_CHECK_FINGER = 0x84,
            ENROLL_CHECK_FINGER_AUTO_ID = 0x85,
            ENROLL_ADD_DURESS = 0x92,
            // delete options
            DELETE_ONLY_ONE = 0x70,
            DELETE_MULTIPLE_ID = 0x71; 
 


        // Start code  Command  Param   Size   Flag/Error  Checksum  End code
        // 1byte       1byte    4bytes  4bytes 1byte       1byte     1byte 
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] constroiComandoSuprema(byte Command, UInt32 Param, UInt32 Size, byte FlagError, byte[] packet )
        {
            int i = 0, j;
            byte[] buf = new byte[13];

            buf[i++] = 0x40;
            buf[i++] = Command;
            buf[i++] = (byte)(Param & 0x000000FF);
            buf[i++] = (byte)(Param >> 8);
            buf[i++] = (byte)(Param >> 16);
            buf[i++] = (byte)(Param >> 24);
            buf[i++] = (byte)(Size & 0x000000FF);
            buf[i++] = (byte)(Size >> 8);
            buf[i++] = (byte)(Size >> 16);
            buf[i++] = (byte)(Size >> 24);
            buf[i++] = FlagError;
            for (j = 0; j < 11; j++) buf[i] += buf[j]; // checksum
            i++;
            buf[i++] = 0x0A;

            if (packet != null )
            {
                Array.Resize<byte>(ref buf, (int)(13 + Size));
                for (j = 0; j < packet.Length; j++) buf[i] += packet[j]; // exChecksum
            }
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] SUPREMA_CMD_ES( UInt16 id , byte flags )
        {
            return constroiComandoSuprema(CMD_ES, id, 0, flags, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] SUPREMA_CMD_IS()
        {
            return constroiComandoSuprema(CMD_IS, 0, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] SUPREMA_CMD_DT(UInt16 id)
        {
            return constroiComandoSuprema(CMD_DT, id, 0, DELETE_ONLY_ONE , null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] SUPREMA_CMD_DA()
        {
            return constroiComandoSuprema(CMD_DA, 0, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] SUPREMA_CMD_RT(UInt16 id)
        {
            return constroiComandoSuprema(CMD_RT, id, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] SUPREMA_CMD_SW(byte flag, byte parametro )
        {
            return constroiComandoSuprema(CMD_SW, 0, flag, parametro, null);
        }

    }
}
