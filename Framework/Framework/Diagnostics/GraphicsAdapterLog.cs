#region Using

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public static class GraphicsAdapterLog
    {
        public static readonly Logger Logger = new Logger(typeof(GraphicsAdapter).Name);

        [Conditional("TRACE")]
        public static void Info()
        {
            foreach (var graphicsAdapter in GraphicsAdapter.Adapters)
                Info(graphicsAdapter);
        }

        [Conditional("TRACE")]
        public static void Info(GraphicsAdapter graphicsAdapter)
        {
            Logger.Info("DeviceId:           {0}", graphicsAdapter.DeviceId);
            Logger.Info("DeviceName:         {0}", graphicsAdapter.DeviceName);
            Logger.Info("Description:        {0}", graphicsAdapter.Description);
            Logger.Info("VendorId:           {0}", graphicsAdapter.VendorId);
            Logger.Info("Revision:           {0}", graphicsAdapter.Revision);
            Logger.Info("SubSystemId:        {0}", graphicsAdapter.SubSystemId);
            Logger.Info("IsDefaultAdapter:   {0}", graphicsAdapter.IsDefaultAdapter);
            Logger.Info("IsWideScreen:       {0}", graphicsAdapter.IsWideScreen);
            Logger.Info("CurrentDisplayMode: {0}", graphicsAdapter.CurrentDisplayMode);

            //Logger.Info("SupportedDisplayModes={");
            //foreach (var mode in graphicsAdapter.SupportedDisplayModes) logger.Info("    {0}", mode);
            //Logger.Info("}");
        }
    }
}
