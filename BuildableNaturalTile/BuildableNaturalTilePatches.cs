using System;
using Harmony;
using UnityEngine;
using CaiLib.Utils;
using static CaiLib.Utils.BuildingUtils;
using static CaiLib.Utils.StringUtils;

namespace BuildableNaturalTile {
    public class BuildableNaturalTilePatches {

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                AddBuildingStrings(NaturalTileConfig.ID, NaturalTileConfig.DisplayName, NaturalTileConfig.Description, NaturalTileConfig.Effect);
                AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, NaturalTileConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                AddBuildingToTechnology(GameStrings.Technology.Food.BasicFarming, NaturalTileConfig.ID);
            }
        }

        [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
        public class BuildingComplete_OnSpawn_Patch
        {
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

            public static void Postfix(BuildingComplete __instance)
            {
                GameObject go = __instance.gameObject;
                if(__instance.name == "NaturalTileComplete"){

                    Debug.Log("[NATURAL] <BuildingComplete_OnSpawn> __instance.name: " + __instance.name);

                    Vector3 pos = go.transform.position;
                    PrimaryElement element = go.GetComponent<PrimaryElement>();
                    float temperature = element.Temperature;
                    float mass = 50f; // 50kg
                    byte disease_idx = byte.MaxValue;
                    int cell = Grid.PosToCell(pos);
                    SimMessages.ReplaceAndDisplaceElement(cell, element.ElementID, null, mass, temperature, disease_idx, 0, -1); // spawn Dirt Block

                    Debug.Log($"[NATURAL] <BuildingComplete_OnSpawn> pos: {pos}, temperature: {temperature}, cell: {cell}");

                    // NOTE: Displace pickupables needs to be watched for possibly slowing down the game during late game
                    int origCell = cell;
                    foreach (Pickupable pickupable in Components.Pickupables){
                        if (Grid.PosToCell(pickupable) == origCell){
                            Debug.Log($"[NATURAL] <BuildingComplete_OnSpawn> pickupable: {pickupable}");

                            for (int i=0; i<_displacementOffsets.Length; i++){
                                int offsetCell = Grid.OffsetCell(cell, _displacementOffsets[i]);
                                if (Grid.IsValidCell(offsetCell) && !Grid.Solid[offsetCell]){
                                    Vector3 position = Grid.CellToPosCBC(offsetCell, Grid.SceneLayer.Move);
                                    KCollider2D component = pickupable.GetComponent<KCollider2D>();
                                    if (component != null)
                                    {
                                        position.y += pickupable.transform.GetPosition().y - component.bounds.min.y;
                                    }
                                    pickupable.transform.SetPosition(position);
                                    cell = offsetCell;

                                    Traverse.Create(pickupable).Method("RemoveFaller", new Type[] {}).GetValue();
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
