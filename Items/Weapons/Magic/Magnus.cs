﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Buffs;
namespace Origins.Items.Weapons.Magic {
	public class Magnus : ModItem, ICustomWikiStat {
		public const int baseDamage = 34;
        public string[] Categories => [
            "Wand"
        ];
        public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			Origins.DamageBonusScale[Type] = 1.5f;
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ModContent.ProjectileType<Felnum_Zap>(), 40, 32, true);
			Item.damage = baseDamage;
			Item.UseSound = null;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.FallenStar, 20)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			//damage = damage.Scale(1.5f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
	}
	public class Felnum_Zap : ModProjectile {
		(Vector2?, Vector2)[] oldPos = new (Vector2?, Vector2)[7];
		public override string Texture => "Terraria/Images/Projectile_3";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft *= 3;
			Projectile.penetrate = -1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.position += oldVelocity;
			Projectile.velocity = Vector2.Zero;
			Projectile.timeLeft = 14;
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 1;
			height = 1;
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Static_Shock_Debuff>(), Main.rand.Next(120, 210));
			float targetWeight = 160;
			Vector2 targetPos = default;
			if (Main.player[Projectile.owner].DoHoming((target) => {
				if (target is NPC npc && Projectile.localNPCImmunity[npc.whoAmI] != 0) return false;
				Vector2 currentPos = target.Center;
				float dist = Math.Abs(Projectile.Center.X - currentPos.X) + Math.Abs(Projectile.Center.Y - currentPos.Y);
				if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
					targetWeight = dist;
					targetPos = currentPos;
					return true;
				}
				return false;
			}, false)) {
				Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(default) * Projectile.velocity.Length();
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			int l = Math.Min(Projectile.timeLeft, 7);
			if ((Projectile.timeLeft & 1) == 0) {
				for (int i = l; --i > 0;) {
					oldPos[i] = oldPos[i - 1];
					oldPos[i].Item1 += oldPos[i].Item2;
				}
				Vector2 dir = Main.rand.NextVector2Unit();
				oldPos[0] = (Projectile.Center + dir * 2, dir / 2);
			}
			List<Vector2> positions = oldPos.Where(i => i.Item1.HasValue).Select(i => i.Item1.Value - Main.screenPosition).ToList();
			positions.Insert(0, Projectile.Center - Main.screenPosition);
			Main.spriteBatch.DrawLightningArc(
				positions.ToArray(),
				null,
				1f,
				(0.15f, new Color(80, 204, 219, 0) * 0.5f),
				(0.1f, new Color(80, 251, 255, 0) * 0.5f),
				(0.05f, new Color(200, 255, 255, 0) * 0.5f));
			for (int i = 0; i < positions.Count; i++) {
				Lighting.AddLight(positions[i] + Main.screenPosition, 0.15f, 0.4f, 0.43f);
			}
			return false;
		}
		/// <summary>
		/// The core function of drawing a laser
		/// </summary>
		public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, Vector2? uScale = null, float maxDist = 200f, Color color = default) {
			Vector2 scale = uScale ?? new Vector2(0.66f, 0.66f);
			Vector2 origin = start;
			float maxl = (float)Math.Sqrt(Main.screenWidth * Main.screenWidth + Main.screenHeight * Main.screenHeight);
			float r = unit.ToRotation();
			float l = unit.Length() * 2.5f;
			int t = Projectile.timeLeft > 10 ? 25 - Projectile.timeLeft : Projectile.timeLeft;
			float s = Math.Min(t / 15f, 1f);
			Vector2 perpUnit = unit.RotatedBy(MathHelper.PiOver2);
			for (float i = 0; i <= maxDist; i += step) {
				if (i * unit.Length() > maxl) break;
				origin = start + i * unit;
				spriteBatch.Draw(texture, origin - Main.screenPosition,
					new Rectangle(0, 0, 1, 1), color, r,
					new Vector2(0.5f, 0.5f), scale, 0, 0);
				spriteBatch.Draw(texture, origin - Main.screenPosition + perpUnit * Main.rand.NextFloat(-1f, 1f),
					new Rectangle(0, 0, 1, 1), color, r,
					new Vector2(0.5f, 0.5f), scale, 0, 0);
				Lighting.AddLight(origin, color.R / 255f, color.G / 255f, color.B / 255f);
			}
		}
	}
}
