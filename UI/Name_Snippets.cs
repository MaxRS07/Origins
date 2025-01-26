﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items;
using Origins.Journal;
using System.Security.Policy;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Player_Name_Handler : ITagHandler {
		public class Player_Name_Snippet(Color color = default) : TextSnippet(Main.LocalPlayer.name, color) { }
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) => new Player_Name_Snippet(baseColor);
	}
	public class NPC_Name_Handler : ITagHandler {
		public class NPC_Name_Snippet : TextSnippet {
			readonly int type;
			int lastHovered = 0;
			readonly bool isRealName = false;
			public NPC_Name_Snippet(int type, Color color = default) : base() {
				this.type = type;
				if (type == -1) {
					Text = "Invalid NPC type";
					return;
				}
				if (NPC.GetFirstNPCNameOrNull(type) is string name) {
					Text = name;
					isRealName = true;
				} else {
					Text = Language.GetOrRegister("Mods.Origins.Generic.Definite_Article").Format(Lang.GetNPCNameValue(type));
					isRealName = false;
				}
				CheckForHover = true;
				Color = color;
			}
			public override void Update() {
				base.Update();
				if (lastHovered > 0) lastHovered--;
			}
			public override void OnHover() {
				base.OnHover();
				lastHovered = 4;
				Main.LocalPlayer.mouseInterface = true;
				if (isRealName) {
					UICommon.TooltipMouseText(Language.GetOrRegister("Mods.Origins.Generic.Definite_Article").Format(Lang.GetNPCNameValue(type)));
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if ((int.TryParse(text, out int npcType) && npcType < NPCLoader.NPCCount) || NPCID.Search.TryGetId(text, out npcType)) {
				return new NPC_Name_Snippet(npcType, baseColor);
			}
			return new NPC_Name_Snippet(-1, baseColor);
		}
	}
	public class Item_Name_Handler : ITagHandler {
		public class Item_Name_Snippet : TextSnippet {
			readonly Item item;
			int lastHovered = 0;
			public Item_Name_Snippet(int type, Color color = default) : base() {
				if (type == -1) {
					Text = "Invalid Item type";
					return;
				}
				item = ContentSamples.ItemsByType[type];
				Text = Lang.GetItemNameValue(type);
				CheckForHover = true;
				Color = color;
			}
			public override void Update() {
				base.Update();
				if (lastHovered > 0) lastHovered--;
			}
			public override void OnHover() {
				base.OnHover();
				lastHovered = 4;
				Main.LocalPlayer.mouseInterface = true;
				if (item is not null) {
					Main.hoverItemName = $"{item.Name} [i:{item.type}]";
					Main.HoverItem = item.Clone();
					Main.HoverItem.SetNameOverride(Main.hoverItemName);
					Main.instance.MouseText(Main.hoverItemName, item.rare, 0);
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if ((int.TryParse(text, out int itemType) && itemType < ItemLoader.ItemCount) || ItemID.Search.TryGetId(text, out itemType)) {
				return new Item_Name_Snippet(itemType, baseColor);
			}
			return new Item_Name_Snippet(-1, baseColor);
		}
	}
	public class Imperfect_Item_Name_Handler : ITagHandler {
		public class Imperfect_Item_Name_Snippet : TextSnippet {
			readonly Item item;
			int lastHovered = 0;
			public Imperfect_Item_Name_Snippet(int type, Color color = default) : base() {
				if (type == -1) {
					Text = "Invalid Item type";
					return;
				}
				item = ContentSamples.ItemsByType[type].Clone();
				item.Prefix(ModContent.PrefixType<Imperfect_Prefix>());
				Text = item.AffixName();
				CheckForHover = true;
				Color = color;
			}
			public override void Update() {
				base.Update();
				if (lastHovered > 0) lastHovered--;
			}
			public override void OnHover() {
				base.OnHover();
				lastHovered = 4;
				Main.LocalPlayer.mouseInterface = true;
				if (item is not null) {
					Main.hoverItemName = $"{item.Name} [i:{item.type}]";
					Main.HoverItem = item.Clone();
					Main.HoverItem.SetNameOverride(Main.hoverItemName);
					Main.instance.MouseText(Main.hoverItemName, item.rare, 0);
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if ((int.TryParse(text, out int itemType) && itemType < ItemLoader.ItemCount) || ItemID.Search.TryGetId(text, out itemType)) {
				return new Imperfect_Item_Name_Snippet(itemType, baseColor);
			}
			return new Imperfect_Item_Name_Snippet(-1, baseColor);
		}
	}
}
