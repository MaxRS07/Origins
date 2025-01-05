﻿using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Conditions;
using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Tools;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs.Defiled;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Riven;
using Origins.Projectiles.Misc;
using Origins.Questing;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs {
	public partial class OriginGlobalNPC : GlobalNPC {
		public override void SetStaticDefaults() {
			NPCHappiness.Get(NPCID.PartyGirl)
			.SetNPCAffection(NPCID.SantaClaus, AffectionLevel.Love)
			.SetBiomeAffection<SpaceBiome>(AffectionLevel.Love);
		}
		public static ShoppingSettings ShopHelper_GetShoppingSettings(On_ShopHelper.orig_GetShoppingSettings orig, ShopHelper self, Player player, NPC npc) {
			ShoppingSettings settings = orig(self, player, npc);
			float discount = 0;
			switch (npc.type) {
				case NPCID.BestiaryGirl:
				if (ModContent.GetInstance<Discount_1_Quest>().Completed) {
					discount += 0.03f;
				}
				if (ModContent.GetInstance<Discount_2_Quest>().Completed) {
					discount += 0.03f;
				}
				break;
			}
			settings.PriceAdjustment *= 1 - discount;
			return settings;
		}
		public override void ModifyShop(NPCShop shop) {
			switch (shop.NpcType) {
				case NPCID.Clothier: {
					shop.Add<Pincushion>();
				}
				break;
				case NPCID.GoblinTinkerer: {
					shop.Add<Turbo_Reel>(Quest.QuestCondition<Turbo_Reel_Quest>());
					shop.Add<Gun_Glove>(Quest.QuestCondition<Gun_Glove_Quest>());
					break;
				}
				case NPCID.Merchant: {
					shop.Add<Blue_Bovine>(Quest.QuestCondition<Blue_Bovine_Quest>());
					shop.Add<Lottery_Ticket>(Quest.QuestCondition<Lottery_Ticket_Quest>());
					break;
				}
				case NPCID.Demolitionist: {
					shop.Add(ItemID.ExplosivePowder, Condition.PreHardmode);
					shop.Add<Peatball>(PeatSoldCondition(15));
					shop.Add<Flashbang>(PeatSoldCondition(25));
					shop.Add<IWTPA_Standard>(PeatSoldCondition(35));
					shop.Add<Impact_Grenade>(PeatSoldCondition(40));
					shop.Add<Defiled_Spirit>(PeatSoldCondition(50), WorldEvilBossCondition<Defiled_Wastelands_Alt_Biome>("Mods.Origins.Conditions.DownedDefiledAmalgamation"));
					shop.Add<Ameballoon>(PeatSoldCondition(60), WorldEvilBossCondition<Riven_Hive_Alt_Biome>("Mods.Origins.Conditions.DownedWorldCracker"));
					shop.Add<Impact_Bomb>(PeatSoldCondition(70));
					shop.Add<Brainade>(PeatSoldCondition(81), Condition.DownedBrainOfCthulhu);
					shop.Add<Link_Grenade>(PeatSoldCondition(85), ShopConditions.GetWorldEvilCondition<Ashen_Alt_Biome>());
					shop.Add<Nitro_Crate>(PeatSoldCondition(100));
					shop.Add<Outbreak_Bomb>(PeatSoldCondition(110), Condition.DownedEaterOfWorlds);
					shop.Add<Shrapnel_Bomb>(PeatSoldCondition(125), WorldEvilBossCondition<Ashen_Alt_Biome>("Mods.Origins.Conditions.DownedScrapper"));
					shop.Add<Magic_Tripwire>(PeatSoldCondition(135));
					shop.Add<Trash_Lid>(PeatSoldCondition(150));
					//shop.Add(ItemID.Beenade)(PeatSoldCondition(170), Condition.NotTheBeesWorld);
					shop.Add<Impact_Dynamite>(PeatSoldCondition(180), Condition.Hardmode);
					shop.Add<Alkaline_Grenade>(PeatSoldCondition(200), Condition.Hardmode); // Lost Diver condition for both
					shop.Add<Alkaline_Bomb>(PeatSoldCondition(230), Condition.Hardmode);
					shop.Add<Indestructible_Saddle>(PeatSoldCondition(250), Condition.DownedMechBossAny);
					shop.Add<Caustica>(PeatSoldCondition(999), Condition.Hardmode);
					break;
				}
				case NPCID.Steampunker: {
					shop.InsertAfter<Gray_Solution>(ItemID.PurpleSolution, Condition.EclipseOrBloodMoon.And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>()));
					shop.InsertAfter<Teal_Solution>(ItemID.RedSolution, Condition.EclipseOrBloodMoon.And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>()));
					break;
				}
				case NPCID.Dryad: {
					shop.Add<Bloombomb>(Quest.QuestCondition<Bloombomb_Quest>());
					shop.Add<Cleansing_Station_Item>(Quest.QuestCondition<Cleansing_Station_Quest>());
					shop.Add<Mojo_Flask>(Quest.QuestCondition<Cleansing_Station_Quest>());

					shop.InsertAfter<Dreadful_Powder>(ItemID.CorruptGrassEcho, Condition.NotRemixWorld.CommaAnd(Condition.BloodMoon).And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>()));
					shop.InsertAfter<Sentient_Powder>(ItemID.CrimsonGrassEcho, Condition.NotRemixWorld.CommaAnd(Condition.BloodMoon).And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>()));

					shop.InsertAfter<Dreadful_Powder, Defiled_Grass_Seeds>(Condition.BloodMoon.And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>()));
					shop.InsertAfter<Sentient_Powder, Riven_Grass_Seeds>(Condition.BloodMoon.And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>()));

					shop.InsertAfter<Defiled_Grass_Seeds>(ItemID.CorruptSeeds, Condition.InGraveyard.CommaAnd(Condition.Hardmode).And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>().Not()));
					shop.InsertAfter<Riven_Grass_Seeds>(ItemID.CrimsonSeeds, Condition.InGraveyard.CommaAnd(Condition.Hardmode).And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>().Not()));
					break;
				}
				case NPCID.Cyborg: {
					shop.Add<Advanced_Imaging>(Quest.QuestCondition<Advanced_Imaging_Quest>());
					break;
				}
				case NPCID.SkeletonMerchant: {
					shop.Add(ItemID.BlackInk);
					shop.Add<Trash_Lid>(Condition.MoonPhaseFull);
					break;
				}
				case NPCID.Golfer: {
					shop.Add<Baseball_Bat>(OriginsModIntegrations.AprilFools);
					break;
				}
				case NPCID.ArmsDealer: {
					shop.Add(ModContent.ItemType<Gun_Magazine>());
					shop.Add<Shardcannon>(Quest.QuestCondition<Shardcannon_Quest>());
					shop.Add<Harpoon_Burst_Rifle>(Quest.QuestCondition<Harpoon_Burst_Rifle_Quest>());
					break;
				}
				case NPCID.Stylist: {
					shop.Add<Holiday_Hair_Dye>(Quest.QuestCondition<Holiday_Hair_Dye_Quest>());
					break;
				}
				case NPCID.WitchDoctor: {
					shop.InsertAfter<Defiled_Fountain_Item>(ItemID.CorruptWaterFountain);
					shop.InsertAfter<Riven_Fountain_Item>(ModContent.ItemType<Defiled_Fountain_Item>());
					shop.InsertAfter<Brine_Fountain_Item>(ModContent.ItemType<Riven_Fountain_Item>());
					break;
				}
				case NPCID.Mechanic: {
					shop.Add<Fabricator_Item>(Condition.DownedMechBossAll);
					break;
				}
				case NPCID.PartyGirl: {
					shop.Add<Partybringer>(Quest.QuestCondition<Tax_Collector_Hat_Quest>());
					break;
				}
			}
		}
		public override bool PreAI(NPC npc) {
			if (npc.oldPosition == default && npc.oldVelocity == default && npc.position.LengthSquared() > 16) {
				npc.oldPosition = npc.position;
			}
			preAIVelocity = npc.velocity;
			if (shockTime > 0) {
				npc.noGravity = true;
				npc.velocity = Vector2.Zero;
				npc.position = npc.oldPosition;
				if (--shockTime == 0) {
					npc.life = 0;
					npc.checkDead();
				}
				return false;
			}
			if (npc.HasBuff(Impaled_Debuff.ID)) {
				//npc.position = npc.oldPosition;//-=npc.velocity;
				npc.velocity = Vector2.Zero;
				return false;
			}
			if (rasterizedTime > 0 && npc.oldPosition != default) {
				if (Math.Abs(npc.velocity.Y) < 0.001f) {
					switch (npc.aiStyle) {
						case NPCAIStyleID.Slime:
						case NPCAIStyleID.King_Slime:
						case NPCAIStyleID.Queen_Slime:
						npc.oldVelocity.Y = 0;
						break;
					}
				}
				float accelerationFactor = 1; 
				float velocityFactor = 1; 
				if (Origins.RasterizeAdjustment.TryGetValue(npc.type, out var adjustment)) {
					accelerationFactor = adjustment.accelerationFactor;
					velocityFactor = adjustment.velocityFactor;
				}
				npc.velocity = Vector2.Lerp(npc.velocity, npc.oldVelocity, rasterizedTime * 0.0625f * 0.5f * accelerationFactor);
				npc.position = Vector2.Lerp(npc.position, npc.oldPosition, rasterizedTime * 0.0625f * 0.5f * velocityFactor);
			}
			if (slowDebuff) {
				npc.position = Vector2.Lerp(npc.oldPosition, npc.position, 0.7f);
			}
			if (barnacleBuff) {
				Vector2 vel = (npc.velocity * 0.2f).WithMaxLength(8);
				npc.position += npc.noTileCollide ? vel : Collision.AnyCollision(npc.position, vel, npc.width, npc.height);
			}
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				if (toxicShockStunTime > 0) {
					toxicShockStunTime--;
					npc.position -= npc.velocity;
					return false;
				}
			} else if (toxicShockStunTime > 0) {
				toxicShockStunTime = 0;
			}
			if (npc.HasBuff(BuffID.OgreSpit) && (npc.collideX || npc.collideY)) {
				npc.velocity *= 0.95f;
			}
			infusionSpikes?.Clear();
			if (deadBird) return false;
			return true;
		}
		public override void PostAI(NPC npc) {
			if (barnacleBuff) {
				if (Math.Abs(preAIVelocity.Y) <= 0.075f && npc.velocity.Y < preAIVelocity.Y) {
					npc.velocity.Y *= 1.2f;//
				}
			}
			if (birdedTime > 0 && (deadBird || npc.knockBackResist != 0)) {
				const float birdKnockback = 8;
				if (npc.velocity.LengthSquared() < 7 * 7) birdedTime = 0;
				else {
					if (npc.collideX || npc.collideY) {
						if (airBird) {
							birdedTime = 0;
							npc.SimpleStrikeNPC(birdedDamage, Math.Sign(npc.velocity.X - npc.oldVelocity.X), false, birdKnockback);
							airBird = false;
						}
					} else {
						airBird = true;
						Rectangle hitbox = npc.Hitbox;
						foreach (NPC other in Main.ActiveNPCs) {
							if (other.whoAmI != npc.whoAmI && other.Hitbox.Intersects(hitbox)) {
								birdedTime = 0;
								int dir = -Math.Sign(other.Center.X - npc.Center.X);
								npc.SimpleStrikeNPC(birdedDamage, dir, false, birdKnockback);
								other.SimpleStrikeNPC(birdedDamage, -dir, false, birdKnockback);
								break;
							}
						}
					}
				}
			}
		}
		public override bool CheckDead(NPC npc) {
			if (birdedTime > 0) {
				deadBird = true;
				npc.life = 1;
				return false;
			}
			return true;
		}
		public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			switch (npc.type) {
				case NPCID.DesertDjinn:
				case NPCID.DesertLamiaDark:
				bestiaryEntry.AddTags(
					ModContent.GetInstance<Defiled_Wastelands_Underground_Desert>().ModBiomeBestiaryInfoElement,
					ModContent.GetInstance<Riven_Hive_Underground_Desert>().ModBiomeBestiaryInfoElement
				);
				break;
			}
		}
		public override bool CanHitNPC(NPC npc, NPC target) {
			if (birdedTime > 0 && (deadBird || npc.knockBackResist != 0)) return false;
			return true;
		}
		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
			if ((birdedTime > 0 && (deadBird || npc.knockBackResist != 0)) || npc.HasBuff(Impaled_Debuff.ID) || npc.HasBuff(Stunned_Debuff.ID)) return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				modifiers.CritDamage *= 1.3f;
				modifiers.Defense -= 0.2f;
				if (npc.HasBuff(Toxic_Shock_Strengthen_Debuff.ID)) {// roughly 50% boost
					modifiers.CritDamage *= 1.1f;
					modifiers.Defense -= 0.1f;
				}
			}
			if (tornCurrentSeverity > 0) {
				modifiers.FinalDamage /= 1 - tornCurrentSeverity;
			}
			if (barnacleBuff) {
				modifiers.Defense += 0.25f;
				modifiers.Defense.Flat += 5;
			}
			if (amberDebuff) modifiers.Defense *= 0.5f;
		}
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (projectile.IsMinionOrSentryRelated) {
				float damageBoost = 0;
				for (int i = 0; i < npc.buffType.Length; i++) {
					if (npc.buffTime[i] <= 0) continue;
					int buffType = npc.buffType[i];
					if (buffType == Flagellash_Buff_0.ID) {
						damageBoost += 2.7f;
					} else if (buffType == Flagellash_Buff_1.ID) {
						damageBoost += 2.65f;
					} else if (buffType == Flagellash_Buff_2.ID) {
						damageBoost += 2.65f;
					} else if (buffType == Maelstrom_Buff_Damage.ID) {
						damageBoost += npc.HasBuff(Maelstrom_Buff_Zap.ID) ? 7f : 5f;
					} else if (buffType == Futurephones_Buff.ID) {
						modifiers.SourceDamage *= 1.05f;
						if (Main.rand.NextBool(10)) modifiers.SetCrit();
					}
				}
				if (amebolizeDebuff) {
					damageBoost += 5f;
				}
				if (beeAfraidDebuff) {
					damageBoost += 6f;
				}
				if (jointPopDebuff) {
					damageBoost += 3f;
				}
				if (ziptieDebuff) {
					damageBoost += 6f;
				}
				if (beeIncantationDebuff) {
					damageBoost += 5f;
				}
				if (beeIncantationDebuff) {
					damageBoost += 7f;
				}
				if (hibernalIncantationDebuff) {
					damageBoost += 4f;
				}
				modifiers.FlatBonusDamage += Main.rand.RandomRound(damageBoost * ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type]);
			} else if (npc.HasBuff(Futurephones_Buff.ID)) {
				modifiers.SourceDamage *= 1.05f;
				if (Main.rand.NextBool(10)) modifiers.SetCrit();
			}
			int forceCritBuff = npc.FindBuffIndex(Headphones_Buff.ID);
			if (forceCritBuff >= 0) {
				if (Main.rand.NextBool(4)) modifiers.SetCrit();
				npc.DelBuff(forceCritBuff);
			}
			if (Main.expertMode) {
				if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail) {
					switch (projectile.type) {
						case ProjectileID.Bomb:
						case ProjectileID.Dynamite:
						case ProjectileID.Grenade:
						case ProjectileID.StickyBomb:
						case ProjectileID.Explosives:
						case ProjectileID.Bee://well, is there a way to track if they came out of an explosive?
						case ProjectileID.Beenade:
						case ProjectileID.ExplosiveBunny:
						case ProjectileID.StickyGrenade:
						case ProjectileID.StickyDynamite:
						case ProjectileID.BouncyBomb:
						case ProjectileID.BouncyGrenade:
						case ProjectileID.BombFish:
						case ProjectileID.GiantBee:
						case ProjectileID.PartyGirlGrenade:
						case ProjectileID.BouncyDynamite:
						case ProjectileID.ScarabBomb:
						if (!Main.masterMode) modifiers.SourceDamage *= (float)new Fraction(5, 2);
						break;
						default:
						if (projectile.CountsAsClass(DamageClasses.Explosive)) modifiers.SourceDamage /= Main.masterMode ? 5 : 2;
						break;
					}
				}
			}
		}
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (projectile.IsMinionOrSentryRelated) {
				if (npc.HasBuff(Maelstrom_Buff_Zap.ID) && projectile.owner == Main.myPlayer) {
					const float maxDist = 136 * 136;
					bool first = true;
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC currentTarget = Main.npc[i];
						if (i == npc.whoAmI || !currentTarget.CanBeChasedBy()) continue;

						Vector2 currentStart = currentTarget.Center.Clamp(npc.Hitbox);
						Vector2 currentEnd = npc.Center.Clamp(currentTarget.Hitbox);
						float currentDist = (currentEnd - currentStart).LengthSquared();

						if (currentDist < maxDist && (first || Main.rand.NextBool(3))) {
							first = false;
							Projectile.NewProjectileDirect(
								projectile.GetSource_OnHit(npc),
								currentEnd,
								default,
								Felnum_Shock_Leader.ID,
								Main.rand.RandomRound(Math.Max(5 * ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type], 1)),
								hit.Knockback,
								projectile.owner,
								currentStart.X,
								currentStart.Y
							).ArmorPenetration = 20;
						}
					}
				}
			}
		}
		public override void HitEffect(NPC npc, NPC.HitInfo hit) {
			if (OriginPlayer.LocalOriginPlayer.priorityMail) {
				prevPriorityMailTime = priorityMailTime;
				priorityMailTime = 300;
			}
		}
		/*public override void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit) {
			knockback*=MeleeCollisionNPCData.knockbackMult;
			MeleeCollisionNPCData.knockbackMult = 1f;
		}*/
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
			Player player = spawnInfo.Player;
			if (player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerStardust || player.ZoneTowerVortex) {
				return;
			}
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (spawnInfo.SpawnTileType == ModContent.TileType<Fiberglass_Tile>()) {
				pool.Add(ModContent.NPCType<Fiberglass.Enchanted_Fiberglass_Sword>(), Fiberglass_Undergrowth.SpawnRates.Sword);
				pool.Add(ModContent.NPCType<Fiberglass.Enchanted_Fiberglass_Bow>(), Fiberglass_Undergrowth.SpawnRates.Bow);
				pool.Add(ModContent.NPCType<Fiberglass.Enchanted_Fiberglass_Pistol>(), Fiberglass_Undergrowth.SpawnRates.Gun);
				pool.Add(ModContent.NPCType<Fiberglass.Enchanted_Fiberglass_Cannon>(), Fiberglass_Undergrowth.SpawnRates.Gun);
				pool.Add(ModContent.NPCType<Fiberglass.Fiberglass_Weaver>(), Fiberglass_Undergrowth.SpawnRates.Spider);
				pool[0] = 0;
				return;
			}
			if (originPlayer.ZoneFiberglass) {
				pool[0] = 0;
				return;
			}
			if (player.InModBiome<Defiled_Wastelands>()) {
				if (Defiled_Amalgamation.spawnDA) {
					foreach (var entry in pool) {
						pool[entry.Key] = 0;
					}
					if (spawnInfo.PlayerFloorY < Main.worldSurface && Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY].WallType != ModContent.WallType<Walls.Defiled_Stone_Wall>()) {
						pool.Add(ModContent.NPCType<Defiled_Amalgamation>(), 9999);
					}
					return;
				}
			}
			if (spawnInfo.DesertCave) {
				if (player.InModBiome<Defiled_Wastelands>() || player.InModBiome<Riven_Hive>()) {
					pool[NPCID.DesertDjinn] = 1f;
					pool[NPCID.DesertLamiaDark] = 1f;
					pool[NPCID.DesertBeast] = 1f;
				}
			}
			if (TileLoader.GetTile(spawnInfo.SpawnTileType) is IDefiledTile) {
				if (Main.invasionType <= 0) pool[0] = 0;

				if (spawnInfo.SpawnTileY > Main.worldSurface && !spawnInfo.DesertCave) {
					int yPos = spawnInfo.SpawnTileY;
					Tile tile;
					for (int i = 0; i < Defiled_Mite.spawnCheckDistance; i++) {
						tile = Main.tile[spawnInfo.SpawnTileX, ++yPos];
						if (tile.HasTile) {
							yPos--;
							break;
						}
					}
					bool? halfSlab = null;
					for (int i = spawnInfo.SpawnTileX - 1; i < spawnInfo.SpawnTileX + 2; i++) {
						tile = Main.tile[i, yPos + 1];
						if (!tile.HasTile || !Main.tileSolid[tile.TileType] || tile.Slope != SlopeID.None || (halfSlab.HasValue && halfSlab.Value != tile.IsHalfBlock)) {
							goto SkipMiteSpawn;
						}
						halfSlab = tile.IsHalfBlock;
					}
					pool.Add(ModContent.NPCType<Defiled_Mite>(), Defiled_Wastelands.SpawnRates.Mite);
					SkipMiteSpawn:;
				}
			}
			if (TileLoader.GetTile(spawnInfo.SpawnTileType) is IRivenTile || player.InModBiome<Riven_Hive>()) {
				if (Main.invasionType <= 0) pool[0] = 0;
			}
			if (Main.hardMode && !spawnInfo.PlayerSafe && spawnInfo.SpawnTileY > Main.rockLayer && !spawnInfo.DesertCave) {
				if (player.InModBiome<Defiled_Wastelands>()) {
					pool.Add(ModContent.NPCType<Defiled_Mimic>(), Defiled_Wastelands.SpawnRates.Mimic);
					pool.Add(ModContent.NPCType<Enchanted_Trident>(), Defiled_Wastelands.SpawnRates.Bident);
				}
				if (player.InModBiome<Riven_Hive>()) {
					pool.Add(ModContent.NPCType<Riven_Mimic>(), Riven_Hive.SpawnRates.Mimic);
					pool.Add(ModContent.NPCType<Savage_Whip>(), Riven_Hive.SpawnRates.Whip);
				}
			}
		}
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			float spawnRateMultiplier = 1f;
			float maxSpawnsMultiplier = 1f;
			if (originPlayer.necroSet) {
				spawnRateMultiplier *= 0.5f;
				maxSpawnsMultiplier *= 2f;
			}
			if (player.InModBiome<Defiled_Wastelands>() || player.InModBiome<Riven_Hive>()) {
				spawnRateMultiplier *= 0.65f;
				maxSpawnsMultiplier *= 1.3f;
			}
			spawnRate = (int)(spawnRate * spawnRateMultiplier);
			maxSpawns = (int)(maxSpawns * maxSpawnsMultiplier);
			if (originPlayer.rapidSpawnFrames > 0 || originPlayer.swarmStatue) {
				spawnRate = 1;
			}
		}
		public static Condition PeatSoldCondition(int amount) {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.PeatSoldCondition").WithFormatArgs(amount),
				() => ModContent.GetInstance<OriginSystem>().peatSold >= amount
			);
		}
		public static Condition WorldEvilBossCondition<TEvil>(string key) where TEvil : AltBiome {
			Condition evil = ShopConditions.GetWorldEvilCondition<TEvil>();
			return new Condition(
				Language.GetOrRegister(key),
				() => NPC.downedBoss2 && evil.IsMet()
			);
		}
	}
}
