using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.A.Tarkov.Core.Menus
{
    internal class GraphicsMenu : ModulePatch
    {
        public GraphicsMenu() {
        var TypeOfGraphicsSettingsTab = typeof(EFT.UI.Settings.GraphicsSettingsTab);
        var readOnlyCollection_0 = TypeOfGraphicsSettingsTab.GetField(
            "readOnlyCollection_0", 
            BindingFlags.Static |
            BindingFlags.NonPublic
            );

        var readOnlyCollection_1 = TypeOfGraphicsSettingsTab.GetField(
            "readOnlyCollection_1", 
            BindingFlags.Static |
            BindingFlags.NonPublic
            );

        var readOnlyCollection_2 = TypeOfGraphicsSettingsTab.GetField(
            "readOnlyCollection_2", 
            BindingFlags.Static | 
            BindingFlags.NonPublic
            );

        List<float> overallVisibility = new();
        for (int i = 0; i <= 12; i++)
        {
          overallVisibility.Add(400 + (i* 50));
        }


        var Collection_0 = Array.AsReadOnly<float>(overallVisibility.ToArray());
        var Collection_1 = Array.AsReadOnly<float>(new float[] { 1f, 2f, 3f });
        var Collection_2 = Array.AsReadOnly<float>(new float[] { 1f, 2f, 3f });

        readOnlyCollection_0.SetValue(null, Collection_0);
        readOnlyCollection_1.SetValue(null, Collection_1);
        readOnlyCollection_2.SetValue(null, Collection_2);
        }

        protected override MethodBase GetTargetMethod()
        {
            return null;
        }
    }
}
