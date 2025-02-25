﻿using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Projectiles;
using Origins.Tiles.Brine;

namespace Origins.Items.Weapons.Ranged {
    public class Boat_Rocker : Harpoon_Gun, ICustomWikiStat {
        public new string[] Categories => [
            "HarpoonGun"
        ];
		public override void SetStaticDefaults() {
			ChainFrames = 3;
		}
		public override void SetDefaults() {
			Item.damage = 48;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 2;
			Item.width = 48;
			Item.height = 22;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 14f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextBool(4);
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.controlUseTile && player.releaseUseTile) {
				player.OriginPlayer().boatRockerAltUse = true;
			}
		}
		public override int GetChainFrame(int index, HarpoonGlobalProjectile global, Projectile projectile) {
			if (index == 0) {
				global.chainRandom = new(global.chainFrameSeed);
			}
			return global.chainRandom.Next(ChainFrames);
		}
	}
	public class Boat_Rocker_Alt : Boat_Rocker {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Boat_Rocker>()] = Type;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.controlUseTile && player.releaseUseTile) {
				player.OriginPlayer().boatRockerAltUse2 = true;
			}
		}
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Boat_Rocker>())
			.AddIngredient(Type)
			.AddIngredient<Peat_Moss_Item>(4)
			.Register();
		}
	}
}
