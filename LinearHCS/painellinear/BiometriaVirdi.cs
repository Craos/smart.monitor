using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UNIONCOMM.SDK.UCBioBSP;

namespace LinearHCS
{
    class BiometriaVirdi
    {
        //---------------------------------------------------------------
        // Estruturas
        //---------------------------------------------------------------
        /*
        1.1 Packet Structure   
 
        Communication between main controller and UFOM is executed with a 20-byte packet. In 
        case a field is composed of 2 bytes, the lower byte becomes the first byte of the field and 
        the higher byte becomes the second byte (Little Endian).   
 
        1Byte 1Byte    2Byte   4Byte  4Byte  4Byte      2Byte     1Byte    1Byte 
        Start Command  wParam  Param1 Param2 exDatalen  ErrorCode Checksum Exchecksum 
        */

        /*
        public struct TCmdPacket
        {
            public byte Stx;
            public byte Command;
            public UInt16 wParam;
            public UInt32 Param1;
            public UInt32 Param2;
            public UInt32 exDatalen;
            public UInt16 ErrorCode;
            public byte Checksum;
            public byte exChecksum;
        };
        // Time structure 
        public struct TimeInfo 
        {
            public UInt16 Year;    // 1999~ 
            public byte Month;   // 1 ~ 12 
            public byte Day;     // 1 ~ 31 
            public byte Hour;    // 00 - 23 
            public byte Min;     // 0 - 59 
            public byte Sec;     // 0 – 59 
            public byte Reserved; 
        }
 
        // User Record 
        public struct TUserRecord 
        {
            public UInt32 UserId;
            public byte UserFlag;
            public byte[] Reserved1;  //3
            public TimeInfo Time;
            public byte[] Reserved2;  //12
            public byte[] pwd;  //8
            public byte[] cardnum; //20 
        }

        // Fingerprint Record 
        public struct TFPRecord 
        {
            public byte type;  // 0x01 
            public UInt16 len;   //Fingerprint data length (800) 
            public byte[] Minutiae1; //[400]; 
            public byte[] Minutiae2; //[400]; 
        } 
 
        // Log Record 
        public struct TLogRecord
        {
            public UInt32 UserId;
            public TimeInfo Time;
            public byte Mode;  // Arriving/Leaving/Away from the work 
            public byte Type;  // Method of verifying fingerprint/password 
            public byte Result;  // Result 
            public byte Reserved;   
        }
        */ 

