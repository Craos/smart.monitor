using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;

namespace smart.info
{
    public class HardwareInfo
    {
        public string Hostname
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
        public string MacAddress
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
        public PortCom USBSerial
        {
            get
            {
                try
                {
                    ManagementObjectSearcher searcher = VniPnp();
                    string Description = null;

                    foreach (ManagementObject item in searcher.Get())
                    {
                        string Name = item["Name"]?.ToString();
                        Description = item["Description"]?.ToString();

                        if (Description != null && Name.Contains("(COM"))
                        {
                            if (
                                Description.Contains("CH340") ||
                                Description.Contains("Arduino") ||
                                Description.Contains("Prolific")
                                )
                            {
                                PortCom port = new PortCom();
                                port.PortName = Name.Substring(Name.IndexOf("(COM")).Replace("(", "").Replace(")", "");
                                port.Model = item["SystemName"]?.ToString();
                                port.Description = Description;
                                return port;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new PortCom();
                }
                return new PortCom();
            }
        }
       


        /// <summary>
        /// 
        /// </summary>
        public HardwareInfo()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ManagementObjectSearcher VniPnp()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_PnPEntity");
            return new ManagementObjectSearcher(connectionScope, serialQuery);
        }

    }

    public class PortCom
    {
        public string PortName { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
    }

}

