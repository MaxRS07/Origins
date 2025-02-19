﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Materials;
using Origins.Items.Other.LootBags;
using Origins.Items.Pets;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.LootConditions;
using Origins.Projectiles.Enemies;
using Origins.Tiles.BossDrops;
using Origins.Tiles.Defiled;
using Origins.Walls;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Tyfyter.Utils;

namespace Origins.NPCs.Defiled.Boss {
	[AutoloadBossHead]
	public class Defiled_Amalgamation : Glowing_Mod_NPC, IDefiledEnemy, ICustomWikiStat {
		static AutoLoadingAsset<Texture2D> RightArmTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm";
		static AutoLoadingAsset<Texture2D> RightArmGlowTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm_Glow";
		static AutoLoadingAsset<Texture2D> LeftArmTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm";
		static AutoLoadingAsset<Texture2D> LeftArmGlowTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm_Glow";
		public string CustomSpritePath => "DefiledAmalg";
		public AssimilationAmount? Assimilation => 0.06f;
		public static bool spawnDA = false;
		float rightArmRot = 0.25f;
		float leftArmRot = 0.25f;
		float time = 0;
		int trappedTime = 0;
		int roars = 0;
		int armFrame = 0;
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		public static int TripleDashCD {
			get {
				int inactiveTime = 455 - DifficultyMult * 35;
				if (DifficultyMult == 3) {
					inactiveTime += 30;
				}
				return inactiveTime;
			}
		}
		//public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
		//bool attacking = false;
		internal static IItemDropRule normalDropRule;
		public override void Unload() {
			normalDropRule = null;
		}
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Rasterized_Debuff>()] = true;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Scale = 0.75f,
				PortraitScale = 1f,
			};
			ID = Type;
			Origins.NPCOnlyTargetInBiome.Add(Type, ModContent.GetInstance<Defiled_Wastelands>());
			Origins.RasterizeAdjustment[Type] = (16, 0f, 0f);
		}
		public override void SetDefaults() {
			NPC.boss = true;
			NPC.BossBar = ModContent.GetInstance<Boss_Bar_DA>();
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 3100;
			NPC.defense = 14;
			NPC.damage = 65;
			NPC.width = 81;
			NPC.height = 96;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0f, 0.25f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(-1f, -0.75f);
			NPC.noGravity = true;
			NPC.npcSlots = 200;
			Music = Origins.Music.DefiledBoss;
			NPC.knockBackResist = 0; // actually a multiplier
			NPC.value = Item.sellPrice(gold: 1);
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
		}
		public bool ForceSyncMana => false;
		public float Mana { get => 1; set { } }
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			float terriblyPlacedHookMult = 1;
			if (Main.GameModeInfo.IsJourneyMode) {
				CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
				if (power != null && power.GetIsUnlocked()) {
					if (power.StrengthMultiplierToGiveNPCs > 2) {
						terriblyPlacedHookMult /= 3;
					} else if (power.StrengthMultiplierToGiveNPCs > 1) {
						terriblyPlacedHookMult /= 2;
					}
				}
			}
			switch (DifficultyMult) {
				case 2:
				NPC.lifeMax = (int)(4960 * balance * terriblyPlacedHookMult);
				// NPC.defense = 13;
				NPC.damage = (int)(76 * terriblyPlacedHookMult);
				break;

				case 3:
				NPC.lifeMax = (int)(7936 * balance * terriblyPlacedHookMult);
				// NPC.defense = 15;
				NPC.damage = (int)(104 * terriblyPlacedHookMult);
				break;
			}
		}

		public override void OnSpawn(IEntitySource source) {
			spawnDA = false;
			if (Main.netMode == NetmodeID.Server) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", NPC.GetTypeNetName()), new Color(222, 222, 222));
			} else {
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(Language.GetTextValue("Announcement.HasAwoken", NPC.TypeName), 222, 222, 222);
				}
				SoundEngine.PlaySound(
					new SoundStyle("Origins/Sounds/Custom/Defiled_Kill1") {
						Pitch = -1,
						Volume = 0.66f
					}, NPC.Center
				);
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			normalDropRule = new LeadingSuccessRule();

			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Ore_Item>(), 1, 140, 330));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Undead_Chunk>(), 1, 40, 100));
			normalDropRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<Low_Signal>(), ModContent.ItemType<Return_To_Sender>()));

			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Defiled_Amalgamation_Trophy_Item>(), 10));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Defiled_Amalgamation_Mask>(), 10));

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Defiled_Amalgamation_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Mysterious_Spray>(), 4));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Defiled_Amalgamation_Relic_Item>()));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Blockus_Tube>(), 4));
		}
		const int state_single_dash = 1;
		const int state_projectiles = 2;
		const int state_triple_dash = 3;
		const int state_sidestep_dash = 4;
		const int state_summon_roar = 5;
		const int state_ground_spikes = 6;
		int AIState { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
		public override void AI() {
			if (Main.rand.NextBool(650)) SoundEngine.PlaySound(Origins.Sounds.Amalgamation, NPC.Center);
			NPC.target = Main.maxPlayers;
			NPC.TargetClosest();
			if (NPC.HasValidTarget) {
				float leftArmTarget = 0.5f;
				float rightArmTarget = 0.25f;
				float armSpeed = 0.03f;

				int difficultyMult = DifficultyMult;// just saving the value as a slight optimization 

				int tickCount = 10 - difficultyMult * 2;
				int tickSize = NPC.lifeMax / tickCount;
				float currentTick = NPC.life / tickSize;

				switch (AIState) {
					//default state, uses default case so that negative values can be used for which action was taken last
					default: {
						CheckTrappedCollision();
						float targetHeight = 96 + (float)(Math.Sin(++time * (0.04f + (0.01f * difficultyMult))) + 0.5f) * 32 * difficultyMult;
						float targetX = 256 + (float)Math.Sin(++time * (0.02f + (0.01f * difficultyMult))) * 48 * difficultyMult;
						float speed = 5;
						float accelerationMult = 1f;

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						float dist = NPC.DistanceSQ(NPC.targetRect.Center());
						if (dist > 640 * 640) {
							accelerationMult = 1 + ((dist / (640 * 640)) - 1) * 3;
							speed *= accelerationMult;
						}
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), (Math.Abs(NPC.velocity.Y) > 16 ? 4 : 0.4f) * accelerationMult);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), (Math.Abs(NPC.velocity.X) > 16 ? 4 : 0.4f) * accelerationMult);

						if (AIState <= 0) {
							NPC.ai[1] += 0.75f + (0.25f * difficultyMult);
							NPC.ai[1] += 0.5f * (difficultyMult - 1) * (1f - (currentTick / tickCount));
							if (NPC.ai[1] > 300) {
								WeightedRandom<int> rand = new(
									Main.rand,
									[
									new(0, 0f),
									new(state_single_dash, 0.9f),
									new(state_projectiles, 1f),
									new(state_triple_dash, 0.35f),
									new(state_sidestep_dash, 0.45f + (0.05f * difficultyMult)),
									new(state_summon_roar, 0f),
									new(state_ground_spikes, 1f),
									]
								);
								int lastUsedAttack = -AIState;

								if (lastUsedAttack > 0) {
									rand.elements[lastUsedAttack] = new(rand.elements[lastUsedAttack].Item1, rand.elements[lastUsedAttack].Item2 / 3f);
									if (Main.masterMode && lastUsedAttack == state_triple_dash) {
										rand.elements[state_single_dash] = new(rand.elements[state_single_dash].Item1, 0);
										rand.elements[state_triple_dash] = new(rand.elements[state_triple_dash].Item1, 0);
									}
								}

								if (!Collision.CanHitLine(NPC.targetRect.TopLeft(), NPC.targetRect.Width, NPC.targetRect.Height, NPC.Center, 16, 16)) {
									rand.elements[0] = new(rand.elements[0].Item1, rand.elements[0].Item2 / 3f);
									rand.elements[1] = new(rand.elements[1].Item1, rand.elements[1].Item2 * 6f);
									rand.elements[2] = new(rand.elements[2].Item1, rand.elements[2].Item2 / 3f);
								}

								AIState = rand.Get();
								NPC.ai[2] = NPC.targetRect.Center().X;
								NPC.ai[3] = NPC.targetRect.Center().Y;
								NPC.ai[1] = 0;

								int roarCount = difficultyMult;
								int roarHP = NPC.lifeMax / (roarCount + 1);

								if (roarCount - roars > NPC.life / roarHP) {
									AIState = state_summon_roar;
									roars++;
								}

								if (AIState == state_single_dash) {
									SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.9f), NPC.Center);
								}
								if (AIState == state_sidestep_dash) {
									SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.5f), NPC.Center);
								}
							}
						}
					}
					break;

					//single dash
					case state_single_dash: {
						NPC.ai[1]++;
						NPC.velocity = NPC.oldVelocity;
						if (NPC.ai[1] < 25) {
							float speed = 6;
							OriginExtensions.LinearSmoothing(ref NPC.velocity, (NPC.Center - new Vector2(NPC.ai[2], NPC.ai[3])).WithMaxLength(speed), 1.8f);
							NPC.oldVelocity = NPC.velocity;
						} else if (NPC.ai[1] < 35) {
							float speed = 12 + difficultyMult * 2f;
							OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 3f);
							NPC.oldVelocity = NPC.velocity;
						} else if (NPC.ai[1] > 80) {
							AIState = -state_single_dash;
							NPC.ai[1] = 0;
						}
					}
					break;

					//projectile spray
					case state_projectiles: {
						CheckTrappedCollision();
						NPC.ai[1] += Main.rand.NextFloat(0.9f, 1f);
						float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
						float targetX = 320 + (float)Math.Sin(++time * 0.01f) * 32;
						float speed = 3;

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), 0.4f);
						leftArmTarget = -0.75f;
						rightArmTarget = -0.75f;
						armSpeed = 0.1f;

						switch ((int)NPC.ai[1]) {
							case 10:
							case 15:
							case 20:
							case 60:
							case 70:
							SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f), NPC.Center);
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								float realDifficultyMult = Math.Min(ContentExtensions.DifficultyDamageMultiplier, 3.666f);
								Projectile.NewProjectileDirect(
									NPC.GetSource_FromAI(),
									NPC.Center,
									Vector2.Normalize(NPC.targetRect.Center() - NPC.Center).RotatedByRandom(0.15f) * (10 + difficultyMult * 2) * Main.rand.NextFloat(0.9f, 1.1f),
									ModContent.ProjectileType<Low_Signal_Hostile>(),
									(int)((24 - (realDifficultyMult * 3)) * realDifficultyMult), // for some reason NPC projectile damage is just arbitrarily doubled
									0f,
									Main.myPlayer
								).tileCollide = Collision.CanHitLine(NPC.targetRect.TopLeft(), NPC.targetRect.Width, NPC.targetRect.Height, NPC.Center, 8, 8);
							}
							break;
							case 12:
							case 17:
							case 65:
							if (difficultyMult > 1) {
								goto case 10;
							}
							break;
							default:
							if (NPC.ai[1] > 100) {
								AIState = -state_projectiles;
								NPC.ai[1] = 0;
							}
							break;
						}
					}
					break;

					//triple dash and downtime after
					case state_triple_dash: {
						NPC.ai[1]++;
						int cycleLength = 100 - (difficultyMult * 4);
						int dashLength = 60 - (difficultyMult * 2);
						int activeLength = cycleLength * 2 + dashLength;
						if (NPC.ai[1] < activeLength) {
							if (NPC.ai[1] % cycleLength is < 2 and >= 1) {
								SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1), NPC.Center);
							}
							NPC.velocity = NPC.oldVelocity;
							if (NPC.ai[1] % cycleLength < 30 - (difficultyMult * 3)) {
								float speed = 8;
								OriginExtensions.LinearSmoothing(ref NPC.velocity, (NPC.Center - new Vector2(NPC.ai[2], NPC.ai[3])).WithMaxLength(speed), 1.8f);
								NPC.oldVelocity = NPC.velocity;
							} else if (NPC.ai[1] % cycleLength < 40 - (difficultyMult * 2)) {
								float speed = 13 + (3 * difficultyMult);
								OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 3f);
								NPC.oldVelocity = NPC.velocity;
							} else if (NPC.ai[1] % cycleLength > dashLength || NPC.collideX || NPC.collideY) {
								NPC.ai[2] = NPC.targetRect.Center().X;
								NPC.ai[3] = NPC.targetRect.Center().Y;
								goto default;
							}
						} else {
							NPC.velocity.X *= 0.97f;
							if (NPC.velocity.Y < 0) NPC.velocity.Y *= 0.97f;
							//SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitch(-1.2f).WithVolume(0.05f), NPC.Center);
							NPC.collideX = NPC.collideY = false;
							CheckTrappedCollision();
							if (NPC.ai[1] - activeLength < TripleDashCD) {
								NPC.velocity.Y += 0.12f;
								leftArmTarget = 0;
								rightArmTarget = 0;
								armSpeed *= 3;
							} else {
								AIState = -state_triple_dash;
								NPC.ai[1] = 100 * difficultyMult;
							}
						}
					}
					break;

					//"sidestep" dash
					case state_sidestep_dash: {
						if ((int)NPC.ai[1] == 0) {
							NPC.ai[2] = NPC.targetRect.Center().X - Math.Sign(NPC.Center.X - NPC.targetRect.Center().X) * 288;
							NPC.ai[3] = NPC.targetRect.Center().Y - 128;
						}
						NPC.ai[1]++;
						float targetHeight = (float)(Math.Sin(++time * 0.05f) + 0.5f) * 32;
						float targetX = (float)Math.Sin(++time * 0.03f) * 48;
						float speed = 5 * difficultyMult;

						float diffX = NPC.Center.X - NPC.ai[2];
						float diffY = NPC.Bottom.Y - (NPC.ai[3] - targetHeight);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed * 4, speed * 4), 2.4f);
						if (Math.Abs(diffX) < 64 || NPC.ai[1] > 25) {
							AIState = -state_sidestep_dash;
							NPC.ai[1] = 160 + (difficultyMult * 40);
						}
						NPC.noTileCollide = true;
					}
					break;

					//"beckoning roar"
					case state_summon_roar: {
						Main.instance.CameraModifiers.Add(new CameraShakeModifier(
							NPC.Center, 10f, 16f, 60, 1200f, 1f, nameof(Defiled_Amalgamation)
						));
						NPC.ai[1]++;
						NPC.velocity *= 0.9f;
						SoundEngine.PlaySound(Origins.Sounds.BeckoningRoar.WithPitchRange(0.1f, 0.2f).WithVolumeScale(0.25f), NPC.Center, (sound) => {
							sound.Position = NPC.Center;
							return true;
						});
						if (NPC.ai[1] < 40) {
							leftArmTarget = 0;
							rightArmTarget = 0;
							armSpeed *= 0.5f;
						} else if (NPC.ai[1] > 60) {
							AIState = 0;
							NPC.ai[1] = -40 + (difficultyMult * 20);
						} else if (NPC.ai[1] >= 45) {
							NPC.velocity = new Vector2(0, -4);
							if ((int)NPC.ai[1] == 45) {
								if (Main.netMode != NetmodeID.MultiplayerClient) {
									for (int i = 3 + (difficultyMult * NPC.statsAreScaledForThisManyPlayers); i-- > 0;) {
										Projectile.NewProjectileDirect(
											NPC.GetSource_FromAI(),
											NPC.targetRect.Center() - new Vector2(Main.rand.Next(80, 640) * (Main.rand.Next(2) * 2 - 1), 640),
											new Vector2(0, 8),
											ModContent.ProjectileType<Defiled_Enemy_Summon>(),
											0,
											0f,
											Main.myPlayer
										);
									}
								}
							}
							leftArmTarget = -1.25f;
							rightArmTarget = -1.25f;
							armSpeed *= 5f;
						}
					}
					break;

					//ground spikes
					case state_ground_spikes: {
						Main.instance.CameraModifiers.Add(new CameraShakeModifier(
							NPC.Center, 10f, 16f, 60, 1200f, 1f, nameof(Defiled_Amalgamation)
						));
						CheckTrappedCollision();
						NPC.ai[1]++;
						float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
						float targetX = 0;// 320 + (float)Math.Sin(++time * 0.01f) * 32;
						float speed = 2;
						float acceleration = 0.4f;
						targetHeight += NPC.ai[1] * 2;
						if (NPC.ai[1] >= 60) {
							if ((int)NPC.ai[1] == 60) {
								NPC.velocity.Y += 8;
								SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(-1f, -0.8f), NPC.Center);
								SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1), NPC.Center);
							}
							speed = 16 + 8 * ContentExtensions.DifficultyDamageMultiplier;
							targetHeight = (NPC.Bottom.Y + 24) - NPC.targetRect.Center().Y;
							if (NPC.collideY || NPC.ai[1] > 120) {
								if (Main.netMode != NetmodeID.MultiplayerClient) {
									float realDifficultyMult = Math.Min(ContentExtensions.DifficultyDamageMultiplier, 3.666f);
									int count = Main.rand.Next(6, 8) + Main.rand.RandomRound(realDifficultyMult * 2);
									for (int i = count; i-- > 0;) {
										SoundEngine.PlaySound(SoundID.Item62.WithPitch(2f), NPC.Center);
										Projectile.NewProjectileDirect(
											NPC.GetSource_FromAI(),
											NPC.targetRect.Center() - new Vector2((i - count * 0.5f) * (56 - realDifficultyMult * 8 + 34 + 24), 640),
											new Vector2(0, 8),
											ModContent.ProjectileType<DA_Spike_Summon>(),
											0,
											0f,
											Main.myPlayer,
											ai2: NPC.targetRect.Center().Y
										);
									}
								}
								NPC.velocity = Vector2.Zero;
								AIState = -AIState;
							}
						}

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), acceleration);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), acceleration);
						/* April Fools' DAb
						leftArmTarget = 0.15f;
						rightArmTarget = -0.15f;
						*/
						leftArmTarget = 0.6f;
						rightArmTarget = 0.7f;
						armSpeed = 0.2f;
					}
					break;
				}
				OriginExtensions.AngularSmoothing(ref rightArmRot, rightArmTarget, armSpeed);
				OriginExtensions.AngularSmoothing(ref leftArmRot, leftArmTarget, armSpeed * 1.5f);
			} else {
				NPC.EncourageDespawn(300);
				if (++trappedTime > 30) {
					NPC.noTileCollide = true;
				}
				float leftArmTarget = 0f;
				float rightArmTarget = 0f;
				float armSpeed = 0.09f;
				OriginExtensions.AngularSmoothing(ref rightArmRot, rightArmTarget, armSpeed);
				OriginExtensions.AngularSmoothing(ref leftArmRot, leftArmTarget, armSpeed * 1.5f);
			}
			NPC.alpha = NPC.noTileCollide ? 75 : 0;
		}
		public void CheckTrappedCollision() {
			if (NPC.position.Y > Main.UnderworldLayer * 16 && NPC.HasValidTarget) {
				NPC.noTileCollide = false;
				trappedTime = 30;
				return;
			}
			if (!NPC.Hitbox.OverlapsAnyTiles()) {
				NPC.noTileCollide = false;
			} else if (AIState == state_triple_dash) {
				NPC.velocity.Y = -4;
			}
			if (NPC.collideX || NPC.collideY) {
				if (++trappedTime > 30) {
					NPC.noTileCollide = true;
					NPC.collideX = NPC.collideY = false;
				}
			} else if (trappedTime > 0) {
				trappedTime--;
			}
		}
		public override bool? CanFallThroughPlatforms() {
			switch (AIState) {
				case state_triple_dash:
				int cycleLength = 100 - (DifficultyMult * 4);
				int dashLength = 60 - (DifficultyMult * 2);
				int activeLength = cycleLength * 2 + dashLength;
				return NPC.ai[1] <= activeLength;

				case state_ground_spikes:
				return NPC.BottomLeft.Y < NPC.targetRect.Center.Y;
			}
			return true;
		}

		public override void FindFrame(int frameHeight) {
			if (!NPC.HasValidTarget && !NPC.IsABestiaryIconDummy) {
				NPC.frame = new Rectangle(0, (frameHeight * 7) % (frameHeight * 8), 122, frameHeight);
				NPC.velocity.Y += 0.12f;
				if (NPC.direction == 0) NPC.direction = 1;
				return;
			}
			int cycleLength = 100 - (DifficultyMult * 4);
			int dashLength = 60 - (DifficultyMult * 2);
			int activeLength = cycleLength * 2 + dashLength;
			if (AIState == state_triple_dash && NPC.ai[1] > activeLength) {
				NPC.frame = new Rectangle(0, (frameHeight * (int)(Math.Pow((NPC.ai[1] - activeLength) / TripleDashCD, 3) * 5) + frameHeight * 7) % (frameHeight * 8), 122, frameHeight);
				armFrame = 3;
			} else if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + frameHeight) % (frameHeight * 3) + frameHeight * 4, 122, frameHeight);
				NPC.frameCounter = 0;
				armFrame = (armFrame + 1) % 3;
			}
		}
		public void Regenerate(out int lifeRegen) {
			lifeRegen = 0;
			if (AIState != state_triple_dash) {
				int tickSize = NPC.lifeMax / (10 - DifficultyMult * 2);
				int threshold = (((NPC.life - 1) / tickSize) + 1) * tickSize;
				if (NPC.life < threshold) {
					lifeRegen = 6 + (DifficultyMult * 2);
				}
			}
		}
		public override void OnKill() {
			if (!NPC.downedBoss2 || Main.rand.NextBool(2)) WorldGen.spawnMeteor = true;
			NPC.SetEventFlagCleared(ref NPC.downedBoss2, GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);
		}
		public void SpawnWisp(NPC npc) {
			for (int releasedWisps = 0; releasedWisps < 5; releasedWisps++) {
				NPC.NewNPC(npc.GetSource_Death(), (int)npc.position.X + Main.rand.Next(npc.width), (int)npc.position.Y + Main.rand.Next(npc.height), ModContent.NPCType<Defiled_Wisp>());
			}
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			switch (AIState) {
				case 2:
				case 3:
				break;
				default: {
					Rectangle highHitbox = NPC.Hitbox;
					highHitbox.Height /= 4;

					Rectangle lowHitbox = NPC.Hitbox;
					lowHitbox.Y += highHitbox.Height;
					lowHitbox.Height -= highHitbox.Height;
					lowHitbox.Width /= 2;
					lowHitbox.X += lowHitbox.Width / 2;

					if (!highHitbox.Intersects(projectile.Hitbox) && !lowHitbox.Intersects(projectile.Hitbox)) {
						modifiers.DefenseEffectiveness *= 1 + DifficultyMult;
					}
				}
				break;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (DifficultyMult >= 2) {
				if (Main.rand.NextBool(2 * DifficultyMult, 9)) {
					target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), (DifficultyMult - 1) * 15);
				}
			}
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_OnHurt(projectile), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_OnHurt(player), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				SpawnGore(NPC.Center + new Vector2(NPC.spriteDirection * -30, -20), 1);
				SpawnGore(NPC.Center + new Vector2(NPC.spriteDirection * 15, 18), 2);
				SpawnGore(NPC.Center + new Vector2(NPC.spriteDirection * -4, -22), 3);
				for (int i = 0; i < 6; i++)
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 10; i++)
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		void SpawnGore(Vector2 position, int num) {
			Gore gore = Main.gore[Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), position, NPC.velocity, $"Gores/NPCs/DA{num}_Gore")];
			gore.Frame = new SpriteFrame(2, 1) {
				CurrentColumn = (NPC.spriteDirection == 1) ? (byte)0 : (byte)1
			};
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			base.PostDraw(spriteBatch, screenPos, drawColor);
			drawColor *= (255 - NPC.alpha) / 255f;
			bool dir = NPC.spriteDirection == 1;
			Rectangle armsFrame = new(0, armFrame * 96, 30, 94);
			spriteBatch.DrawGlowingNPCPart(RightArmTexture, RightArmGlowTexture,
				NPC.Center - new Vector2(-46 * NPC.spriteDirection, 12) * NPC.scale - screenPos,
				armsFrame,
				drawColor, Color.White,
				rightArmRot * NPC.spriteDirection,
				new Vector2(dir ? 7 : 23, 19),
				NPC.scale,
				dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);

			spriteBatch.DrawGlowingNPCPart(LeftArmTexture, LeftArmGlowTexture,
				NPC.Center - new Vector2(36 * NPC.spriteDirection, 0) * NPC.scale - screenPos,
				armsFrame,
				drawColor, Color.White,
				-leftArmRot * NPC.spriteDirection,
				new Vector2(dir ? 23 : 7, 19),
				NPC.scale,
				dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
		}
		public class Spawn : SpawnPool {
			public override string Name => $"{nameof(Defiled_Amalgamation)}_{base.Name}";
			public override void SetStaticDefaults() {
				AddSpawn<Defiled_Amalgamation>(spawnInfo => spawnInfo.PlayerFloorY < Main.worldSurface && Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY].WallType != ModContent.WallType<Defiled_Stone_Wall>() ? 99999999 : 0);
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) => spawnDA && spawnInfo.Player.InModBiome<Defiled_Wastelands>();
		}
	}
	public class Boss_Bar_DA : ModBossBar {
		bool isDead = false;
		float lastTickPercent = 1f;
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return Asset<Texture2D>.Empty;
		}
		AutoLoadingAsset<Texture2D> tickTexture = typeof(Boss_Bar_DA).GetDefaultTMLName() + "_Tick";
		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) {
			NPC owner = Main.npc[info.npcIndexToAimAt];
			if (owner.type != Defiled_Amalgamation.ID || (lastTickPercent < 0 && isDead)) return false;
			if (!owner.active || owner.life <= 0) {
				isDead = true;
				life = 0;
			}
			if (owner.life > 0 && owner.active) {
				isDead = false;
				life = owner.life;
				lifeMax = owner.lifeMax;
				shield = owner.life;
				shieldMax = lifeMax;
			}

			int tickCount = 10 - Defiled_Amalgamation.DifficultyMult * 2;
			float tickSize = lifeMax / tickCount;
			float lifeTarget = MathF.Ceiling((life - 1) / tickSize) / tickCount;
			OriginExtensions.LinearSmoothing(ref lastTickPercent, lifeTarget, 0.015f);
			life = lastTickPercent * lifeMax;
			return life > 0;
		}
		public override void PostDraw(SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams) {
			int tickCount = 10 - Defiled_Amalgamation.DifficultyMult * 2;
			Vector2 barSize = new(456, 22);
			Vector2 barPos = drawParams.BarCenter - barSize * new Vector2(0.5f, 0);
			Vector2 origin = tickTexture.Value.Size() / 2;
			float tickPercent = 1f / tickCount;
			float lifePercentToShow = drawParams.Life / drawParams.LifeMax;
			for (float f = 0; f < lifePercentToShow; f += tickPercent) {
				if (f == 0f) continue;
				float animFactor = Math.Min((lifePercentToShow - f) / tickPercent, 1);
				spriteBatch.Draw(
					tickTexture,
					barPos + barSize * new Vector2(f, 0),
					null,
					new Color(animFactor, animFactor, animFactor, animFactor),
					0f,
					origin,
					2f - animFactor,
					SpriteEffects.None,
				0f);
			}
		}
	}
	public class Low_Signal_Hostile : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Default;
			Projectile.friendly = false;
			if (Main.masterMode || Main.expertMode) {
				Projectile.hostile = true;
			}
			Projectile.timeLeft = 40;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = 1;
			Projectile.hide = true;
		}
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Center, DustID.AncientLight, default, newColor: Color.White, Scale: 0.5f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f);
			if (Projectile.timeLeft % 15 == 0) {
				Projectile.localNPCImmunity.CopyTo(Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Defiled_Spike_Explosion_Hostile>(),
					Projectile.damage,
					0,
					Projectile.owner,
					7,
					ai2: 0.5f
				).localNPCImmunity.AsSpan());
			}
		}
		public override void OnKill(int timeLeft) {
			Projectile.localNPCImmunity.CopyTo(Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Defiled_Spike_Explosion_Hostile>(),
				Projectile.damage,
				0,
				Projectile.owner,
			7).localNPCImmunity.AsSpan());
		}
	}
	public class Defiled_Spike_Explosion_Hostile : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public override void SetDefaults() {
			Projectile.timeLeft = 600;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.hide = true;
			Projectile.rotation = Main.rand.NextFloatDirection();
			Projectile.tileCollide = false;
			Projectile.npcProj = true;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Default;
		}
		public override bool? CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target) => false;
		public override bool CanHitPvp(Player target) => false;
		public override void AI() {
			if (Projectile.ai[2] != 0) Projectile.scale = Projectile.ai[2];
			if (Projectile.ai[0] > 0) {
				Projectile.ai[0]--;
				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					(Vector2)new PolarVec2(Main.rand.NextFloat(8, 16), Projectile.ai[1]++),
					Defiled_Spike_Explosion_Spike_Hostile.ID,
					Projectile.damage,
					0,
					Projectile.owner,
					ai1: Projectile.whoAmI
				);
			}
		}
	}
	public class Defiled_Spike_Explosion_Spike_Hostile : ModProjectile {
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public static int ID { get; private set; }
		Vector2 realPosition;
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.timeLeft = Main.rand.Next(22, 25);
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 0;
			Projectile.hide = true;
			Projectile.npcProj = false;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Magic;
		}
		public Projectile ParentProjectile => Main.projectile[(int)Projectile.ai[1]];
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
			Projectile.scale = ParentProjectile.scale;
			Projectile.Center = ParentProjectile.Center - Projectile.velocity;
			if (movementFactor == 0f) {
				movementFactor = 1f;
				//if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft > 18) {
				movementFactor += 1f;
			}
			Projectile.position += Projectile.velocity * movementFactor * Projectile.scale;
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.rotation += MathHelper.PiOver2;
			ParentProjectile.timeLeft = 7;
		}
		public override bool? CanHitNPC(NPC target) {
			if (ParentProjectile.localNPCImmunity[target.whoAmI] == 0) {
				return null;
			}
			return false;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParentProjectile.localNPCImmunity[target.whoAmI] = -1;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (DifficultyMult >= 2) {
				if (Main.rand.NextBool(2 * DifficultyMult, 9)) {
					target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), (DifficultyMult - 1) * 15);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			float totalLength = Projectile.velocity.Length() * movementFactor;
			int avg = (lightColor.R + lightColor.G + lightColor.B) / 3;
			lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
			totalLength -= 58;
			Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 58 * Projectile.scale;
			Texture2D texture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Dismay_Mid").Value;
			int c = 0;
			Vector2 pos;
			for (int i = (int)totalLength; i > 0; i -= 58) {
				c++;
				pos = (Projectile.Center - Main.screenPosition) - (offset * c);
				//lightColor = Projectile.GetAlpha(new Color(Lighting.GetColor((pos + Projectile.velocity * 2).ToTileCoordinates()).ToVector4()));
				Main.EntitySpriteDraw(texture, pos, new Rectangle(0, 0, 18, Math.Min(58, i)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
			}
			return false;
		}
	}
}
