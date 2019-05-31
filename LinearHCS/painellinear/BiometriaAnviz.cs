using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearHCS
{
    class BiometriaAnviz
    {
        //--------------------------------
        public const int
            MX_OK = 0x00,
            MX_COMM_ERR = 0x01,
            MX_NO_FINGER = 0x02,
            MX_GET_IMG_ERR = 0x03,
            MX_FP_TOO_DRY = 0x04,
            MX_FP_TOO_WET = 0x05,
            MX_FP_DISORDER = 0x06,
            MX_LITTLE_FEATURE = 0x07,
            MX_NOT_MATCH = 0x08,
            MX_NOT_SEARCHED = 0x09,
            MX_MERGE_ERR = 0x0a,
            MX_ADDRESS_OVER = 0x0b,
            MX_READ_ERR = 0x0c,
            MX_UP_TEMP_ERR = 0x0d,
            MX_RECV_ERR = 0x0e,
            MX_UP_IMG_ERR = 0x0f,
            MX_DEL_TEMP_ERR = 0x10,
            MX_CLEAR_TEMP_ERR = 0x11,
            MX_SLEEP_ERR = 0x12,
            MX_INVALID_PASSWORD = 0x13,
            MX_RESET_ERR = 0x14,
            MX_INVALID_IMAGE = 0x15,
            MX_HANGOVER_UNREMOVE = 0X17,
            MX_NO_TEMPLET = 0X19,
            //--------------------------------
            CHAR_BUFFER_A = 0x01,
            CHAR_BUFFER_B           = 0x02,
            MODEL_BUFFER            = 0x03,
            //--------------------------------
            COM1 = 0x01,
            COM2                    = 0x02,
            COM3                    = 0x03,
            //--------------------------------
            BAUD_RATE_38400        = 0x00,
            BAUD_RATE_57600        = 0x01,  //default
            BAUD_RATE_115200       = 0x02;

        public byte[] iniCmd = new byte[] { 0xC0, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 7
        public byte[] detectFinger    = new byte[] { 0x01, 0x01, 0x00, 0x03, 0xC0 };
        public byte[] getImage        = new byte[] { 0x01, 0x02, 0x00, 0x04, 0xC0 };
        public byte[] genTemplateA    = new byte[] { 0x02, 0x03, 0x01, 0x00, 0x07, 0xC0 };
        public byte[] genTemplateB    = new byte[] { 0x02, 0x03, 0x02, 0x00, 0x08, 0xC0 };
        public byte[] lightFlash      = new byte[] { 0x03, 0x16, 0x11, 0x01, 0x00, 0x2C, 0xC0 };
        public byte[] merge           = new byte[] { 0x01, 0x06, 0x00, 0x08, 0xC0 };
        public byte[] upTemplateA     = new byte[] { 0x02, 0x09, 0x01, 0x00, 0x0D, 0xC0 };
        public byte[] upTemplateB     = new byte[] { 0x02, 0x09, 0x02, 0x00, 0x0E, 0xC0 };
        public byte[] upTemplateI     = new byte[] { 0x02, 0x09, 0x03, 0x00, 0x0F, 0xC0 };
        public byte[] searchA         = new byte[] { 0x06, 0x05, 0x01, 0x00, 0x00, 0x06, 0xEF, 0x01, 0x02, 0xC0 };
        public byte[] searchB         = new byte[] { 0x06, 0x05, 0x02, 0x00, 0x00, 0x06, 0xEF, 0x01, 0x03, 0xC0 };
        public byte[] erase           = new byte[] { 0x01, 0x0E, 0x00, 0x10, 0xC0 };
        public byte[] downTemplateI   = new byte[] { 0x02, 0x0A, 0x03, 0x00, 0x10, 0xC0 };
        public byte[] downTemplateA   = new byte[] { 0x02, 0x0A, 0x01, 0x00, 0x0E, 0xC0 };
        public byte[] reset           = new byte[] { 0x01, 0x15, 0x01, 0x00, 0x18, 0xC0 };
        public byte[] dataPacket128   = new byte[] { 0xC0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 };
        public byte[] movMinutiaAtoB = new byte[] { 0x03, 0x20, 0x01, 0x02, 0x27, 0xC0 };
        public byte[] match = new byte[] { 0x01, 0x04, 0x00, 0x06, 0xC0 };
        public byte[] readParTable    = new byte[] {0x01, 0x0F, 0x00, 0x11 , 0xC0 };
        public byte[] store = new byte[] { 0x04, 0x07, 0x03, 0x00, 0x00, 0x00, 0x00, 0xC0 };
        public byte[] read = new byte[] { 0x03, 0x08, 0x00, 0x00, 0x00, 0x00, 0xC0 };
        public byte[] delete = new byte[] { 0x03, 0x0D, 0x00, 0x00, 0x00, 0x00, 0xC0 };
        public byte[] sysConfig = new byte[] { 0x05, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0 };
        public byte[] securityLevel = new byte[] { 0x02, 0x12, 0x01, 0x00, 0x00, 0xC0 };
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdDetectFinger()
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + detectFinger.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < detectFinger.Length; i++) buf[iniCmd.Length + i] = detectFinger[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdGetImage()
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + getImage.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < getImage.Length; i++) buf[iniCmd.Length + i] = getImage[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdGenTemplet(int iBufferID)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + genTemplateA.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < genTemplateA.Length; i++) buf[iniCmd.Length + i] = genTemplateA[i];
            buf[9] = (byte)iBufferID;

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdMoveTemplet(int iScrBufferID, int iDstBufferID)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + movMinutiaAtoB.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < movMinutiaAtoB.Length; i++) buf[iniCmd.Length + i] = movMinutiaAtoB[i];
            buf[9] = (byte)iScrBufferID;
            buf[10] = (byte)iDstBufferID;

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdMatchTwoTemplet()
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + match.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < match.Length; i++) buf[iniCmd.Length + i] = match[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdSearch(int iBufferID, int iStartPage, int iPageNum)
        {
            /*
            public const byte[] searchA         = new byte[] { 0x06, 0x05, 0x01, 0x00, 0x00, 0x06, 0xEF, 0x01, 0x02, 0xC0 };
            public const byte[] searchB         = new byte[] { 0x06, 0x05, 0x02, 0x00, 0x00, 0x06, 0xEF, 0x01, 0x03, 0xC0 };
             */
            int i;
            byte[] buf = new byte[iniCmd.Length + searchA.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < searchA.Length; i++) buf[iniCmd.Length + i] = searchA[i];

            buf[9] = (byte)iBufferID;
            buf[10] = (byte)(iStartPage >> 8);
            buf[11] = (byte)(iStartPage);
            buf[12] = (byte)(iPageNum >> 8);
            buf[13] = (byte)(iPageNum);

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdLevelSearch(int iBufferID, ref int iMbAddress, ref byte[] sUserInfo)
        {
            byte[] buf = new byte[0];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdMergeTwoTemplet()
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + merge.Length];
                        for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
                        for (i = 0; i < merge.Length; i++) buf[iniCmd.Length + i] = merge[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdStoreTemplet(int iBufferID, int iPageID)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + store.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < store.Length; i++) buf[iniCmd.Length + i] = store[i];

            buf[9] = (byte)iBufferID; 
            buf[10] = (byte)(iPageID>>8);
            buf[11] = (byte)(iPageID);

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdLoadTemplet(int iPageID)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + read.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < read.Length; i++) buf[iniCmd.Length + i] = read[i];

            buf[9] = (byte)(iPageID >> 8);
            buf[10] = (byte)(iPageID);

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdUpTemplet(int iBufferID)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + upTemplateA.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < upTemplateA.Length; i++) buf[iniCmd.Length + i] = upTemplateA[i];

            buf[9] = (byte)iBufferID;

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdDownTemplet(int iBufferID, ref byte[] pTemplet, int iTempletLength)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + downTemplateI.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < downTemplateI.Length; i++) buf[iniCmd.Length + i] = downTemplateI[i];

            buf[9] = (byte)iBufferID;

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdUpTempletFile(int iBufferID, ref byte[] pFileName)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + upTemplateA.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < upTemplateA.Length; i++) buf[iniCmd.Length + i] = upTemplateA[i];

            buf[9] = (byte)iBufferID;

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdDownTempletFile(int iBufferID, ref byte[] pFileName)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + downTemplateI.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < downTemplateI.Length; i++) buf[iniCmd.Length + i] = downTemplateI[i];

            buf[9] = (byte)iBufferID;

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdUpImage(ref byte[] pImageData, ref int iImageLength)
        {
            byte[] buf = new byte[0];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdDownImage(ref byte[] pImageData, int iLength)
        {
            byte[] buf = new byte[0];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdDeletOneTemplet(int iPageID)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + delete.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < delete.Length; i++) buf[iniCmd.Length + i] = delete[i];

            buf[9] = (byte)(iPageID >> 8);
            buf[10] = (byte)(iPageID);

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdEraseAllTemplet()
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + erase.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < erase.Length; i++) buf[iniCmd.Length + i] = erase[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdReadParTable(ref byte[] pParTable)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + readParTable.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < readParTable.Length; i++) buf[iniCmd.Length + i] = readParTable[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdSysConfig(ref byte[] pConfig)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + sysConfig.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < sysConfig.Length; i++) buf[iniCmd.Length + i] = sysConfig[i];
            /*
            Characteristics 0 or 1 (1)
            Data Packet Size (0,1 or 2) (1) 
            Baud Rate (0,1 or 2) (1) 
            Rsvd
            Rsvd
            Rsvd
            Rsvd
            Rsvd 
            */
            buf[9]  = 1;
            buf[10] = 1;
            buf[11] = BAUD_RATE_57600;
            buf[12] = 0;
            buf[13] = 0;
            buf[14] = 0;
            buf[15] = 0;
            buf[16] = 0;

            //checksum
            int checksum = 0;
            for (i = 0; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdSetSecurLevel(int iSecurLevel)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + securityLevel.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < securityLevel.Length; i++) buf[iniCmd.Length + i] = securityLevel[i];

            buf[9] = (byte)(iSecurLevel);

            //checksum
            int checksum = 0;
            for (i = 1; i < buf.Length - 3; i++) checksum += buf[i];
            buf[buf.Length - 3] = (byte)(checksum >> 8);
            buf[buf.Length - 2] = (byte)(checksum);
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdReset()
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + detectFinger.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < detectFinger.Length; i++) buf[iniCmd.Length + i] = detectFinger[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public byte[] CmdFlashLED(int iCode, int iTime)
        {
            int i;
            byte[] buf = new byte[iniCmd.Length + detectFinger.Length];
            for (i = 0; i < iniCmd.Length; i++) buf[i] = iniCmd[i];
            for (i = 0; i < detectFinger.Length; i++) buf[iniCmd.Length + i] = detectFinger[i];
            return buf;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public UInt16 ConvertFinger_360_To_338(byte[] bufOrigem, ref byte[] bufDestino, Boolean moduloAnviz)
        {
            if (moduloAnviz == true)
            {
                if ((bufOrigem.Length < 360) || (bufOrigem.Length < 511)) return 0; // Invalid template
            }

            if ((bufOrigem[0] != 0xC0) || (bufOrigem[bufOrigem.Length - 1] != 0xC0)) return 0; // Invalid template

            UInt16 RecLen = 0, i;
            byte Tmp = 0;
            Boolean DBFlag = false;
            byte[] Temp = new byte[512];
            byte[] Buff = new byte[512];

            for (i = 0; i < bufOrigem.Length; i++) Buff[i] = bufOrigem[i];

            for (i = 0; i < bufOrigem.Length - 1; i++)
            {
                Tmp = Buff[i];
                if (Tmp == 0xDB)
                {
                    DBFlag = true;
                }
                else if (Tmp == 0xDC)
                {
                    if (DBFlag == true)
                    {
                        DBFlag = false;
                        Temp[RecLen++] = 0xC0;
                    }
                    else
                    {
                        Temp[RecLen++] = 0xDC;
                    }
                }
                else if (Tmp == 0xDD)
                {
                    if (DBFlag == true)
                    {
                        DBFlag = false;
                        Temp[RecLen++] = 0xDB;
                    }
                    else
                    {
                        Temp[RecLen++] = 0xDD;
                    }
                }
                else
                {
                    Temp[RecLen++] = Tmp;
                }
            }
            if (moduloAnviz == true)
            {
                for (i = 0; i < 169; i++) bufDestino[i] = Temp[i + 8];
                for (i = 0; i < 169; i++) bufDestino[i + 169] = Temp[i + 188];
            }
            else
            {
                for (i = 0; i < RecLen; i++) bufDestino[i] = Temp[i];
            }
            return RecLen;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public UInt16 ConvertFinger_338_To_360(byte[] bufOrigem, ref byte[] bufDestino, Boolean moduloAnviz)
        {
            if (moduloAnviz == true)
            {
                if (bufOrigem.Length != 338) return 0; // Invalid template
            }
            else
            {
                if (bufOrigem.Length != 256) return 0; // Invalid template
            }

            UInt16 RecLen = 0, i,j;
            UInt16 checksumA = 0, checksumB = 0;
            byte[] Temp = new byte[512];
            byte[] Buff = new byte[512];

            for (j = 0; j < (bufOrigem.Length / 2); j++) checksumA += bufOrigem[j];
            checksumA += 0x02;
            for (j = (UInt16)(bufOrigem.Length / 2); j < bufOrigem.Length; j++) checksumB += bufOrigem[j];
            checksumB += 0x08;

            if (moduloAnviz == true)
            {
                checksumA += 0xA9;
                checksumB += 0xA9;
            }
            else
            {
                checksumA += 0x80;
                checksumB += 0x80;
            }

            bufDestino[RecLen++] = 0xC0;
            bufDestino[RecLen++] = 0x02;
            bufDestino[RecLen++] = 0x00;
            bufDestino[RecLen++] = 0x00;
            bufDestino[RecLen++] = 0x00;
            bufDestino[RecLen++] = 0x00;
            bufDestino[RecLen++] = 0x00;

            if (moduloAnviz == true)
            {
                bufDestino[RecLen++] = 0xA9;
            }
            else
            {
                bufDestino[RecLen++] = 0x80;
            }

            for (i = 0; i < bufOrigem.Length; i++)
            {
                if (i == (bufOrigem.Length / 2))
                {
                    bufDestino[RecLen++] = (byte)(checksumA >> 8);
                    bufDestino[RecLen++] = (byte)checksumA;
                    bufDestino[RecLen++] = 0xC0;

                    bufDestino[RecLen++] = 0xC0;
                    bufDestino[RecLen++] = 0x08;
                    bufDestino[RecLen++] = 0x00;
                    bufDestino[RecLen++] = 0x00;
                    bufDestino[RecLen++] = 0x00;
                    bufDestino[RecLen++] = 0x00;
                    bufDestino[RecLen++] = 0x00;
                    if (moduloAnviz == true)
                    {
                        bufDestino[RecLen++] = 0xA9;
                    }
                    else
                    {
                        bufDestino[RecLen++] = 0x80;
                    }
                }

                if (bufOrigem[i] == 0xC0)
                {
                    bufDestino[RecLen++] = 0xDB;
                    bufDestino[RecLen++] = 0xDC;
                }
                else if (bufOrigem[i] == 0xDB)
                {
                    bufDestino[RecLen++] = 0xDB;
                    bufDestino[RecLen++] = 0xDD;
                }
                else
                {
                    bufDestino[RecLen++] = bufOrigem[i];
                }
            }
            bufDestino[RecLen++] = (byte)(checksumB >> 8);
            bufDestino[RecLen++] = (byte)checksumB;
            bufDestino[RecLen++] = 0xC0;
            return RecLen;
        }
    }

}
