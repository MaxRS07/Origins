using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Buffs;
namespace Origins.Items.Weapons.Melee {
    public class Depth_Charge : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Flail"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.ToolTipDamageMultiplier[Type] = 2f;
		}
		public override void SetDefaults() {
			Item.damage = 55;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.MeleeNoSpeed];
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 66;
			Item.height = 68;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Depth_Charge_P>();
			Item.shootSpeed = 8f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
	}
	public class Depth_Charge_P : ModProjectile, IIsExplodingProjectile {
		public static AutoCastingAsset<Texture2D> ChainTexture { get; private set; }
		const int ai_state_spinning = 0;
		const int ai_state_launching_forward = 1;
		const int ai_state_retracting = 2;
		const int ai_state_unused_state = 3;
		const int ai_state_forced_retracting = 4;
		const int ai_state_ricochet = 5;
		const int ai_state_dropping = 6;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Depth Charge");
			if (Mod.RequestAssetIfExists<Texture2D>("Items/Weapons/Melee/Depth_Charge_Chain", out var chainTexture)) ChainTexture = chainTexture;
		}
		public override void Unload() {
			ChainTexture = null;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Sunfury);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[0] != ai_state_spinning) Projectile.penetrate = 0;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[0] != ai_state_spinning) {
				Projectile.penetrate = 0;
				Projectile.position = Vector2.Lerp(Projectile.position, Projectile.oldPosition + oldVelocity, 0.5f);
				Projectile.velocity = Vector2.Zero;
				Projectile.tileCollide = false;
			}
			return true;
		}
		public override bool PreDrawExtras() {
			Vector2 chainDrawPosition = Projectile.Center;
			Vector2 vectorFromProjectileToPlayerArms = Main.GetPlayerArmPosition(Projectile).MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
			float rotation = vectorFromProjectileToPlayerArms.ToRotation();
			List<Vector2> chainPositions = GetChainPositions(chainDrawPosition, vectorFromProjectileToPlayerArms);
			for (int i = 0; i < chainPositions.Count; i++) {
				Main.EntitySpriteDraw(
					ChainTexture,
					chainPositions[i] - Main.screenPosition,
					null,
					Lighting.GetColor(chainPositions[i].ToTileCoordinates()),
					rotation,
					new Vector2(6, 3),
					1,
					0,
				0);
			}
			return false;
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.penetrate >= 0) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Depth_Charge_Explosion>(),
					Projectile.damage * 2,
					Projectile.knockBack * 2,
					Projectile.owner
				);
				Vector2 chainDrawPosition = Projectile.Center;
				Vector2 vectorFromProjectileToPlayerArms = Main.GetPlayerArmPosition(Projectile).MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
				List<Vector2> chainPositions = GetChainPositions(chainDrawPosition, vectorFromProjectileToPlayerArms);
				for (int i = 0; i < chainPositions.Count; i++) {
					Gore.NewGore(
						Projectile.GetSource_Death(),
						chainPositions[i],
						Projectile.velocity * 0.1f,
						ModContent.GoreType<Depth_Charge_Chain>()
					);
				}
			}
		}
		List<Vector2> GetChainPositions(Vector2 chainDrawPosition, Vector2 vectorFromProjectileToPlayerArms) {
			const int overlapPixels = 1;
			const float chainLength = 12 - (overlapPixels * 2);
			Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero) * chainLength;
			float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() / chainLength + 1;
			List<Vector2> chainPositions = new();
			while (chainLengthRemainingToDraw > 0f) {
				chainPositions.Add(chainDrawPosition);
				chainDrawPosition += unitVectorFromProjectileToPlayerArms;
				chainLengthRemainingToDraw--;
			}
			return chainPositions;
		}

		public bool IsExploding() => false;
	}
	public class Depth_Charge_Explosion : ModProjectile, IIsExplodingProjectile, ISelfDamageEffectProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
				Projectile.ai[0] = 1;
			}
			if (Projectile.ai[1] == 0) {
				ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.wet) target.AddBuff(Cavitation_Debuff.ID, 90);
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
		public void OnSelfDamage(Player player, Player.HurtInfo info, double damageDealt) {
			if (player.wet) player.AddBuff(Cavitation_Debuff.ID, 90);
			Projectile.ai[1] = 1;
		}
	}
	public class Depth_Charge_Chain : ModGore {
		public override void OnSpawn(Gore gore, IEntitySource source) {
			gore.velocity *= 2;
			gore.frameCounter = (byte)Main.rand.Next(1, 5);
		}
		public override bool Update(Gore gore) {
			gore.alpha += gore.frameCounter;
			if (gore.alpha > 75) {
				gore.alpha += gore.frameCounter * 2;
				if (gore.alpha > 250) {
					gore.timeLeft = 0;
				}
			}
			return true;
		}
	}
}
