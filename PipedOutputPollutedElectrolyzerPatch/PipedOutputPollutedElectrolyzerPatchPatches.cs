using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Harmony;
using UnityEngine;
using Nightinggale.PipedOutput;
using PollutedElectrolyzer;

namespace PipedOutputPollutedElectrolyzerPatchPatches {
    public class PipedOutputPollutedElectrolyzerPatchPatches {

        public static void AddPollutedElectrolyzer(GameObject go)
        {
            var AddOutput = Traverse.Create(typeof(ApplyExhaust)).Method("AddOutput", new[] { typeof(GameObject), typeof(CellOffset), typeof(SimHashes) });
            AddOutput.GetValue(go, new CellOffset(1, 0), SimHashes.ContaminatedOxygen);
            AddOutput.GetValue(go, new CellOffset(1, 1), SimHashes.Hydrogen);
        }

        [HarmonyPatch(typeof(PollutedElectrolyserConfig), "DoPostConfigurePreview")]
        public static class PollutedElectrolyserConfig_DoPostConfigurePreview_Patch
        {
            public static void Postfix(GameObject go)
            {
                AddPollutedElectrolyzer(go);
            }
        }

        // Token: 0x02000029 RID: 41
        [HarmonyPatch(typeof(PollutedElectrolyserConfig), "DoPostConfigureUnderConstruction")]
        public static class PollutedElectrolyserConfig_DoPostConfigureUnderConstruction_Patch
        {
            public static void Postfix(GameObject go)
            {
                AddPollutedElectrolyzer(go);
            }
        }

        // Token: 0x0200002A RID: 42
        [HarmonyPatch(typeof(PollutedElectrolyserConfig), "ConfigureBuildingTemplate")]
        public static class PollutedElectrolyserConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                AddPollutedElectrolyzer(go);
            }
        }
    }
}
