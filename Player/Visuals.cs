﻿using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.Dev.PlagueTexan;
using Origins.Layers;
using Origins.Tiles.Defiled;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
			Item item = drawInfo.heldItem;
			if (item.ModItem is ICustomDrawItem) PlayerDrawLayers.HeldItem.Hide();

			if (mountOnly && !drawInfo.headOnlyRender) {
				for (int i = 0; i < PlayerDrawLayerLoader.DrawOrder.Count; i++) {
					PlayerDrawLayer layer = PlayerDrawLayerLoader.DrawOrder[i];
					if (layer != PlayerDrawLayers.MountFront && layer != PlayerDrawLayers.MountBack) {
						layer.Hide();
					}
				}
			}
			if (hideAllLayers) {
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
					layer.Hide();
				}
			} else if (dashVaseVisual) {
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
					if (layer is not Fallacious_Vase_Dash_Layer) layer.Hide();
				}
			}
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if (cursedCrownVisual) {
				drawInfo.skinDyePacked = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BlueAcidDye);
				const float alphaMult = 0.9f;
				drawInfo.colorHead *= alphaMult;
				drawInfo.colorBodySkin *= alphaMult;
				drawInfo.colorLegs *= alphaMult;
			}
			if (plagueSight) drawInfo.colorEyes = IsDevName(Player.name, 1) ? new Color(43, 185, 255) : Color.Gold;
			if (mysteriousSprayMult != 1f) {
				float lightSaturationMult = (float)Math.Pow(mysteriousSprayMult, 2f);
				float saturationMult = 1f - (float)Math.Pow(1f - mysteriousSprayMult, 1.5f);
				drawInfo.colorArmorHead = OriginExtensions.Desaturate(drawInfo.colorArmorHead, lightSaturationMult);
				drawInfo.colorArmorBody = OriginExtensions.Desaturate(drawInfo.colorArmorBody, lightSaturationMult);
				drawInfo.colorArmorLegs = OriginExtensions.Desaturate(drawInfo.colorArmorLegs, lightSaturationMult);
				drawInfo.floatingTubeColor = OriginExtensions.Desaturate(drawInfo.floatingTubeColor, lightSaturationMult);
				drawInfo.itemColor = OriginExtensions.Desaturate(drawInfo.itemColor, lightSaturationMult);

				drawInfo.headGlowColor = OriginExtensions.Desaturate(drawInfo.headGlowColor, saturationMult);
				drawInfo.armGlowColor = OriginExtensions.Desaturate(drawInfo.armGlowColor, saturationMult);
				drawInfo.bodyGlowColor = OriginExtensions.Desaturate(drawInfo.bodyGlowColor, saturationMult);
				drawInfo.legsGlowColor = OriginExtensions.Desaturate(drawInfo.legsGlowColor, saturationMult);

				drawInfo.colorElectricity = OriginExtensions.Desaturate(drawInfo.colorElectricity, saturationMult);
				drawInfo.ArkhalisColor = OriginExtensions.Desaturate(drawInfo.ArkhalisColor, saturationMult);

				drawInfo.colorHair = OriginExtensions.Desaturate(drawInfo.colorHair, saturationMult);
				drawInfo.colorHead = OriginExtensions.Desaturate(drawInfo.colorHead, saturationMult);
				drawInfo.colorEyes = Color.Lerp(drawInfo.colorEyes, Color.White, 1f - saturationMult);
				drawInfo.colorEyeWhites = Color.Lerp(drawInfo.colorEyeWhites, Color.Black, 1f - saturationMult);
				drawInfo.colorBodySkin = OriginExtensions.Desaturate(drawInfo.colorBodySkin, saturationMult);

			}
			if (drawInfo.drawPlayer.shield == Resin_Shield.ShieldID && resinShieldCooldown > 0) {
				drawInfo.drawPlayer.shield = (sbyte)Resin_Shield.InactiveShieldID;
			}
			if (tornCurrentSeverity > 0 && !GraphicsUtils.drawingEffect) {
				Torn_Debuff.cachedTornPlayers.Add(Player);
				Torn_Debuff.anyActiveTorn = true;
			}
			if (blizzardwalkerJacketVisual && blizzardwalkerActiveTime > 0) {
				float progress = blizzardwalkerActiveTime / (float)Blizzardwalkers_Jacket.max_active_time;
				drawInfo.colorEyes = Color.Lerp(drawInfo.colorEyes, Color.Red, progress * progress);
			}
		}
		public override void FrameEffects() {
			Debugging.LogFirstRun(FrameEffects);
			for (int i = 13; i < 18 + Player.extraAccessorySlots * 2; i++) {
				if (!Player.armor.IndexInRange(i)) break;
				if (Player.armor[i].type == Plague_Texan_Sight.ID) Plague_Texan_Sight.ApplyVisuals(Player);
			}
			if (shineSparkCharge > 0 || shineSparkDashTime > 0) {
				Player.armorEffectDrawShadow = true;
			}
		}
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			if (blizzardwalkerJacketVisual && blizzardwalkerActiveTime > 0) {
				float progress = blizzardwalkerActiveTime / (float)Blizzardwalkers_Jacket.max_active_time;
				progress *= progress;
				float progressInvColor = 1 - (progress * 0.8f);
				r = r * (1 - progress) * progressInvColor + (50 / 255f) * progress * 0.8f;
				g = g * (1 - progress) * progressInvColor;
				b = b * (1 - progress) * progressInvColor + (160 / 255f) * progress * 0.8f;
				a = a * (1 - progress) * progressInvColor + progress * 0.9f;
			}
			if (shineSparkCharge > 0 || shineSparkDashTime > 0) {
				fullBright = true;
				for (int i = 0; i < 3; i++) {
					const float speed_mult = -1.5f;
					int dust = Dust.NewDust(Player.position, Player.width, Player.height, DustID.VilePowder, Player.velocity.X * speed_mult, Player.velocity.Y * speed_mult);
					Main.dust[dust].noGravity = true;
					drawInfo.DustCache.Add(dust);
				}
			}
		}
		public override void PostUpdateRunSpeeds() {
			Debugging.LogFirstRun(PostUpdateRunSpeeds);
			oldGravDir = Player.gravDir;
			if (forceFallthrough) Player.GoingDownWithGrapple = true;
			forceFallthrough = false;
			if (cursedCrown && Player.velocity.Y == 0) {
				Player.maxRunSpeed *= 0.9f;
				Player.accRunSpeed *= 0.9f;
			}
			if (dashVase && Player.dashDelay < 0) {
				const float factor = 1.5f;
				Player.accRunSpeed *= factor;
				Player.maxRunSpeed *= factor;
			}
			int brambleType = ModContent.TileType<Tangela_Bramble>();
			foreach (Point point in Collision.GetTilesIn(Player.position, Player.BottomRight)) {
				if (Framing.GetTileSafely(point).TileIsType(brambleType)) {
					Tangela_Bramble.StandInside(Player);
					break;
				}
			}
		}
	}
}
