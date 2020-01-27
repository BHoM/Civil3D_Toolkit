﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;

using BH.Adapter;
using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Socket;
using BH.oM.Adapters.Civil3D;
using BH.UI.Civil.Adapter;

using BH.oM.Adapter;

namespace BH.UI.Civil
{
    //Class handling the communication between outer application and Civil3d.
    //Responsible for dispatching incoming data to the corret adapter method
    public class CivilListener : IExtensionApplication
    {

        /***************************************************/
        /**** Public Application methods                ****/
        /***************************************************/

        public void Initialize()
        {
            //Get the folder path of the plugin to load up all dlls
            //string folder = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), @"Autodesk\ApplicationPlugins\GroundSnake.bundle\Contents");
            string folder = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), @"BHoM\Assemblies");
            BH.Engine.Reflection.Compute.LoadAllAssemblies(folder);

            //Create the internal adapter to be used to cumminicate with Civil3d
            m_adapter = new CivilUIAdapter();

            //Socket link for listening and sending data
            m_linkIn = new SocketLink_Tcp(14230);
            m_linkIn.DataObservers += M_linkIn_DataObservers;

            m_linkOut = new SocketLink_Tcp(14231);

            m_adapter = new CivilUIAdapter();
        }

        /***************************************************/

        public void Terminate()
        {
            m_linkIn.DataObservers -= M_linkIn_DataObservers;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //Callback method being triggered when the sockets recieves new data
        private void M_linkIn_DataObservers(DataPackage package)
        {
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

            lock (m_packageLock)
            {
                //General checks of the recived data
                if (package.Data.Count < 1)
                {
                    ReturnData(new List<string> { "Cant handle empty package" });
                    return;
                }

                PackageType packageType = PackageType.ConnectionCheck;

                try
                {
                    packageType = (PackageType)package.Data[0];
                }
                catch
                {
                    ReturnData(new List<string> { "Unrecognized package type" });
                    return;
                }

                if (packageType != PackageType.ConnectionCheck)
                {
                    if (!CheckPackageSize(package))
                        return;
                }
                else
                {
                    ReturnData(new List<object>());
                }


                string tag = package.Tag;
                ActionConfig config = package.Data[2] as ActionConfig;
                Civil3DConfig settings = package.Data[3] as Civil3DConfig;

                try
                {

                    //Handle the data recieved depending on the package
                    switch (packageType)
                    {
                        case PackageType.ConnectionCheck:
                            ReturnData(new List<object>());
                            return;
                        case PackageType.Push:
                            if (!CheckPackageSize(package)) return;
                            List<IObject> pushData = new List<IObject>();    //Clear the previous package list
                                                                             //Add all objects to the list
                            foreach (object obj in package.Data[1] as IEnumerable<object>)
                            {
                                if (obj is IObject)
                                    pushData.Add(obj as IObject);
                            }

                            ReturnData(m_adapter.Push(pushData, tag, PushType.AdapterDefault, config));
                            break;
                        case PackageType.Pull:
                            if (!CheckPackageSize(package)) return;
                            IRequest request = package.Data[1] as IRequest;
                            ReturnData(m_adapter.Pull(request, PullType.AdapterDefault, config));
                            break;
                        default:
                            ReturnData(new List<string> { "Unrecognized package type" });
                            return;
                    }
                }
                catch (System.Exception e)              
                {
                    BH.Engine.Reflection.Compute.RecordError("OPeration failed. Message from adapter: " + e.Message);
                    ReturnData(new List<string> { "Operation failed." });
                }


            }
        }

        /***************************************************/

        private bool CheckPackageSize(oM.Socket.DataPackage package)
        {
            if (package.Data.Count < 4)
            {
                ReturnData(new List<string> { "Invalid Package" });
                return false;
            }
            return true;
        }

        /***************************************************/

        public void ReturnData(IEnumerable<object> objs)
        {

            oM.Socket.DataPackage package = new oM.Socket.DataPackage
            {
                Data = objs.ToList(),
                //Events = BH.Engine.Reflection.Query.CurrentEvents(),
                Tag = ""
            };

            m_linkOut.SendData(package);
        }

        /***************************************************/


        public void SetPorts(int inputPort, int outputPort)
        {
            //Not sure if needed
            m_linkIn.DataObservers -= M_linkIn_DataObservers;

            m_linkIn = new SocketLink_Tcp(inputPort);
            m_linkIn.DataObservers += M_linkIn_DataObservers;

            m_linkOut = new SocketLink_Tcp(outputPort);
        }


        /***************************************************/
        /**** Private feilds                            ****/
        /***************************************************/

        private SocketLink_Tcp m_linkIn;
        private SocketLink_Tcp m_linkOut;

        private CivilUIAdapter m_adapter;

        public object m_packageLock = new object();

        /***************************************************/
    }
}
