using System;
using UnityEngine;
using Verse;

namespace MilitaryStances
{
    public class MilitaryStancesMod : Mod
    {
        private readonly SettingsMain _settingsMain;
        private float _halfWidth;
        private float _leftScrollHeight;
        private Vector2 _leftScrollPos = Vector2.zero;

        public override string SettingsCategory() => "MilitaryStances.SettingCategory".Translate();

        public MilitaryStancesMod(ModContentPack pack) : base(pack)
        {
            _settingsMain = GetSettings<SettingsMain>();

            Log.Message("[Military Stances] Assembly loaded, ready for action!");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            _halfWidth = (inRect.width - 30f) / 1.2f;
            LeftSideScrollViewHandler(new Rect(inRect.x, inRect.y, _halfWidth, inRect.height));
        }

        private void LeftSideScrollViewHandler(Rect inRect)
        {
            Listing_Standard listLeft = new();
            Rect viewRectLeft = new(inRect.x, inRect.y, inRect.width, inRect.height);
            float viewHeight = Mathf.Max(inRect.height, _leftScrollHeight + 20f);
            Rect vROffsetLeft = new(0f, 0f, inRect.width - 20f, viewHeight);

            bool prevActivated = _settingsMain.activated;
            bool prevProneSpeed = _settingsMain.proneAffectsMoveSpeed;
            bool prevProneWeaponBehindPawn = _settingsMain.proneWeaponBehindPawn;
            bool prevProneWeaponOffset = _settingsMain.proneWeaponOffset;
            float prevSideOffset = _settingsMain.proneWeaponSideOffset;
            float prevForwardOffset = _settingsMain.proneWeaponForwardOffset;
            float prevStanceOffset = _settingsMain.proneWeaponStanceVerticalOffset;
            bool prevMelee = _settingsMain.weaponReadyIncludeMelee;
            bool prevIgnoreOffset = _settingsMain.ignoreEquippedAngleOffset;

            Widgets.BeginScrollView(viewRectLeft, ref _leftScrollPos, vROffsetLeft);
            listLeft.Begin(vROffsetLeft);
            listLeft.Gap();

            listLeft.CheckboxLabeled(
                "MilitaryStances.Settings.Activated".Translate(),
                ref _settingsMain.activated,
                "MilitaryStances.Settings.ActivatedDesc".Translate());

            if (_settingsMain.activated)
            {
                listLeft.Gap();

                listLeft.CheckboxLabeled(
                    "MilitaryStances.Settings.ShowButtons".Translate(),
                    ref _settingsMain.showButtons,
                    "MilitaryStances.Settings.ShowButtonsDesc".Translate());

                listLeft.Gap();

                listLeft.CheckboxLabeled(
                    "MilitaryStances.Settings.ProneAffectsMoveSpeed".Translate(),
                    ref _settingsMain.proneAffectsMoveSpeed,
                    "MilitaryStances.Settings.ProneAffectsMoveSpeedDesc".Translate());

                listLeft.Gap();

                listLeft.CheckboxLabeled(
                    "MilitaryStances.Settings.ProneWeaponBehindPawn".Translate(),
                    ref _settingsMain.proneWeaponBehindPawn,
                    "MilitaryStances.Settings.ProneWeaponBehindPawnDesc".Translate());

                listLeft.Gap();

                listLeft.CheckboxLabeled(
                    "MilitaryStances.Settings.ProneWeaponOffset".Translate(),
                    ref _settingsMain.proneWeaponOffset,
                    "MilitaryStances.Settings.ProneWeaponOffsetDesc".Translate());

                if (_settingsMain.proneWeaponOffset)
                {
                    listLeft.Gap(4f);
                    _settingsMain.proneWeaponSideOffset = (float)Math.Round(
                        listLeft.SliderLabeled(
                            "MilitaryStances.Settings.ProneWeaponSideOffset".Translate(_settingsMain.proneWeaponSideOffset.ToString("F2")),
                            _settingsMain.proneWeaponSideOffset,
                            0f,
                            0.40f,
                            0.55f,
                            "MilitaryStances.Settings.ProneWeaponSideOffsetDesc".Translate()),
                        2);

                    _settingsMain.proneWeaponForwardOffset = (float)Math.Round(
                        listLeft.SliderLabeled(
                            "MilitaryStances.Settings.ProneWeaponForwardOffset".Translate(_settingsMain.proneWeaponForwardOffset.ToString("F2")),
                            _settingsMain.proneWeaponForwardOffset,
                            -0.20f,
                            0.40f,
                            0.55f,
                            "MilitaryStances.Settings.ProneWeaponForwardOffsetDesc".Translate()),
                        2);

                    _settingsMain.proneWeaponStanceVerticalOffset = (float)Math.Round(
                        listLeft.SliderLabeled(
                            "MilitaryStances.Settings.ProneWeaponStanceVerticalOffset".Translate(_settingsMain.proneWeaponStanceVerticalOffset.ToString("F2")),
                            _settingsMain.proneWeaponStanceVerticalOffset,
                            -0.20f,
                            0.30f,
                            0.55f,
                            "MilitaryStances.Settings.ProneWeaponStanceVerticalOffsetDesc".Translate()),
                        2);

                    if (listLeft.ButtonText("MilitaryStances.Settings.ResetProneWeaponOffsets".Translate(), widthPct: 0.45f))
                    {
                        _settingsMain.ResetProneWeaponOffsets();
                    }
                }

                listLeft.Gap();

                listLeft.CheckboxLabeled(
                    "MilitaryStances.Settings.WeaponReadyIncludeMelee".Translate(),
                    ref _settingsMain.weaponReadyIncludeMelee,
                    "MilitaryStances.Settings.WeaponReadyIncludeMeleeDesc".Translate());

                listLeft.Gap();

                listLeft.CheckboxLabeled(
                    "MilitaryStances.Settings.IgnoreEquippedAngleOffset".Translate(),
                    ref _settingsMain.ignoreEquippedAngleOffset,
                    "MilitaryStances.Settings.IgnoreEquippedAngleOffsetDesc".Translate());
            }

            _leftScrollHeight = listLeft.CurHeight;
            listLeft.End();
            Widgets.EndScrollView();

            if (prevActivated != _settingsMain.activated ||
                prevProneSpeed != _settingsMain.proneAffectsMoveSpeed ||
                prevProneWeaponBehindPawn != _settingsMain.proneWeaponBehindPawn ||
                prevProneWeaponOffset != _settingsMain.proneWeaponOffset ||
                !Mathf.Approximately(prevSideOffset, _settingsMain.proneWeaponSideOffset) ||
                !Mathf.Approximately(prevForwardOffset, _settingsMain.proneWeaponForwardOffset) ||
                !Mathf.Approximately(prevStanceOffset, _settingsMain.proneWeaponStanceVerticalOffset) ||
                prevMelee != _settingsMain.weaponReadyIncludeMelee ||
                prevIgnoreOffset != _settingsMain.ignoreEquippedAngleOffset)
            {
                PatchesUtil.RefreshAllPawnGraphics();
            }
        }
    }
}
