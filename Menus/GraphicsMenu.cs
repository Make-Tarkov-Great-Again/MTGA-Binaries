using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.A.Tarkov.Core.Menus
{
    public class GraphicsMenu : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return null;
        }
        public GraphicsMenu()
        {
            Logger.LogInfo("in Graphics Menu");
            var TypeOfGraphicsSettingsTab = typeof(EFT.UI.Settings.GraphicsSettingsTab);

            if (TypeOfGraphicsSettingsTab != null) Logger.LogInfo("EFT.UI.Settings.GraphicsSettingsTab");

            var readOnlyCollection_0 = TypeOfGraphicsSettingsTab.GetField(
                "readOnlyCollection_0",
                BindingFlags.Static |
                BindingFlags.NonPublic
                );

            if (readOnlyCollection_0 != null) Logger.LogInfo("readOnlyCollection_0");
            var readOnlyCollection_1 = TypeOfGraphicsSettingsTab.GetField(
                "readOnlyCollection_1",
                BindingFlags.Static |
                BindingFlags.NonPublic
                );
            if (readOnlyCollection_1 != null) Logger.LogInfo("readOnlyCollection_1");

            var readOnlyCollection_2 = TypeOfGraphicsSettingsTab.GetField(
                "readOnlyCollection_2",
                BindingFlags.Static |
                BindingFlags.NonPublic
                );
            if (readOnlyCollection_2 != null) Logger.LogInfo("readOnlyCollection_2");

            List<float> overallVisibility = new();
            for (int i = 0; i <= 11; i++)
            {
                overallVisibility.Add(400 + (i + 50));
            }

            for (int i = 0; i <= 4; i++)
            {
                overallVisibility.Add(1000 + (i + 500));
            }


            List<float> lodQuality = new();
            for (int i = 0; i <= 9; i++)
            {
                overallVisibility.Add(2 + (i + Convert.ToSingle(0.25)));
            }


            var Collection_0 = Array.AsReadOnly<float>(overallVisibility.ToArray());
            //var Collection_1 = Array.AsReadOnly<float>(new float[] { 1f, 2f, 3f }); //this will be for head bobbing
            var Collection_2 = Array.AsReadOnly<float>(lodQuality.ToArray());

            readOnlyCollection_0.SetValue(null, Collection_0);
            //readOnlyCollection_1.SetValue(null, Collection_1);
            readOnlyCollection_2.SetValue(null, Collection_2);
            Logger.LogInfo("leaving patch");
        }
    }
}
