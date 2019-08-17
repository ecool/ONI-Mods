using System;
using Harmony;
using UnityEngine;
using CaiLib.Utils;
using static CaiLib.Utils.BuildingUtils;
using static CaiLib.Utils.StringUtils;

namespace BuildableDirtTile {
    public class BuildableDirtTilePatches {

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                AddBuildingStrings(DirtTileConfig.ID, DirtTileConfig.DisplayName, DirtTileConfig.Description, DirtTileConfig.Effect);
                AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, DirtTileConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                AddBuildingToTechnology(GameStrings.Technology.Food.BasicFarming, DirtTileConfig.ID);
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
                if(__instance.name == "DirtTileComplete"){

                    Debug.Log("[DIRT] <BuildingComplete_OnSpawn> __instance.name: " + __instance.name);
                    Vector3 pos = go.transform.position;
                    float temperature = go.GetComponent<PrimaryElement>().Temperature;

                    //Element element = ElementLoader.FindElementByHash(SimHashes.Dirt);
                    float mass = 50f; // 50kg
                    byte disease_idx = byte.MaxValue; // not sure if MaxValue is what I want.
                    //element.substance.SpawnResource(pos, mass, temperature, disease_idx, 0, false, false);
                    int cell = Grid.PosToCell(pos);
                    //SimMessages.ReplaceElement(cell, SimHashes.Dirt, null, mass, temperature, disease_idx, 0, -1);
                    SimMessages.ReplaceAndDisplaceElement(cell, SimHashes.Dirt, null, mass, temperature, disease_idx, 0, -1);
                    //go.DeleteObject(); // testing if this would remove the tile.

                    // GetComponent<KPrefabID>().RemoveTag(GameTags.Entombed); from Pickupables.IsEntombed
                    // NOTE: test Pickupables.IsEntombed = false;
                    // NOTE: might be better - Pickupables.TryToOffsetIfBuried()

                    Debug.Log($"[DIRT] <BuildingComplete_OnSpawn> pos: {pos}, temperature: {temperature}, cell: {cell}");
                    //Debug.Log(Grid.Objects[cell, (int) ObjectLayer.Pickupables]);
                    /* GameObject obj = Grid.Objects[cell, (int) ObjectLayer.Pickupables];
                    while (obj != null) {
                        Pickupable pickupable = obj.AddOrGet<Pickupable>();
                        Debug.Log($"[DIRT] <BuildingComplete_OnSpawn> obj: {obj}, pickupable: {pickupable}");
                        pickupable.TryToOffsetIfBuried();
                        obj = null;
                    } */

                    /* Pickupable.TryToOffsetIfBuried() // NOTE: might need to right a version of this method and attempt it.
                    for (int i = 0; i < Pickupable.displacementOffsets.Length; i++)
                    {
                        int num2 = Grid.OffsetCell(num, Pickupable.displacementOffsets[i]);
                        if (Grid.IsValidCell(num2) && !Grid.Solid[num2])
                        {
                            Vector3 position = Grid.CellToPosCBC(num2, Grid.SceneLayer.Move);
                            KCollider2D component = base.GetComponent<KCollider2D>();
                            if (component != null)
                            {
                                position.y += base.transform.GetPosition().y - component.bounds.min.y;
                            }
                            base.transform.SetPosition(position);
                            num = num2;
                            this.RemoveFaller();
                            this.AddFaller(Vector2.zero);
                            break;
                        }
                    } */

                    //private static CellOffset[] displacementOffsets = new CellOffset[]

                    int origCell = cell;

                    foreach (Pickupable pickupable in Components.Pickupables){
                        if (Grid.PosToCell(pickupable) == origCell){
                            Debug.Log($"[DIRT] <BuildingComplete_OnSpawn> pickupable: {pickupable}");

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

                    /* GameObject obj = Grid.Objects[origCell, (int) ObjectLayer.Pickupables];
                    while (obj != null) {
                        Pickupable pickupable = obj.AddOrGet<Pickupable>();
                        Debug.Log($"[DIRT] <BuildingComplete_OnSpawn> obj: {obj}, pickupable: {pickupable}");

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

                        obj = Grid.Objects[origCell, (int) ObjectLayer.Pickupables];
                        Debug.Log(obj);
                    } */

                    go.DeleteObject();
                }
            }
        }

        /* [HarmonyPatch(typeof(Pickupable), "OnSolidChanged")]
        public class Pickupable_OnSolidChanged_Patch
        {
            private static List<GameObject> pickupables = new List<GameObject>();

            // NOTE: testing what this shows during my BuildingComplete_OnSpawn_Patch
            public static bool Prefix(Pickupable __instance, object data)
            {
                int cell = __instance.GetCell();
                GameObject go = Grid.Objects[cell, (int) ObjectLayer.Building];

                var solid = Grid.Solid[cell];
                var foundation = Grid.Foundation[cell];
                var properties = Grid.Properties[cell];
                Debug.Log($"[DIRT] <Pickupable_OnSolidChanged> solid: {solid}, foundation: {foundation}, properties: {properties}");

                if (go == null) return true;

                if (go.name == "DirtTileComplete"){
                    Debug.Log($"[DIRT] <Pickupable_OnSolidChanged> __instance: {__instance}, data: {data}, cell: {cell}");

                    __instance.TryToOffsetIfBuried();
                    GameObject pickupable = Grid.Objects[cell, (int) ObjectLayer.Pickupables];
                    if (pickupable == null) {
                        //go.DeleteObject();
                    }
                    return false;
                }

                return true;
            }
        } */

        /*
        public void ForceEmit(float mass, byte disease_idx, int disease_count, float temperature = -1f)
        {
            if (mass <= 0f)
            {
                return;
            }
            float temperature2 = (temperature <= 0f) ? this.outputElement.minOutputTemperature : temperature;
            Element element = ElementLoader.FindElementByHash(this.outputElement.elementHash);
            if (element.IsGas || element.IsLiquid)
            {
                int gameCell = Grid.PosToCell(base.transform.GetPosition());
            ->  SimMessages.AddRemoveSubstance(gameCell, this.outputElement.elementHash, CellEventLogger.Instance.ElementConsumerSimUpdate, mass, temperature2, disease_idx, disease_count, true, -1);
            }
            else if (element.IsSolid)
            {
                element.substance.SpawnResource(base.transform.GetPosition() + new Vector3(0f, 0.5f, 0f), mass, temperature2, disease_idx, disease_count, false, true, false);
            }
            PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Resource, ElementLoader.FindElementByHash(this.outputElement.elementHash).name, base.gameObject.transform, 1.5f, false);
        }
        */


        /* NestingPoopState
        string name = CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME;
		string tooltip = CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(name, tooltip, string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, false, default(HashedString), 129022, null, null, main).PlayAnim("poop").OnAnimQueueComplete(this.behaviourcomplete);
        */


        /* Substance
        public GameObject SpawnResource(Vector3 position, float mass, float temperature, byte disease_idx, int disease_count, bool prevent_merge = false, bool forceTemperature = false, bool manual_activation = false)
        */

        // CreatureCalrorieMonitor.Stomach.Poop()
        /*
        element.substance.SpawnResource(Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore), mass, temperature, disease_idx, disease_count, false, false, false);
        */
    }
}
