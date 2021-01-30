using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using CaiLib.Utils;
using static CaiLib.Utils.BuildingUtils;
using static CaiLib.Utils.StringUtils;
using CoolLib;
using static CoolLib.Log;

namespace BuildableNaturalTile {
    public static class BuildableNaturalTilePatches {
        public static Config Settings = Config.Load(); // Load Settings from Config <CoolLib>

        public static void OnLoad() => LogInit("BNT"); // Initialize mod logs <CoolLib>

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch {
            public static void Prefix() {
                AddBuildingStrings(NaturalTileConfig.ID, NaturalTileConfig.DisplayName,
                    NaturalTileConfig.Description, NaturalTileConfig.Effect);
                int index = TUNING.BUILDINGS.PLANORDER.FindIndex(x => x.category == "Base");
                if (index == -1)
                    return;

                // add tile to `Base` building tab
                IList<string> planOrderList = TUNING.BUILDINGS.PLANORDER[index].data;
                planOrderList.Add(NaturalTileConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Database.Techs), "Init")]
        public static class Database_Techs_Init_Patch {
            public static void Postfix(ref Database.Techs __instance) {
                // Add to `Basic Farming` technology
                Tech techBasicFarming = __instance.TryGetTechForTechItem("RationBox");
                techBasicFarming.unlockedItemIDs.Add(NaturalTileConfig.ID);
            }
        }

        [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
        public static class BuildingComplete_OnSpawn_Patch {
            private static readonly CellOffset[] _displacementOffsets = new CellOffset[]
            {
                new CellOffset(0, 1),
                new CellOffset(0, -1),
                new CellOffset(1, 0),
                new CellOffset(-1, 0),
                new CellOffset(1, 1),
                new CellOffset(1, -1),
                new CellOffset(-1, 1),
                new CellOffset(-1, -1)
            };

            public static void Postfix(BuildingComplete __instance) {
                GameObject go = __instance.gameObject;
                if (__instance.name == "NaturalTileComplete") {
                    LogDebug($"__instance.name: {__instance.name}");

                    Vector3 pos = go.transform.position;
                    PrimaryElement element = go.GetComponent<PrimaryElement>();
                    float temperature = element.Temperature;
                    float mass = Settings.BlockMass;
                    int cell = Grid.PosToCell(pos);
                    LogDebug($"{element}");

                    LogDebug($"pos: {pos}, temperature: {temperature}, cell: {cell}");

                    // SimMessages.ReplaceAndDisplaceElement(
                    //     int gameCell, SimHashes new_element, CellElementEvent ev,
                    //     float mass, float temperature = -1f, byte disease_idx = 255,
                    //     int disease_count = 0, int callbackIdx = -1
                    // )
                    SimMessages.ReplaceAndDisplaceElement(cell, element.ElementID,
                        null, mass, temperature, byte.MaxValue, 0, -1); // spawn Natural Block

                    // Error randomly showing up after creation of natural block
                    // NullReferenceException: Object reference not set to an instance of an object
                    // at ObjectLayerListItem.Refresh(System.Int32 new_cell)[0x00149]
                    //      in C:\jenkins_workspace\workspace\SimGame_Windows\game\Assets\
                    //      scripts\game\ObjectLayerListItem.cs:59
                    // at ObjectLayerListItem.Update(System.Int32 cell)[0x00001]
                    //      in C:\jenkins_workspace\workspace\SimGame_Windows\game\Assets\
                    //      scripts\game\ObjectLayerListItem.cs:72
                    // at Pickupable.OnCellChange()[0x000d1] in C:\jenkins_workspace\workspace\
                    //      SimGame_Windows\game\Assets\scripts\components\Pickupable.cs:479
                    // at CellChangeMonitor.RenderEveryTick()[0x00143] in C:\jenkins_workspace\
                    //      workspace\SimGame_Windows\game\Assets\Plugins\Klei\util\
                    //      CellChangeMonitor.cs:194
                    // at Game.Update()[0x00091] in C:\jenkins_workspace\workspace\SimGame_Windows\
                    //      game\Assets\scripts\game\Game.cs:1341

                    // NOTE: Displace pickupables needs to be watched for possibly slowing down the game during late game
                    int origCell = cell;
                    foreach (Pickupable pickupable in Components.Pickupables) {
                        if (Grid.PosToCell(pickupable) == origCell) {
                            LogDebug($"pickupable: {pickupable}");

                            for (int i = 0; i < _displacementOffsets.Length; i++) {
                                int offsetCell = Grid.OffsetCell(cell, _displacementOffsets[i]);
                                if (Grid.IsValidCell(offsetCell) && !Grid.Solid[offsetCell]) {
                                    Vector3 position = Grid.CellToPosCBC(offsetCell, Grid.SceneLayer.Move);
                                    KCollider2D component = pickupable.GetComponent<KCollider2D>();
                                    if (component != null) {
                                        position.y += pickupable.transform.GetPosition().y - component.bounds.min.y;
                                    }
                                    pickupable.transform.SetPosition(position);
                                    cell = offsetCell;

                                    Traverse.Create(pickupable).Method("RemoveFaller", new Type[] { }).GetValue();
                                    Traverse.Create(pickupable).Method("AddFaller", new Type[] { typeof(Vector2) }).GetValue(Vector2.zero);

                                    break;
                                }
                            }
                        }
                    }

                    go.DeleteObject(); // remove Natural Tile
                }
            }
        }
    }
}
