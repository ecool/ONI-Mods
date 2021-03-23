using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using CaiLib.Utils;
using static CaiLib.Utils.BuildingUtils;
using static CaiLib.Utils.StringUtils;
using static CoolLib.Log;

namespace BuildableDirtTile {
    public class BuildableDirtTilePatches {
        public static Config Settings = Config.Load();

        public static void OnLoad() => LogInit("BDT");

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch {
            public static void Prefix() {
                AddBuildingStrings(DirtTileConfig.ID, DirtTileConfig.DisplayName, DirtTileConfig.Description, DirtTileConfig.Effect);
                int index = TUNING.BUILDINGS.PLANORDER.FindIndex(x => x.category == "Base");
                if (index == -1)
                    return;

                // add tile to `Base` building tab
                IList<string> planOrderList = TUNING.BUILDINGS.PLANORDER[index].data;
                planOrderList.Add(DirtTileConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Database.Techs), "Init")]
        public static class Database_Techs_Init_Patch {
            public static void Postfix(ref Database.Techs __instance) {
                // Add to `Basic Farming` technology
                Tech techBasicFarming = __instance.TryGetTechForTechItem("RationBox");
                techBasicFarming.unlockedItemIDs.Add(DirtTileConfig.ID);
            }
        }

        [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
        public class BuildingComplete_OnSpawn_Patch {
            private static CellOffset[] _displacementOffsets = new CellOffset[]
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
                if (__instance.name == "DirtTileComplete") {

                    Debug.Log("[DIRT] <BuildingComplete_OnSpawn> __instance.name: " + __instance.name);

                    Vector3 pos = go.transform.position;
                    float temperature = go.GetComponent<PrimaryElement>().Temperature;
                    float mass = Settings.BlockMass; // 50kg
                    byte disease_idx = byte.MaxValue;
                    int cell = Grid.PosToCell(pos);
                    SimMessages.ReplaceAndDisplaceElement(cell, SimHashes.Dirt, null, mass, temperature, disease_idx, 0, -1); // spawn Dirt Block

                    Debug.Log($"[DIRT] <BuildingComplete_OnSpawn> pos: {pos}, temperature: {temperature}, cell: {cell}");

                    // NOTE: Displace pickupables needs to be watched for possibly slowing down the game during late game
                    int origCell = cell;
                    foreach (Pickupable pickupable in Components.Pickupables) {
                        if (Grid.PosToCell(pickupable) == origCell) {
                            Debug.Log($"[DIRT] <BuildingComplete_OnSpawn> pickupable: {pickupable}");

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

                    go.DeleteObject(); // remove Dirt Tile
                }
            }
        }
    }
}
