using System;
using Harmony;
using UnityEngine;
using CaiLib.Utils;
using static CaiLib.Utils.BuildingUtils;
using static CaiLib.Utils.StringUtils;
using static CoolLib.Log;

namespace BuildableNaturalTile {
    // TODO: refactor all LogDebug "BuildableNaturalTile", to CoolLib.Log
    public static class BuildableNaturalTilePatches {
        public static void OnLoad() => LogInit("BuildableNaturalTile", true);

        public static Config Settings = Config.Load();

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
        public static class BuildingComplete_OnSpawn_Patch
        {
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

            public static void Postfix(BuildingComplete __instance)
            {
                GameObject go = __instance.gameObject;
                if(__instance.name == "NaturalTileComplete"){
                    LogDebug("BuildableNaturalTile", "<BuildingComplete_OnSpawn> __instance.name: " + __instance.name);

                    Vector3 pos = go.transform.position;
                    PrimaryElement element = go.GetComponent<PrimaryElement>();
                    float temperature = element.Temperature;
                    float mass = Settings.BlockMass;
                    int cell = Grid.PosToCell(pos);
                    SimMessages.ReplaceAndDisplaceElement(cell, element.ElementID, null, mass, temperature, byte.MaxValue, 0, -1); // spawn Dirt Block

                    LogDebug("BuildableNaturalTile", $"<BuildingComplete_OnSpawn> pos: {pos}, temperature: {temperature}, cell: {cell}");

                    // NOTE: Displace pickupables needs to be watched for possibly slowing down the game during late game
                    int origCell = cell;
                    foreach (Pickupable pickupable in Components.Pickupables){
                        if (Grid.PosToCell(pickupable) == origCell){
                            LogDebug("BuildableNaturalTile", $"<BuildingComplete_OnSpawn> pickupable: {pickupable}");

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
