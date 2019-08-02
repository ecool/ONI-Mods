using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Harmony;

namespace FavoritesCategory {
	public class FavoritesCategoryPatches{
		public static ResourceCategoryHeader favoritesCategoryHeader = null;
		// Global.GameInputManager - class GameInputManager : KInputManager
		// KInputController KInputManager.GetDefaultController()
		// KInputController.GetKeyDown(KKeyCode key_code)

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
					// __instance.Resource
					/* Example output:
					[04:59:42.160] [1] [INFO] Dirt
					[04:59:44.123] [1] [INFO] Dirt
					[04:59:45.495] [1] [INFO] Sand
					[04:59:46.878] [1] [INFO] SandStone
					[04:59:47.607] [1] [INFO] Cuprite
					[04:59:49.185] [1] [INFO] Algae
					*/
					Tag tag = (Tag) __instance.Resource;
					Debug.Log(tag);
					Debug.Log(favoritesCategoryHeader);
					if (favoritesCategoryHeader != null) {
						var test1 = Traverse.Create(favoritesCategoryHeader).Method("NewResourceEntry", new[] { typeof(Tag), typeof(GameUtil.MeasureUnit) });
						ResourceEntry resourceEntry = (ResourceEntry) test1.GetValue(tag, GameUtil.MeasureUnit.mass);
						Debug.Log(resourceEntry);
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
				Debug.Log("--- ResourceCategoryScreen_OnActivate_Patch ---");

				// Create Favorites tag
				Tag favoritesTag = TagManager.Create("Favorites", "Favorites");
				/*
				Debug.Log("Tag: " + favoritesTag); // [INFO] Tag: Favorites

				// Create Header for Favorites
				var test = Traverse.Create(__instance).Method("NewCategoryHeader", new[] { typeof(Tag), typeof(GameUtil.MeasureUnit) });
				ResourceCategoryHeader favoritesCategoryHeader = (ResourceCategoryHeader) test.GetValue(favoritesTag, GameUtil.MeasureUnit.mass);

				Debug.Log("ResourceCategoryHeader: " + favoritesCategoryHeader); // [INFO] ResourceCategoryHeader: CategoryHeader_Favorites (ResourceCategoryHeader)

				// Create temporary ResourceEntry of Dirt to test if Category will show up
				//var test1 = Traverse.Create(favoritesCategoryHeader).Method("NewResourceEntry", new[] { typeof(Tag), typeof(GameUtil.MeasureUnit) });
				//ResourceEntry dirtResourceEntry = (ResourceEntry) test1.GetValue(GameTags.Dirt, GameUtil.MeasureUnit.mass);

				//Debug.Log("ResourceEntry: " + dirtResourceEntry); // [INFO] ResourceEntry: ResourceEntry (ResourceEntry)

				// NOTE: below doesn't seem to do anything
				// ???: try to add to DisplayedCategories
				__instance.DisplayedCategories.Add(favoritesTag, favoritesCategoryHeader);

				// ???: try to tell WorldInventory to Discover the Category
				//var test3 = Traverse.Create(WorldInventory.Instance).Method("DiscoverCategory", new[] { typeof(Tag), typeof(Tag) });
				//test3.GetValue(favoritesTag, GameTags.Dirt);
				*/

				// private void CreateTagSetHeaders(IEnumerable<Tag> set, GameUtil.MeasureUnit measure)
				// ResourceCategoryHeader NewCategoryHeader(Tag categoryTag, GameUtil.MeasureUnit measure)
				var test4 = Traverse.Create(__instance).Method("CreateTagSetHeaders", new[] { typeof(IEnumerable<Tag>), typeof(GameUtil.MeasureUnit) });
				test4.GetValue(new TagSet{ favoritesTag }, GameUtil.MeasureUnit.mass);

				var test5 = Traverse.Create(__instance).Field("DisplayedCategoryKeys");
				//var tags = new List<Tag>((Tag[]) test5.GetValue());
				//tags.Insert(0, favoritesTag);
				//test5.SetValue(tags.ToArray());
				test5.SetValue(__instance.DisplayedCategories.Keys.ToArray<Tag>());
				//private Tag[] DisplayedCategoryKeys

				__instance.DisplayedCategories.TryGetValue(favoritesTag, out favoritesCategoryHeader);
				Debug.Log(favoritesCategoryHeader);

			}
		}

		/* WorldInventory
		private void DiscoverCategory(Tag category_tag, Tag item_tag)
		{
			HashSet<Tag> hashSet;
			if (!this.DiscoveredCategories.TryGetValue(category_tag, out hashSet))
			{
				hashSet = new HashSet<Tag>();
				this.DiscoveredCategories[category_tag] = hashSet;
			}
			hashSet.Add(item_tag);
		}
		*/
	}
}
