﻿using Microsoft.Xna.Framework;
using Origins.Walls;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Defiled_Sandstone : OriginTile, IDefiledTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.Sandstone];
			Main.tileMerge[TileType<Defiled_Sand>()][Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.Sandstone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			//Main.tileMerge[TileID.Sandstone][Type] = true;
			//Main.tileMerge[Type] = Main.tileMerge[TileID.Sandstone];
			//Main.tileMerge[Type][TileID.Sandstone] = true;
			/*for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Sandstone];
            }*/
			AddMapEntry(new Color(150, 150, 150));
			mergeID = TileID.Sandstone;
			AddDefiledTile();
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			TileExtenstions.DoFraming(i, j, resetFrame, map: [(Type, 1), (TileType<Defiled_Sand>(), 2)], TileExtenstions.ExtraTileBlending);
			return false;
		}
	}
	public class Defiled_Sandstone_Item : ModItem {
		public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled_Sandstone_Item>()] = ModContent.ItemType<Defiled_Sand_Item>();
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.Sandstone, Type);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Defiled_Sandstone>());
		}
    }
}
