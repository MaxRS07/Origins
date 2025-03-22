using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Dissector : ModItem {
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumWaraxe);
			Item.damage = 16;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 0;
			Item.hammer = 0;
			Item.axe = 15;
			Item.width = 38;
			Item.height = 32;
			Item.useTime = 28;
			Item.useAnimation = 22;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
