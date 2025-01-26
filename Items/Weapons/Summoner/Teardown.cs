﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

using Origins.Dev;
namespace Origins.Items.Weapons.Summoner {
	public class Teardown : ModItem, ICustomWikiStat {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		static short glowmask;
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1;
		}
		public override void SetDefaults() {
			Item.damage = 11;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 18;
			Item.width = 58;
			Item.height = 68;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = buffID;
			Item.shoot = projectileID;
			Item.noMelee = true;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 10)
			.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (buffID == 0) buffID = ModContent.BuffType<Teardown_Buff>();
			player.AddBuff(buffID, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
	public class Teardown_P : ModProjectile {
		const string texture = "Origins/Items/Weapons/Summoner/Minions/Flying_Exoskeleton";
		public override string Texture => texture;
		public AutoLoadingAsset<Texture2D> glowTexture = texture + "_Glow";
		public const int frameSpeed = 5;
		public override void SetStaticDefaults() {
			Teardown.projectileID = Projectile.type;
			Main.projFrames[Projectile.type] = 3;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			Projectile.netImportant = true;
		}
		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Teardown.buffID);
			}
			if (player.HasBuff(Teardown.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top + new Vector2(player.direction * -player.width / 2, 0);
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float targetDist = 640f;
			float targetAngle = -2;
			Vector2 targetCenter = Projectile.Center;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (isPriorityTarget && Projectile.ai[1] < 0) foundTarget = true;
				if (npc.CanBeChasedBy()) {
					Vector2 diff = npc.Center - Projectile.Center;
					float dist = diff.Length();
					if (dist > targetDist) return;
					float dot = NormDotWithPriorityMult(diff, Projectile.velocity, targetPriorityMultiplier) - (player.DistanceSQ(npc.Center) / (640 * 640));
					bool inRange = dist <= targetDist;
					bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
					if (
						((dot >= targetAngle && inRange) || !foundTarget) &&
						(isPriorityTarget || lineOfSight || npc.whoAmI == Projectile.ai[0])
						) {
						targetDist = dist;
						targetAngle = dot;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			Projectile.friendly = foundTarget;
			#endregion

			#region Movement
			// Default movement parameters (here for attacking)
			float speed = 12f;
			float turnSpeed = 1.5f + (Projectile.ai[1] / 60f);
			float currentSpeed = Projectile.velocity.Length();
			if (foundTarget) {
				if (Projectile.ai[0] != target) {
					Projectile.ai[0] = target;
					Projectile.ai[1] = 0;
				} else {
					if (++Projectile.ai[1] > 180) {
						Projectile.ai[1] = -30;
					}
				}
				if ((int)Math.Ceiling(targetAngle) == -1) {
					targetCenter.Y -= 16;
				}
			} else {
				if (distanceToIdlePosition > 640f) {
					speed = 16f;
				} else if (distanceToIdlePosition < 64f) {
					speed = 4f;
					turnSpeed = 0.5f;
				} else {
					speed = 8f;
				}
			}
			LinearSmoothing(ref currentSpeed, speed, currentSpeed < 1 ? 1 : 0.1f);
			Vector2 direction = foundTarget ? targetCenter - Projectile.Center : vectorToIdlePosition;
			if (direction != Vector2.Zero) {
				direction.Normalize();
				Projectile.velocity = Vector2.Normalize(Projectile.velocity + direction * turnSpeed) * currentSpeed;
			}
			#endregion

			#region Animation and visuals

			Projectile.rotation = (float)Math.Atan(Projectile.velocity.Y / Projectile.velocity.X);
			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);

			// This is a simple "loop through all frames from top to bottom" animation
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
			#endregion
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.DefenseEffectiveness *= 0.5f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.ai[1] = 0;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Projectile.type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				new Vector2(25 + 15 * Projectile.direction, 16),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			Main.EntitySpriteDraw(
				glowTexture,
				Projectile.Center - Main.screenPosition,
				frame,
				new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f),
				Projectile.rotation,
				new Vector2(25 + 15 * Projectile.direction, 16),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Teardown_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Exoskeleton_Buff";
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Teardown.buffID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[Teardown.projectileID] > 0) {
				player.buffTime[buffIndex] = 18000;
			} else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
