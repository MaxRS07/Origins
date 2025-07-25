﻿using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Origins.Gores.NPCs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled.Boss {
	public class DA_Spike_Summon : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		NPC amalgam;
		public override void SetDefaults() {
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 100;
			Projectile.hide = true;
			Projectile.timeLeft = 1800;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNPC) {
				amalgam = parentNPC;
			}
		}
		public override void AI() {
			Projectile.localAI[0] += 2;
			if (Projectile.numUpdates == -1 && Projectile.extraUpdates == 0) {
				Projectile.timeLeft = 1800;
				int pos = (int)(Projectile.localAI[0] % (34 * 2));
				if (pos > 34) pos = 34 * 2 - pos;
				Dust.NewDustPerfect(Projectile.Center + new Vector2(pos - 17, 0), DustID.AncientLight, Vector2.UnitY * -2, newColor: Color.White, Scale: 0.75f).noGravity = true;
				Dust.NewDustPerfect(Projectile.Center - new Vector2(pos - 17, 0), DustID.AncientLight, Vector2.UnitY * -2, newColor: Color.White, Scale: 0.75f).noGravity = true;
				Projectile.ai[0] += 1 + ContentExtensions.DifficultyDamageMultiplier * 0.5f;
				if (Projectile.ai[0] > 120 && Main.netMode != NetmodeID.MultiplayerClient) {
					IEntitySource source = Projectile.GetSource_FromThis();
					if ((amalgam?.active ?? false) && amalgam.type == ModContent.NPCType<Defiled_Amalgamation>()) {
						source = amalgam.GetSource_FromAI();
					}
					Projectile.NewProjectile(
						source,
						Projectile.Center,
						Vector2.Zero,
						Main.expertMode ? ModContent.ProjectileType<DA_Spike_Bendy>() : ModContent.ProjectileType<DA_Spike>(),
						(int)(15 + 15 * ContentExtensions.DifficultyDamageMultiplier),
						0,
						ai1: Main.rand.Next(0, 32) * ContentExtensions.DifficultyDamageMultiplier,
						ai2: Main.rand.NextFloat(0.01f, 0.02f) * Main.rand.NextBool().ToDirectionInt()
					);
					Projectile.Kill();
					SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
					SoundEngine.PlaySound(Origins.Sounds.defiledKillAF.WithVolume(0.25f).WithPitchRange(0.4f, 0.6f), Projectile.Center);
				}
			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = Projectile.Center.Y <= Projectile.ai[2];
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			Projectile.extraUpdates = 0;
			Projectile.ai[0] = Main.rand.Next(10);
			Projectile.localAI[0] += Main.rand.Next(34);
			Projectile.tileCollide = false;
			Projectile.Center = new(Projectile.Center.X, (int)Math.Ceiling(Projectile.Center.Y / 16) * 16);
			return false;
		}
	}
	public class DA_Spike : ModProjectile {
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 34;
			Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
			Projectile.hide = true;
			Projectile.timeLeft = 20 + (int)(10 * ContentExtensions.DifficultyDamageMultiplier);
			Projectile.knockBack = 0;
		}
		public override void AI() {
			float maxGrowth = 96 * ContentExtensions.DifficultyDamageMultiplier - Projectile.ai[1];
			if (Projectile.ai[0] < maxGrowth) {
				int diff = (int)(Math.Min(Projectile.ai[0] + 16, maxGrowth) - Projectile.ai[0]);
				Projectile.ai[0] += diff;
				Projectile.position.Y -= diff;
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Height += (int)Projectile.ai[0];
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void OnKill(int timeLeft) {
			int[] gores = [
				ModContent.GoreType<DF_Effect_Medium1_Short>(),
				ModContent.GoreType<DF_Effect_Medium2_Short>(),
				ModContent.GoreType<DF_Effect_Medium3_Short>()
			];
			const int precision = 8;
			Rectangle tileCheck = new(0, 0, 8, 8);
			for (int i = 0; i < Projectile.ai[0]; i += precision) {
				float maxDeviation = Math.Min(i / 172f, 1) * 17;
				Vector2 pos = Projectile.position + new Vector2(17 + Main.rand.NextFloat(-maxDeviation, maxDeviation), i);
				tileCheck.X = (int)pos.X - 4;
				tileCheck.Y = (int)pos.Y - 4;
				if (tileCheck.OverlapsAnyTiles()) break;
				Gore.NewGore(Projectile.GetSource_Death(), pos, Vector2.Zero, Main.rand.Next(gores));
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 pos = Projectile.Center;
			const int precision = 4;
			Rectangle frame = new(0, 0, 34, precision);
			for (int i = 0; i < Projectile.ai[0]; i += precision) {
				frame.Y = i;
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					pos - Main.screenPosition,
					frame,
					new(Lighting.GetSubLight(pos)),
					0,
					Vector2.UnitX * 17,
					1,
					SpriteEffects.None
				);
				pos.Y += precision;
			}
			return false;
		}
	}
	public class DA_Spike_Bendy : DA_Spike {
		public override string Texture => typeof(DA_Spike).GetDefaultTMLName();
		public override void AI() {
			base.AI();
		}
		/// <summary>
		/// Call a programmer if this method lasts longer than four hours
		/// </summary>
		/// <returns></returns>
		(Vector2 pos, Vector2 perpendicular)[] GetCurve() {
			if (Projectile.localAI[2] != Projectile.ai[0]) {
				const int precision = 16;
				_curve = new (Vector2 pos, Vector2 perpendicular)[(int)Math.Ceiling(Projectile.ai[0] / precision)];
				Vector2 pos = Projectile.Center + Vector2.UnitY * Projectile.ai[0];
				Vector2 mov = -Vector2.UnitY * precision;
				int index = 0;
				for (int i = 0; i < Projectile.ai[0]; i += precision) {
					_curve[index++] = (pos, mov.RotatedBy(MathHelper.PiOver2) / precision);
					pos += mov;
					mov = mov.RotatedBy(Projectile.ai[2]);
				}
			}
			return _curve;
		}
		(Vector2 pos, Vector2 perpendicular)[] _curve = [];
		public (Vector2 pos, Vector2 perpendicular)[] Curve {
			get {
				return _curve;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			(Vector2 pos, Vector2 perpendicular)[] curve = GetCurve();
			Vector2 lastPos0 = curve[0].pos - curve[0].perpendicular * 17;
			Vector2 lastPos1 = curve[0].pos + curve[0].perpendicular * 17;
			Vector2 nextPos0 = default, nextPos1 = default;
			List<(Vector2 start, Vector2 end)> _lines = [(lastPos1, lastPos0)];
			for (int i = 0; i < curve.Length; i++) {
				Vector2 nextPos = curve[i].pos;
				if (nextPos == default) break;
				nextPos0 = nextPos - curve[i].perpendicular * 17;
				nextPos1 = nextPos + curve[i].perpendicular * 17;

				_lines.Add((lastPos0, nextPos0));
				_lines.Add((nextPos1, lastPos1));
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			_lines.Add((nextPos0, nextPos1));
			return CollisionExtensions.PolygonIntersectsRect(_lines.ToArray(), targetHitbox);
		}
		public override bool PreDraw(ref Color lightColor) {
			const int precision = 16;
			int segments = (int)Projectile.ai[0];
			Rectangle frame = new(0, segments, 34, precision - (precision - (segments % precision)) % precision);
			foreach ((Vector2 pos, Vector2 perpendicular) in GetCurve()) {
				frame.Y -= precision;
				if (frame.Height != precision) {
					frame.Height = precision;
					continue;
				}
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					pos - Main.screenPosition,
					frame,
					new(Lighting.GetSubLight(pos)),
					perpendicular.ToRotation(),
					Vector2.UnitX * 17,
					1,
					SpriteEffects.None
				);
			}
			/*(Vector2 pos, Vector2 perpendicular)[] curve = GetCurve();
			Vector2 lastPos0 = curve[0].pos - curve[0].perpendicular * 17;
			Vector2 lastPos1 = curve[0].pos + curve[0].perpendicular * 17;
			Vector2 nextPos0 = default, nextPos1 = default;
			List<(Vector2 start, Vector2 end)> _lines = [(lastPos1, lastPos0)];
			for (int i = 0; i < curve.Length; i++) {
				Vector2 nextPos = curve[i].pos;
				nextPos0 = nextPos - curve[i].perpendicular * 17;
				nextPos1 = nextPos + curve[i].perpendicular * 17;

				_lines.Add((lastPos0, nextPos0));
				_lines.Add((nextPos1, lastPos1));
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			_lines.Add((nextPos0, nextPos1));
			for (int i = 0; i < _lines.Count; i++) {
				OriginExtensions.DrawDebugLineSprite(_lines[i].start, _lines[i].end, Main.hslToRgb(i / (float)_lines.Count, 1, 0.5f), -Main.screenPosition);
			}*/
			return false;
		}
	}
}
