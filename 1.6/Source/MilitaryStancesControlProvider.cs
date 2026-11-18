using System;
using System.Collections.Generic;
using TalesUIFramework;
using Verse;

namespace MilitaryStances
{
    [StaticConstructorOnStartup]
    public sealed class MilitaryStancesControlProvider : IPawnControlProvider
    {
        public string ProviderId => "talesoftherim.militarystances";
        public string DisplayName => "Military Stances";
        public int Order => 100;

        static MilitaryStancesControlProvider()
        {
            TalesUIRegistry.RegisterProvider(new MilitaryStancesControlProvider());
        }

        public void CollectControls(Pawn pawn, List<TalesControlRow> rows)
        {
            if (pawn == null || pawn.Faction is not { IsPlayer: true } || !SettingsMain.Activated || !SettingsMain.ShowButtons)
            {
                return;
            }

            CompMilitaryStances comp = pawn.TryGetComp<CompMilitaryStances>();
            if (comp == null)
            {
                return;
            }

            if (comp.CanUseBodyStance && !pawn.Downed)
            {
                rows.Add(CreateBodyStanceRow(comp));
            }

            if (comp.CanUseWeaponStance && pawn.Drafted)
            {
                rows.Add(CreateWeaponStanceRow(comp));
            }
        }

        private static TalesControlRow CreateBodyStanceRow(CompMilitaryStances comp)
        {
            TalesControlOption[] options =
            [
                new(BodyStance.Stand.ToString(), PatchesUtil.GetBodyStanceLabel(BodyStance.Stand), GraphicsUtil.BodyStanceStandIcon),
                new(BodyStance.Prone.ToString(), PatchesUtil.GetBodyStanceLabel(BodyStance.Prone), GraphicsUtil.BodyStanceProneIcon)
            ];

            return new TalesControlRow(
                id: "MilitaryStances.BodyStances",
                order: 100f,
                title: "MilitaryStances.UI.BodyStancesTitle".Translate(),
                toggleIcon: GraphicsUtil.BodyStanceIcon,
                toggleTooltip: "MilitaryStances.UI.ToggleBodyStanceDesc".Translate(),
                isEnabled: () => comp.enableBodyStance,
                setEnabled: value =>
                {
                    if (comp.enableBodyStance != value)
                    {
                        comp.enableBodyStance = value;
                        comp.Refresh();
                    }
                },
                options: options,
                getSelectedOptionId: () => comp.currentBodyStance.ToString(),
                selectOption: id =>
                {
                    if (Enum.TryParse(id, out BodyStance stance) && comp.currentBodyStance != stance)
                    {
                        comp.currentBodyStance = stance;
                        comp.Refresh();
                    }
                },
                selectionTooltip: "MilitaryStances.ChangeBodyStanceDesc".Translate(),
                selectionDisabledTooltip: "MilitaryStances.UI.EnableBodyStancesFirst".Translate(),
                fallbackToggleLabel: "MilitaryStances.ToggleBodyStanceLabel".Translate(comp.enableBodyStance.ToString()),
                fallbackSelectionLabel: () => "MilitaryStances.ChangeBodyStanceLabel".Translate(PatchesUtil.GetBodyStanceLabel(comp.currentBodyStance)));
        }

        private static TalesControlRow CreateWeaponStanceRow(CompMilitaryStances comp)
        {
            TalesControlOption[] options =
            [
                new(WeaponReadyStance.Normal.ToString(), PatchesUtil.GetWeaponReadyStanceLabel(WeaponReadyStance.Normal), GraphicsUtil.WeaponReadyNormalIcon),
                new(WeaponReadyStance.High.ToString(), PatchesUtil.GetWeaponReadyStanceLabel(WeaponReadyStance.High), GraphicsUtil.WeaponReadyHighIcon),
                new(WeaponReadyStance.Low.ToString(), PatchesUtil.GetWeaponReadyStanceLabel(WeaponReadyStance.Low), GraphicsUtil.WeaponReadyLowIcon)
            ];

            return new TalesControlRow(
                id: "MilitaryStances.WeaponStances",
                order: 110f,
                title: "MilitaryStances.UI.WeaponStancesTitle".Translate(),
                toggleIcon: GraphicsUtil.WeaponStanceIcon,
                toggleTooltip: "MilitaryStances.UI.ToggleWeaponStanceDesc".Translate(),
                isEnabled: () => comp.enableWeaponStance,
                setEnabled: value =>
                {
                    if (comp.enableWeaponStance != value)
                    {
                        comp.enableWeaponStance = value;
                        comp.Refresh();
                    }
                },
                options: options,
                getSelectedOptionId: () => comp.currentWeaponReadyStance.ToString(),
                selectOption: id =>
                {
                    if (Enum.TryParse(id, out WeaponReadyStance stance) && comp.currentWeaponReadyStance != stance)
                    {
                        comp.currentWeaponReadyStance = stance;
                        comp.Refresh();
                    }
                },
                selectionTooltip: "MilitaryStances.ChangeWeaponReadyStanceDesc".Translate(),
                selectionDisabledTooltip: "MilitaryStances.UI.EnableWeaponStancesFirst".Translate(),
                fallbackToggleLabel: "MilitaryStances.ToggleWeaponStanceLabel".Translate(comp.enableWeaponStance.ToString()),
                fallbackSelectionLabel: () => "MilitaryStances.ChangeWeaponReadyStanceLabel".Translate(PatchesUtil.GetWeaponReadyStanceLabel(comp.currentWeaponReadyStance)));
        }
    }
}
