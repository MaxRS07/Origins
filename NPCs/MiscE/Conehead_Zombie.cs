﻿using Microsoft.Xna.Framework;
using Origins.Tiles.Other;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Conehead_Zombie : ModNPC {
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.Tim;
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.lifeMax = 45;
			NPC.defense = 14;
			NPC.damage = 14;
			NPC.width = 28;
			NPC.height = 46;
			NPC.friendly = false;
			AIType = NPCID.Zombie;
			AnimationType = NPCID.Zombie;
			Banner = Item.NPCtoBanner(NPCID.Zombie);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if ((spawnInfo.Player.ZoneGraveyard || !Main.dayTime) && spawnInfo.Player.ZoneForest) {
				return 0.085f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Diamond, 20));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Traffic_Cone_Item>(), 15));
        }
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0 || OriginsModIntegrations.CheckAprilFools()) {
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
			}
		}
	}
}
