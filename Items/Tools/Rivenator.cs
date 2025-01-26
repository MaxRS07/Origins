using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Rivenator : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.NightmarePickaxe);
			Item.damage = 16;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 80;
			Item.width = 34;
			Item.height = 32;
			Item.useTime = 12;
			Item.useAnimation = 24;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 12)
			.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
