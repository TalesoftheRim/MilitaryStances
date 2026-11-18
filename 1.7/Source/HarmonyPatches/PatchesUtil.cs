using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace MilitaryStances
{
    public static class PatchesUtil
    {
        public const float HighReadyAngleRight = 37f;
        public const float HighReadyAngleLeft = 323f;
        public const float LowReadyAngleRight = 118f;
        public const float LowReadyAngleLeft = 242f;

        [ThreadStatic]
        public static Rot4? CurrentCarriedWeaponFacing;

        [ThreadStatic]
        private static bool _isFetchingCrawlSpeed;

        public static bool IsVehiclePawn(Pawn pawn)
        {
            return pawn != null &&
                   ModsConfig.IsActive("SmashPhil.VehicleFramework") &&
                   pawn.GetType().FullName == "Vehicles.VehiclePawn";
        }

        public static bool IsProneActive(Pawn pawn)
        {
            if (!SettingsMain.Activated || pawn == null)
            {
                return false;
            }

            if (pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.CarriedBy != null)
            {
                return false;
            }

            if (pawn.RaceProps?.Humanlike != true || IsVehiclePawn(pawn))
            {
                return false;
            }

            if (pawn.GetPosture() != PawnPosture.Standing)
            {
                return false;
            }

            var comp = pawn.TryGetComp<CompMilitaryStances>();
            return comp is { enableBodyStance: true, currentBodyStance: BodyStance.Prone, CanUseBodyStance: true };
        }

        public static bool IsActivelyAimingAtTarget(Pawn pawn)
        {
            return pawn?.stances?.curStance is Stance_Busy
            {
                neverAimWeapon: false,
                focusTarg.IsValid: true
            };
        }

        public static bool IsWeaponEligible(Thing weapon)
        {
            if (weapon?.def == null || !weapon.def.IsWeapon)
            {
                return false;
            }

            if (!SettingsMain.WeaponReadyIncludeMelee)
            {
                return weapon.def.IsRangedWeapon;
            }

            return true;
        }

        public static bool IsWeaponReadyActive(Pawn pawn, Thing weapon, out CompMilitaryStances comp)
        {
            comp = null;
            if (!SettingsMain.Activated || pawn == null || weapon == null)
            {
                return false;
            }

            if (pawn.Dead || pawn.Downed || !pawn.Spawned || !pawn.Drafted)
            {
                return false;
            }

            if (IsVehiclePawn(pawn) || IsActivelyAimingAtTarget(pawn) || !IsWeaponEligible(weapon))
            {
                return false;
            }

            comp = pawn.TryGetComp<CompMilitaryStances>();
            return comp is { enableWeaponStance: true, currentWeaponReadyStance: not WeaponReadyStance.Normal, CanUseWeaponStance: true };
        }

        public static float GetWeaponReadyAimAngle(Rot4 facing, WeaponReadyStance stance)
        {
            bool isWest = facing == Rot4.West;
            return stance switch
            {
                WeaponReadyStance.High => isWest ? HighReadyAngleLeft : HighReadyAngleRight,
                WeaponReadyStance.Low => isWest ? LowReadyAngleLeft : LowReadyAngleRight,
                _ => 0f
            };
        }

        public static Vector3 GetProneWeaponDrawOffset(Pawn pawn, Thing eq, float aimAngle)
        {
            if (!SettingsMain.Activated || !SettingsMain.ProneWeaponOffset || eq?.def == null || !eq.def.IsWeapon)
            {
                return Vector3.zero;
            }

            if (!IsProneActive(pawn))
            {
                return Vector3.zero;
            }

            bool fromCarriedWeapon = CurrentCarriedWeaponFacing.HasValue;
            Rot4 facing = CurrentCarriedWeaponFacing ?? pawn.Rotation;

            float side = SettingsMain.ProneWeaponSideOffset;
            float forward = SettingsMain.ProneWeaponForwardOffset;
            float stanceVertical = SettingsMain.ProneWeaponStanceVerticalOffset;

            Vector3 offset = Vector3.zero;

            if (!fromCarriedWeapon)
            {
                // Actively aiming while prone: RimWorld centers drawLoc on the aim line through the head/body axis.
                offset += GetProneSideOffset(facing, side);
                offset += new Vector3(0f, 0f, forward).RotatedBy(aimAngle);
            }
            else
            {
                // Carried / Weapon Ready Stance while prone:
                // North/South EqLoc has X = 0 (dead-center); East/West EqLoc already shifts downward (-Z).
                if (facing == Rot4.North || facing == Rot4.South)
                {
                    offset += GetProneSideOffset(facing, side);
                    offset.z += (facing == Rot4.North ? forward : -forward) * 0.5f;
                }
                else
                {
                    offset.x += facing == Rot4.East ? forward * 0.65f : -forward * 0.65f;
                    offset.z += stanceVertical;
                }

                CompMilitaryStances comp = pawn.TryGetComp<CompMilitaryStances>();
                if (comp is { enableWeaponStance: true } &&
                    comp.currentWeaponReadyStance != WeaponReadyStance.Normal &&
                    IsWeaponEligible(eq) &&
                    pawn.Drafted)
                {
                    if (facing == Rot4.East || facing == Rot4.West)
                    {
                        if (comp.currentWeaponReadyStance == WeaponReadyStance.High)
                        {
                            offset.z += 0.05f;
                        }
                        else if (comp.currentWeaponReadyStance == WeaponReadyStance.Low)
                        {
                            offset.z += 0.03f;
                        }
                    }
                    else
                    {
                        offset.z += facing == Rot4.North ? 0.04f : -0.04f;
                    }
                }
            }

            return offset;
        }

        private static Vector3 GetProneSideOffset(Rot4 facing, float side)
        {
            return facing.AsInt switch
            {
                0 => new Vector3(side, 0f, 0f),   // North: shift right onto shoulder/arm side
                1 => new Vector3(0f, 0f, -side),  // East: shift down onto chest/arm side
                2 => new Vector3(-side, 0f, 0f),  // South: shift left onto right-hand side
                3 => new Vector3(0f, 0f, -side),  // West: shift down onto chest/arm side
                _ => Vector3.zero
            };
        }

        public static string GetBodyStanceLabel(BodyStance stance)
        {
            return stance switch
            {
                BodyStance.Stand => "MilitaryStances.BodyStance.Stand".Translate(),
                BodyStance.Prone => "MilitaryStances.BodyStance.Prone".Translate(),
                _ => stance.ToString()
            };
        }

        public static string GetWeaponReadyStanceLabel(WeaponReadyStance stance)
        {
            return stance switch
            {
                WeaponReadyStance.Normal => "MilitaryStances.WeaponReady.Normal".Translate(),
                WeaponReadyStance.High => "MilitaryStances.WeaponReady.High".Translate(),
                WeaponReadyStance.Low => "MilitaryStances.WeaponReady.Low".Translate(),
                _ => stance.ToString()
            };
        }

        public static bool TryGetProneMoveSpeed(Pawn pawn, bool applyPostProcess, int cacheStaleAfterTicks, out float crawlSpeed)
        {
            crawlSpeed = 0f;
            if (_isFetchingCrawlSpeed || !SettingsMain.ProneAffectsMoveSpeed || !IsProneActive(pawn))
            {
                return false;
            }

            try
            {
                _isFetchingCrawlSpeed = true;
                crawlSpeed = pawn.GetStatValue(StatDefOf.CrawlSpeed, applyPostProcess, cacheStaleAfterTicks);
                return true;
            }
            finally
            {
                _isFetchingCrawlSpeed = false;
            }
        }

        public static void RefreshAllPawnGraphics()
        {
            if (Current.ProgramState != ProgramState.Playing || Find.Maps == null)
            {
                return;
            }

            foreach (Map map in Find.Maps)
            {
                if (map?.mapPawns?.AllPawnsSpawned == null)
                {
                    continue;
                }

                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.TryGetComp<CompMilitaryStances>() != null)
                    {
                        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
                    }
                }
            }
        }
    }
}
