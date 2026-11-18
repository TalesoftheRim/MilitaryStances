using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MilitaryStances
{
    [StaticConstructorOnStartup]
    public static class PatchesMainStatic
    {
        private static readonly AccessTools.FieldRef<PawnRenderer, Pawn> PawnRendererPawnField =
            AccessTools.FieldRefAccess<PawnRenderer, Pawn>("pawn");

        public struct EquippedAngleOffsetState
        {
            public bool overridden;
            public float originalOffset;
        }

        static PatchesMainStatic()
        {
            Harmony harmony = new(id: "talesoftherim.militarystances");

            // Prone: Disable standing texture atlas cache when Prone is active so PawnRenderTree is always used
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderer), "ParallelGetPreRenderResults"),
                prefix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderer_ParallelGetPreRenderResults_Prefix))
                {
                    priority = Priority.Last
                });

            // Prone: Apply crawling flag and prone body matrix via Postfix (Priority.Last)
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderer), "GetDrawParms"),
                postfix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderer_GetDrawParms_Postfix))
                {
                    priority = Priority.Last
                });

            // Prone: Apply crawling head rotation via Postfix (Priority.Last) since vanilla checks pawn.Crawling
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderNodeWorker_Head), nameof(PawnRenderNodeWorker_Head.RotationFor)),
                postfix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderNodeWorker_Head_RotationFor_Postfix))
                {
                    priority = Priority.Last
                });

            // Prone: Restore South facing for carried weapon when AdjustParms flips body facing to North for South-facing prone pawns
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderNodeWorker_Carried), nameof(PawnRenderNodeWorker_Carried.OffsetFor)),
                prefix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderNodeWorker_Carried_OffsetFor_Prefix))
                {
                    priority = Priority.Last
                });

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderNodeWorker_Carried), nameof(PawnRenderNodeWorker_Carried.PostDraw)),
                prefix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderNodeWorker_Carried_PostDraw_Prefix))
                {
                    priority = Priority.Last
                });

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.LayerFor)),
                postfix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderNodeWorker_LayerFor_Postfix))
                {
                    priority = Priority.Last
                });

            // Prone Move Speed: Optional crawl speed penalty via Postfix (Priority.Last) on StatExtension.GetStatValue
            harmony.Patch(
                original: AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue),
                    [typeof(Thing), typeof(StatDef), typeof(bool), typeof(int)]),
                postfix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(StatExtension_GetStatValue_Postfix))
                {
                    priority = Priority.Last
                });

            // Weapon Ready: Track carried weapon facing and apply diagonal ready angle with Priority.Last
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawCarriedWeapon)),
                prefix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderUtility_DrawCarriedWeapon_Prefix))
                {
                    priority = Priority.Last
                },
                postfix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderUtility_DrawCarriedWeapon_Postfix))
                {
                    priority = Priority.First
                });

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming)),
                prefix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderUtility_DrawEquipmentAiming_Prefix))
                {
                    priority = Priority.Last
                },
                postfix: new HarmonyMethod(typeof(PatchesMainStatic), nameof(PawnRenderUtility_DrawEquipmentAiming_Postfix))
                {
                    priority = Priority.First
                });
        }

        public static void PawnRenderer_ParallelGetPreRenderResults_Prefix(PawnRenderer __instance, ref bool disableCache)
        {
            if (!SettingsMain.Activated || disableCache)
            {
                return;
            }

            Pawn pawn = PawnRendererPawnField(__instance);
            if (PatchesUtil.IsProneActive(pawn))
            {
                disableCache = true;
            }
        }

        public static void PawnRenderer_GetDrawParms_Postfix(
            Vector3 rootLoc,
            ref PawnDrawParms __result)
        {
            if (!SettingsMain.Activated || __result.Portrait || __result.Statue || __result.crawling)
            {
                return;
            }

            Pawn pawn = __result.pawn;
            if (!PatchesUtil.IsProneActive(pawn))
            {
                return;
            }

            __result.crawling = true;
            float proneAngle = PawnRenderUtility.CrawlingBodyAngle(__result.facing);
            Vector3 bodyDrawOffset = pawn.ageTracker?.CurLifeStage?.bodyDrawOffset ?? Vector3.zero;
            __result.matrix = Matrix4x4.TRS(
                rootLoc + bodyDrawOffset,
                Quaternion.AngleAxis(proneAngle, Vector3.up),
                Vector3.one);
        }

        public static void PawnRenderNodeWorker_Head_RotationFor_Postfix(
            PawnDrawParms parms,
            ref Quaternion __result)
        {
            if (!SettingsMain.Activated || parms.Portrait || parms.Statue)
            {
                return;
            }

            Pawn pawn = parms.pawn;
            if (pawn == null || pawn.Crawling || !PatchesUtil.IsProneActive(pawn))
            {
                return;
            }

            __result *= PawnRenderUtility.CrawlingHeadAngle(parms.facing).ToQuat();
            if (parms.flipHead)
            {
                __result *= 180f.ToQuat();
            }
        }

        public static void PawnRenderNodeWorker_Carried_OffsetFor_Prefix(ref PawnDrawParms parms)
        {
            if (!SettingsMain.Activated)
            {
                return;
            }

            if (parms.flipHead && PatchesUtil.IsProneActive(parms.pawn))
            {
                parms.facing = parms.facing.Opposite;
            }
        }

        public static void PawnRenderNodeWorker_Carried_PostDraw_Prefix(ref PawnDrawParms parms)
        {
            if (!SettingsMain.Activated)
            {
                return;
            }

            if (parms.flipHead && PatchesUtil.IsProneActive(parms.pawn))
            {
                parms.facing = parms.facing.Opposite;
            }
        }

        public static void PawnRenderNodeWorker_LayerFor_Postfix(
            PawnRenderNodeWorker __instance,
            PawnDrawParms parms,
            ref float __result)
        {
            if (!SettingsMain.Activated || !SettingsMain.ProneWeaponBehindPawn || parms.Portrait || parms.Statue)
            {
                return;
            }

            if (__instance is not PawnRenderNodeWorker_Carried || parms.carriedThing != null)
            {
                return;
            }

            Pawn pawn = parms.pawn;
            if (pawn?.carryTracker?.CarriedThing != null)
            {
                return;
            }

            if (PatchesUtil.IsProneActive(pawn))
            {
                __result = PawnRenderUtility.Layer_Carried_Behind;
            }
        }

        public static void StatExtension_GetStatValue_Postfix(
            Thing thing,
            StatDef stat,
            bool applyPostProcess,
            int cacheStaleAfterTicks,
            ref float __result)
        {
            if (stat != StatDefOf.MoveSpeed || thing is not Pawn pawn)
            {
                return;
            }

            if (PatchesUtil.TryGetProneMoveSpeed(pawn, applyPostProcess, cacheStaleAfterTicks, out float crawlSpeed))
            {
                __result = crawlSpeed;
            }
        }

        public static void PawnRenderUtility_DrawCarriedWeapon_Prefix(Rot4 facing)
        {
            PatchesUtil.CurrentCarriedWeaponFacing = facing;
        }

        public static void PawnRenderUtility_DrawCarriedWeapon_Postfix()
        {
            PatchesUtil.CurrentCarriedWeaponFacing = null;
        }

        public static void PawnRenderUtility_DrawEquipmentAiming_Prefix(
            Thing eq,
            ref Vector3 drawLoc,
            ref float aimAngle,
            out EquippedAngleOffsetState __state)
        {
            __state = default;
            if (!SettingsMain.Activated || eq == null)
            {
                return;
            }

            Pawn pawn = (eq.ParentHolder as Pawn_EquipmentTracker)?.pawn;
            if (pawn != null)
            {
                drawLoc += PatchesUtil.GetProneWeaponDrawOffset(pawn, eq, aimAngle);
            }

            if (!PatchesUtil.IsWeaponReadyActive(pawn, eq, out CompMilitaryStances comp))
            {
                return;
            }

            Rot4 facing = PatchesUtil.CurrentCarriedWeaponFacing ?? pawn.Rotation;
            aimAngle = PatchesUtil.GetWeaponReadyAimAngle(facing, comp.currentWeaponReadyStance);

            if (SettingsMain.IgnoreEquippedAngleOffset && eq.def != null && eq.def.equippedAngleOffset != 0f)
            {
                __state.overridden = true;
                __state.originalOffset = eq.def.equippedAngleOffset;
                eq.def.equippedAngleOffset = 0f;
            }
        }

        public static void PawnRenderUtility_DrawEquipmentAiming_Postfix(
            Thing eq,
            EquippedAngleOffsetState __state)
        {
            if (__state.overridden && eq?.def != null)
            {
                eq.def.equippedAngleOffset = __state.originalOffset;
            }
        }
    }
}
