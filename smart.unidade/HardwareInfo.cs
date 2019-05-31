using System;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;

namespace smart.info
{
    public enum HardwareModel
    {
        NaoIdentificado,
        CH340,
        Arduino,
        Prolific,
        FTDIBUS
    }
    public enum HardWareFab
    {
        NaoIdentificado,
        Craos,
        LinearHCS
    }
    public static class HardwareInfo
    {
        public static string Hostname
        {
            get
            {
                ManagementObjectSearcher searcher = VniPnp();
                string name = null;
                foreach (ManagementObject item in searcher.Get())
                {
                    name = item["SystemName"]?.ToString();
                    break;
                }
                return name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string MacAddress
        {
            get
            {
                string address = (
                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();

                string MACwithColons = "";
                for (int i = 0; i < address.Length; i++)
                {
                    MACwithColons = MACwithColons + address.Substring(i, 2) + ":";
                    i++;
                }
                return MACwithColons.Substring(0, MACwithColons.Length - 1);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool USBSerial
        {
            get
            {
                ManagementObjectSearcher searcher = VniPnp();
                try
                {
                    string Description = null;

                    foreach (ManagementObject item in searcher.Get())
                    {
                        string fullname = item.ToString();
                        string Name = item["Name"]?.ToString();
                        Description = item["Description"]?.ToString();

                        if (Description != null && Name.Contains("(COM"))
                        {
                            bool confirmado = false;
                            HardwareModel modelo = HardwareModel.NaoIdentificado;
                            HardWareFab fabricante = HardWareFab.NaoIdentificado;

                            if (Description.Contains("CH340"))
                            {
                                modelo = HardwareModel.CH340;
                                fabricante = HardWareFab.Craos;
                                confirmado = true;
                            }
                            else if (Description.Contains("Arduino"))
                            {
                                modelo = HardwareModel.Arduino;
                                fabricante = HardWareFab.Craos;
                                confirmado = true;
                            }
                            else if (Description.Contains("Prolific"))
                            {
                                modelo = HardwareModel.Prolific;
                                fabricante = HardWareFab.LinearHCS;
                                confirmado = true;
                            }
                            else if (fullname.IndexOf("FTDIBUS") > -1)
                            {
                                modelo = HardwareModel.FTDIBUS;
                                fabricante = HardWareFab.Craos;
                                confirmado = true;
                            }

                            if (confirmado)
                            {

                                string[] hardwareIds = (string[])item["HardWareID"];

                                PortCom.PortName = Name.Substring(Name.IndexOf("(COM")).Replace("(", "").Replace(")", "");
                                PortCom.Model = item["SystemName"]?.ToString();
                                PortCom.Description = Description;
                                PortCom.Fabricante = fabricante;
                                PortCom.Modelo = modelo;

                                if ((hardwareIds != null) && (hardwareIds.Length > 0))
                                    PortCom.HardwareID = hardwareIds[0];

                                return true;

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
                finally
                {
                    searcher.Dispose();
                    searcher = null;
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static ManagementObjectSearcher VniPnp()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_PnPEntity");
            return new ManagementObjectSearcher(connectionScope, serialQuery);
        }

    }

    public static class PortCom
    {
        public static string PortName { get; set; }
        public static string Model { get; set; }
        public static string Description { get; set; }
        public static HardWareFab Fabricante { get; set; }
        public static HardwareModel Modelo { get; set; }
        public static string HardwareID { get; set; }
    }

}

