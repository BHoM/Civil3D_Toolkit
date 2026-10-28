/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.Adapter.Civil3D;
using BH.Adapter.Socket;
using BH.oM.Adapter;
using BH.oM.Adapters.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;

using Autodesk.AutoCAD.ApplicationServices;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Civil3D.Adapter.Core
{
    //Receives packages from the BHoM client adapter, runs them on the AutoCAD main thread and sends the results back.
    internal static class ListenerHost
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static void Start()
        {
            BH.Engine.Base.Compute.LoadAllAssemblies(BH.Engine.Base.Query.BHoMFolder());

            //Creating the handle binds the control to the current (main) thread, so BeginInvoke runs there
            m_SyncControl = new System.Windows.Forms.Control();
            IntPtr handle = m_SyncControl.Handle;

            m_Adapter = new Civil3DListenerAdapter();

            m_LinkIn = new SocketLink_Tcp(14230);
            m_LinkIn.DataObservers += OnDataReceived;
            m_LinkOut = new SocketLink_Tcp(14231);

            Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\nBHoM Civil 3D listener started on ports 14230/14231.\n");
        }

        /***************************************************/

        internal static void Stop()
        {
            if (m_LinkIn != null)
                m_LinkIn.DataObservers -= OnDataReceived;

            m_SyncControl?.Dispose();
            m_SyncControl = null;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //Runs on the socket thread, so it must not touch the AutoCAD API.
        private static void OnDataReceived(DataPackage package)
        {
            m_SyncControl?.BeginInvoke(new Action(() => Handle(package)));
        }

        /***************************************************/

        //Runs on the AutoCAD main thread.
        private static void Handle(DataPackage package)
        {
            lock (m_PackageLock)
            {
                BH.Engine.Base.Compute.ClearCurrentEvents();

                if (package?.Data == null || package.Data.Count == 0)
                {
                    ReturnData(new List<object> { "Cannot handle an empty package." });
                    return;
                }

                PackageType packageType;
                if (!TryGetPackageType(package.Data[0], out packageType))
                {
                    ReturnData(new List<object> { "Unrecognised package type." });
                    return;
                }

                if (packageType == PackageType.ConnectionCheck)
                {
                    ReturnData(new List<object>());
                    return;
                }

                if (package.Data.Count != 4)
                {
                    ReturnData(new List<object> { "Invalid package size for the given package type." });
                    return;
                }

                try
                {
                    string tag = package.Tag;
                    ActionConfig config = package.Data[2] as ActionConfig;

                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    if (doc == null)
                    {
                        BH.Engine.Base.Compute.RecordError("Connected to Civil 3D, but no drawing is open.");
                        ReturnData(new List<object>());
                        return;
                    }

                    using (doc.LockDocument())
                    {
                        if (packageType == PackageType.Push)
                        {
                            IEnumerable<object> data = package.Data[1] as IEnumerable<object> ?? new List<object>();
                            List<IObject> objects = data.OfType<IObject>().ToList();
                            ReturnData(m_Adapter.Push(objects, tag, PushType.AdapterDefault, config));
                        }
                        else
                        {
                            ReturnData(m_Adapter.Pull(package.Data[1] as IRequest, PullType.AdapterDefault, config).ToList());
                        }
                    }
                }
                catch (Exception e)
                {
                    ReturnData(new List<object> { "Civil 3D failed to complete the action: " + e.Message });
                }
            }
        }

        /***************************************************/

        //The package type can arrive as the enum itself or as its number, depending on serialisation.
        private static bool TryGetPackageType(object data, out PackageType packageType)
        {
            packageType = PackageType.ConnectionCheck;

            if (data is PackageType)
            {
                packageType = (PackageType)data;
                return Enum.IsDefined(typeof(PackageType), packageType);
            }

            if (data is int || data is long)
            {
                long value = System.Convert.ToInt64(data);
                if (value > int.MaxValue || value < int.MinValue || !Enum.IsDefined(typeof(PackageType), (int)value))
                    return false;

                packageType = (PackageType)(int)value;
                return true;
            }

            return false;
        }

        /***************************************************/

        private static void ReturnData(IEnumerable<object> objs)
        {
            DataPackage package = new DataPackage
            {
                Data = objs.ToList(),
                Events = BH.Engine.Base.Query.CurrentEvents(),
                Tag = ""
            };

            m_LinkOut.SendData(package);
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static SocketLink_Tcp m_LinkIn;
        private static SocketLink_Tcp m_LinkOut;
        private static Civil3DListenerAdapter m_Adapter;
        private static System.Windows.Forms.Control m_SyncControl;
        private static readonly object m_PackageLock = new object();

        /***************************************************/
    }
}
