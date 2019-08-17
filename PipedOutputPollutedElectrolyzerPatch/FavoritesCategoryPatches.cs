using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using UnityEngine;
using Harmony;

namespace FavoritesCategory {
	public class FavoritesCategoryPatches{
		public static ResourceCategoryHeader favoritesCategoryHeader = null;

		[HarmonyPatch(typeof(ResourceEntry), "OnClick")]
		public static class ResourceEntry_OnClick_Patch
		{
			public static bool Prefix(ref ResourceEntry __instance)
			{
				var cont = true;

				// check active modifiers
				var check = (Modifier) Traverse.Create(Global.Instance.GetInputManager().GetDefaultController()).Field("mActiveModifiers").GetValue();

				if(check == Modifier.Shift){
					cont = false; // don't execute original OnClick

					Tag tag = (Tag) __instance.Resource;

					if (favoritesCategoryHeader != null) {
						ResourceEntry resourceEntry = null;

						HashSet<Tag> hashSet = null;
						hashSet = WorldInventory.Instance.GetDiscoveredResourcesFromTag(favoritesCategoryHeader.ResourceCategoryTag);

						if (hashSet.Contains(tag)) {
							//Debug.Log("Toggle: Off");
							// NOTE: removes resource tracking in Favorites category but still displays the button
							hashSet.Remove(tag);

							favoritesCategoryHeader.ResourcesDiscovered.TryGetValue(tag, out resourceEntry);

							if (resourceEntry != null) {
								resourceEntry.gameObject.SetActive(false);
							}
						}else{
							//Debug.Log("Toggle: On");
							// Activate the ResourceEntry
							var discoverCat = Traverse.Create(WorldInventory.Instance).Method("DiscoverCategory", new[] { typeof(Tag), typeof(Tag) });
							discoverCat.GetValue(favoritesCategoryHeader.ResourceCategoryTag, tag);

							if(!favoritesCategoryHeader.ResourcesDiscovered.TryGetValue(tag, out resourceEntry)){
								List<Tag> ts = WorldInventory.Instance.GetPickupableTagsFromCategoryTag(tag);
								HashSet<Tag> tags = new HashSet<Tag>(ts);
								Tag cat = WorldInventory.GetCategoryForTags(tags);
								//Debug.Log("[Favorites] <ResourceEntry_OnClick> cat: " + cat);

								GameUtil.MeasureUnit measure = GameUtil.MeasureUnit.mass;
								if (GameTags.CalorieCategories.Contains(cat)) measure = GameUtil.MeasureUnit.kcal;
								if (GameTags.UnitCategories.Contains(cat)) measure = GameUtil.MeasureUnit.quantity;

								//Debug.Log("[Favorites] <ResourceEntry_OnClick> measure: " + measure);
								var newResourceEntry = Traverse.Create(favoritesCategoryHeader).Method("NewResourceEntry", new[] { typeof(Tag), typeof(GameUtil.MeasureUnit) });
								resourceEntry = newResourceEntry.GetValue<ResourceEntry>(tag, measure);
								favoritesCategoryHeader.ResourcesDiscovered.Add(tag, resourceEntry);
							}

							if (resourceEntry != null) {
								resourceEntry.gameObject.SetActive(true);
							}
						}
					}

				}
				return cont; // if true: execute original OnClick
			}
		}

		[HarmonyPatch(typeof(ResourceCategoryScreen), "OnActivate")]
		public static class ResourceCategoryScreen_OnActivate_Patch
		{
			public static void Postfix(ResourceCategoryScreen __instance)
			{
				// Create Favorites tag
				Tag favoritesTag = TagManager.Create("Favorites", "Favorites");

				// Create Favorites Header
				var createTagSetHeaders = Traverse.Create(__instance).Method("CreateTagSetHeaders", new[] { typeof(IEnumerable<Tag>), typeof(GameUtil.MeasureUnit) });
				createTagSetHeaders.GetValue(new TagSet{ favoritesTag }, GameUtil.MeasureUnit.mass);

				// reset DisplayedCategories, so Favorites is displayed
				var displayedCategoryKeys = Traverse.Create(__instance).Field("DisplayedCategoryKeys");
				displayedCategoryKeys.SetValue(__instance.DisplayedCategories.Keys.ToArray<Tag>());

				// keep our instance of favoritesCategoryHeader saved
				__instance.DisplayedCategories.TryGetValue(favoritesTag, out favoritesCategoryHeader);

				// move Favorites to the top of the list
				favoritesCategoryHeader.transform.SetAsFirstSibling();
			}
		}
	}
}
