using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;
using STRINGS;

public class NaturalTileConfig : IBuildingConfig {
    public const string ID = "NaturalTile";
    public const string DisplayName = "Natural Tile";
    public const string Description = "Fill that hole you dug out back in with any solid element.";
    public static string Effect = $"Fills a block in the world with {UI.FormatAsLink("Solids", "ELEMENTS_SOLID")}.";

    public static readonly int BlockTileConnectorID = Hash.SDBMLower("natural_tile_block");

    public override BuildingDef CreateBuildingDef() {
        string id = "NaturalTile";
        int width = 1;
        int height = 1;
        string anim = "natural_tile_kanim";
        int hitpoints = 100;
        float construction_time = BuildableNaturalTile.BuildableNaturalTilePatches.Settings.BuildSpeed;
        float[] tier = new float[] { BuildableNaturalTile.BuildableNaturalTilePatches.Settings.BuildMass }; // 50kg
        string[] raw_MINERALS = new string[] { "Solid" }; // currently set to all solids
        float melting_point = 1600f;
        BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
        EffectorValues none = NOISE_POLLUTION.NONE;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(
            id, width, height, anim, hitpoints, construction_time, tier,
            raw_MINERALS, melting_point, build_location_rule,
            TUNING.BUILDINGS.DECOR.BONUS.TIER0, none, 0.2f);

        // NOTE: not sure if any of this stuff is still needed, but errors no
        // longer happen without the code below

        // BuildingTemplates.CreateFoundationTileDef(buildingDef);
        // buildingDef.Floodable = false;
        // buildingDef.Entombable = false;
        // buildingDef.Overheatable = false;
        // buildingDef.ForegroundLayer = Grid.SceneLayer.BuildingBack;
        // buildingDef.AudioCategory = "HollowMetal";
        // buildingDef.AudioSize = "small";
        // buildingDef.BaseTimeUntilRepair = -1f;
        // buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
        // buildingDef.ConstructionOffsetFilter = BuildingDef.ConstructionOffsetFilter_OneDown;
        // buildingDef.isSolidTile = false;
        // buildingDef.DragBuild = true;
        // buildingDef.ObjectLayer = ObjectLayer.Building;
        return buildingDef;
    }

    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag) {
        GeneratedBuildings.MakeBuildingAlwaysOperational(go);
        BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
    }

    public override void DoPostConfigureComplete(GameObject go) {
        GeneratedBuildings.RemoveLoopingSounds(go);
        go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles, false);
    }

    public override void DoPostConfigureUnderConstruction(GameObject go) {
        base.DoPostConfigureUnderConstruction(go);
    }
}