        //---------------------------------------------------------------
        // Constantes
        //---------------------------------------------------------------
        // Appendix E. Error Code Lis
        public const int
            M2ERROR_NONE = 0x00,
            M2ERROR_FLASH_OPEN = 0x01,
            M2ERROR_SENSOR_OPEN = 0x02,
            M2ERROR_REGISTER_FAILED = 0x03,
            M2ERROR_VERIFY_FAILED = 0x04,
            M2ERROR_ALREADY_REGISTERED_USER = 0x05,
            M2ERROR_USER_NOT_FOUND = 0x06,
            M2ERROR_INVALID_PASSWORD = 0x07,
            M2ERROR_TIMEOUT = 0x08,
            M2ERROR_DB_FULL = 0x09,
            M2ERROR_DB_WRONG_USERID = 0x0A,
            M2ERROR_DB_NO_DATA = 0x0B,
            M2ERROR_EXTRACT_FAIL = 0x0C,
            M2ERROR_MEMALLOC_FAILED = 0x0D,
            M2ERROR_SERIAL_OPEN = 0x0E,
            M2ERROR_NOT_IMPLEMENTED = 0x0F,
            M2ERROR_FUNCTION_FAIL = 0x10,
            M2ERROR_INSUFFICIENT_DATA = 0x11,
            M2ERROR_FLASH_WRITE_ERROR = 0x12,
            M2ERROR_FLASH_READ_ERROR = 0x13,
            M2ERROR_INVALID_PARAM = 0x14,
            M2ERROR_MASTERFP_NOT_FOUND = 0x15,
            M2ERROR_MASTERCOUNT_EXCEED = 0x16,
            M2ERROR_AUTHENTICATION_FAIL = 0x17,
            M2ERROR_FPCHANGE_FAILED = 0x1A,
            M2ERROR_IDENTIFY_FAILED = 0x1B,
            M2ERROR_FLASH_ERASE_ERROR = 0x1C,
            M2ERROR_UNKNOWN_COMMAND = 0xFF,
            M2ERROR_VERIFY_FAKE = 0x1D,
            M2ERROR_TIME_ERROR = 0x1E,
            M2ERROR_SEARCHING_FOR_IDENTIFY = 0x1F,
            M2ERROR_INVALID_USERDATA_SIZE = 0x20,
            M2ERROR_INVALID_USERDATA_ADDRESS = 0x21,
            M2ERROR_MUST_BE_SET_DATA_LENGTH = 0x22,
            M2ERROR_CODE_CHECKSUM_ERROR = 0x29,
            M2ERROR_FPCOUNT_EXCEED = 0x80,
            M2ERROR_CONVERT_FPDATA_ENCRYPTION = 0x30,
            M2ERROR_CONVERT_FPDATA_DECRYPTION = 0x31,
            M2ERROR_CONVERT_FPDATA_CHECKSUM = 0x32,
            M2ERROR_CHECKSUM_ERROR = 0x28,
            M2ERROR_CONVERT_FPDATA_NOEXTEND = 0x33,
            M2ERROR_CONVERT_FPDATA_OTHERTYPE = 0x34,
            // SETUP
            SI_USING_MASTER_AUTHENTICATION = 0x00,
            SI_SAVE_LOGEVENT = 0x01,
            SI_SECURITY_LEVEL = 0x02,
            SI_USING_RELAY = 0x03,
            SI_COMM_SPEED = 0x04,
            SI_EXP_COARSE = 0x05,
            SI_EXP_FINE = 0x06,
            SI_EXP_GAIN = 0x07,
            SI_SECURITY_LEVEL_IDENTIFY = 0x08,
            SI_WIEGAND_FORMAT = 0x09,
            SI_WIEGAND_SITECODE = 0x0a,
            SI_REGISTER_QUALITY = 0x0b,
            SI_VERIFY_QUALITY = 0x0c,
            SI_LFD_LEVEL = 0x1a,
            SI_ROTATE_CAPTURE = 0x1b,
            // basic commands
            CMD_GET_VERSION = 0x05,
            CMD_IS_ROOT_MASTER = 0x06,
            CMD_GET_SYSTEM_ID = 0x0e,
            CMD_SET_SYSTEM_ID = 0x0f,
            CMD_DEVICE_TEST = 0x10,
            CMD_RELAY_ONOFF = 0x12,
            CMD_OPTICLED_ONOFF = 0x14,
            CMD_EXP_AUTOTUNNING = 0x16,
            CMD_AUTO_ONOFF = 0x1a,
            CMD_SET_SYSTEM_INFO = 0x20,
            CMD_SET_COMM_SPEED = 0x21,
            CMD_SET_TIME = 0x24,
            CMD_SET_CAPTURE_TIMEOUT = 0x28,
            CMD_GET_SYSTEM_INFO = 0x30,
            CMD_GET_MINUTIAE = 0x40,
            CMD_GET_IMAGE = 0x43,
            CMD_GET_TIME = 0x44,
            CMD_GET_REC_COUNT = 0x46,
            CMD_GET_MASTER_COUNT = 0x47,
            CMD_GET_CAPTURE_TIMEOUT = 0x48,
            CMD_FP_REGISTER_START = 0x50,
            CMD_FP_REGISTER_END = 0x51,
            CMD_FP_CHANGE_START = 0x52,
            CMD_FP_CHANGE_END = 0x53,
            CMD_FP_DELETE = 0x54,
            CMD_FP_VERIFY = 0x55,
            CMD_FP_IDENTIFY = 0x56,
            CMD_FP_VERIFY_MASTER = 0x57,
            CMD_FP_VERIFY_END = 0x58,
            CMD_FP_IDENTIFY_EX = 0x59,
            CMD_FP_MIN_VERIFY = 0x5c,
            CMD_FP_CANCEL = 0x5d,
            CMD_FP_CARD_MIN_VERIFY = 0x87,
            CMD_GET_FP_CARD_MIN = 0x8c,
            CMD_FP_ADD_START = 0x5e,
            CMD_FP_ADD_END = 0x5f,
            CMD_GET_PW = 0x6c,
            CMD_PW_VERIFY = 0x6d,
            CMD_PW_ADD = 0x6e,
            CMD_PW_DELETE = 0x6f,
            CMD_DEVICE_INITIALIZE = 0x88,
            CMD_SELECT_ACK_CHANNEL = 0x89,
            CMD_FP_ENROLL = 0x9b,
            CMD_FP_IDELETE = 0x9c,
            CMD_SET_TEMPER = 0x9d,
            // database commands
            CMD_DB_GET_RECCOUNT = 0x70,
            CMD_DB_ADD_REC = 0x71,
            CMD_DB_DELETE_REC = 0x72,
            CMD_DB_GET_REC = 0x73,
            CMD_DB_GET_FIRSTREC = 0x74,
            CMD_DB_GET_NEXTREC = 0x75,
            CMD_DB_DELETE_ALL = 0x76,
            CMD_DB_GET_CURRENTREC = 0x77,
            CMD_DB_VERIFY = 0x78,
            CMD_DB_IDENTIFY = 0x79,
            CMD_DB_IDENTIFY_EX = 0x7a,
            CMD_DB_VERIFY_MASTER = 0x7b,
            CMD_DB_VERIFY_MASTER_END = 0x7c,
            CMD_DB_GET_ID_LIST = 0x7d,
            CMD_DB_GET_MASTER_LIST = 0x7e,
            CMD_LOG_GET_RECCOUNT = 0x80,
            CMD_LOG_GET_REC = 0x81,
            CMD_LOG_DELETE_ALL = 0x82;

        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] constroiComandoVIRDI(byte Command, UInt16 wParam, UInt32 Param1, UInt32 Param2, UInt32 exDataLen, UInt16 ErrorCode, byte[] exData)
        {
            int i = 0, j;
            byte[] buf = new byte[20];
            buf[i++] = 0x02;
            buf[i++] = Command;
            buf[i++] = (byte)(wParam & 0x00FF);
            buf[i++] = (byte)(wParam >> 8);
            buf[i++] = (byte)(Param1 & 0x000000FF);
            buf[i++] = (byte)(Param1 >> 8);
            buf[i++] = (byte)(Param1 >> 16);
            buf[i++] = (byte)(Param1 >> 24);
            buf[i++] = (byte)(Param2 & 0x000000FF);
            buf[i++] = (byte)(Param2 >> 8);
            buf[i++] = (byte)(Param2 >> 16);
            buf[i++] = (byte)(Param2 >> 24);
            buf[i++] = (byte)(exDataLen & 0x000000FF);
            buf[i++] = (byte)(exDataLen >> 8);
            buf[i++] = (byte)(exDataLen >> 16);
            buf[i++] = (byte)(exDataLen >> 24);
            buf[i++] = (byte)(ErrorCode & 0x00FF);
            buf[i++] = (byte)(ErrorCode >> 8);
            for (j = 0; j < 18; j++) buf[i] += buf[j]; // checksum
            i++;
            if (exDataLen > 0)
            {
                Array.Resize<byte>(ref buf, (int)(20 + exDataLen));
                for (j = 0; j < exData.Length; j++) buf[i] += exData[j]; // exChecksum
            }
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] VIRDI_CMD_FP_REGISTER_START(UInt16 id, UInt16 autoID, UInt16 master)
        {
            return constroiComandoVIRDI(CMD_FP_REGISTER_START, 0, id, (UInt32)((autoID << 16) + master), 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] VIRDI_CMD_FP_REGISTER_END(UInt16 id )
        {
            return constroiComandoVIRDI(CMD_FP_REGISTER_END, 0, id, 0, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] VIRDI_CMD_FP_IDENTIFY()
        {
            return constroiComandoVIRDI(CMD_FP_IDENTIFY, 0, 0, 0, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] VIRDI_CMD_DB_DELETE_REC( UInt16 id )
        {
            return constroiComandoVIRDI(CMD_DB_DELETE_REC, 0, id, 0, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] VIRDI_CMD_DB_DELETE_ALL()
        {
            return constroiComandoVIRDI(CMD_DB_DELETE_ALL, 0, 0, 0, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] VIRDI_CMD_DB_GET_REC(UInt16 id)
        {
            return constroiComandoVIRDI(CMD_DB_GET_REC, 0, id, 0, 0, 0, null);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] VIRDI_CMD_AUTO_ONOFF(byte AutoSenseOn, byte save)
        {
            return constroiComandoVIRDI(CMD_AUTO_ONOFF, 0, AutoSenseOn, save, 0, 0, null);
        }

        /*
        [DllImport("UCBioBSP.dll")]
        public static extern UInt16 AvzFindDevice(byte[] pDeviceName);
        */
        
    }
}
