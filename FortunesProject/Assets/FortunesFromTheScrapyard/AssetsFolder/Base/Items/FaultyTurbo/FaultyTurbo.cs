﻿using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Items
{
    public class FaultyTurbo : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_FAULTYTURBO_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float movespeedBonus = 0.8f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static float baseDuration = 5f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 2)]
        public static float baseDurationStack = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 3)]
        public static float checkInterval = 1f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float baseChance = 15f;
        [FormatToken(TOKEN, 5)]
        public static float chancePerStack = 15f;

        public static BuffDef speedBuff;
        public override void Initialize()
        {
            speedBuff = AssetCollection.FindAsset<BuffDef>("bdFaultyTurbo");
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acFaultyTurbo", ScrapyardBundle.Items);
        }

        public class FaultyTurboBehaviour : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.FaultyTurbo;

            private float timer = 0f;

            private float pity;

            public void ModifyStatArguments(StatHookEventArgs args)
            {
                if (body.HasBuff(speedBuff))
                {
                    args.moveSpeedMultAdd += movespeedBonus;
                }
            }

            private void FixedUpdate()
            {
                if (base.body.isSprinting) timer += Time.fixedDeltaTime;
                else timer = 0f;

                if (timer >= checkInterval)
                {
                    timer = 0f;
                    if (Util.CheckRoll(GetStackValue(baseChance, chancePerStack, stack) + Util.ConvertAmplificationPercentageIntoReductionPercentage(baseChance), body.master))
                    {
                        pity = 0;

                        if(NetworkServer.active)
                        {
                            if (body.HasBuff(speedBuff)) body.RemoveOldestTimedBuff(speedBuff);
                            body.AddTimedBuff(speedBuff, GetStackValue(baseDuration, baseDurationStack, stack));
                            Util.PlaySound("sfx_turbo_start", body.gameObject);
                        }
                    }
                    else
                    {
                        pity += 0.5f;
                    }
                }
            }

            private void OnDisable()
            {
                if(NetworkServer.active)
                {
                    if(body.HasBuff(speedBuff))
                    {
                        body.RemoveBuff(speedBuff);
                    }
                }
            }
        }
    }
}
