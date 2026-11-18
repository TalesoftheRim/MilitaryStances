using Verse;

namespace MilitaryStances
{
    public enum BodyStance : byte
    {
        Stand = 0,
        Prone = 1
    }

    public enum WeaponReadyStance : byte
    {
        Normal = 0,
        High = 1,
        Low = 2
    }

    public class CompProperties_MilitaryStances : CompProperties
    {
        public CompProperties_MilitaryStances()
        {
            compClass = typeof(CompMilitaryStances);
        }
    }

    public class CompMilitaryStances : ThingComp
    {
        public CompProperties_MilitaryStances Props => (CompProperties_MilitaryStances)props;

        public bool enableBodyStance = false;
        public BodyStance currentBodyStance = BodyStance.Prone;

        public bool enableWeaponStance = false;
        public WeaponReadyStance currentWeaponReadyStance = WeaponReadyStance.Normal;

        public bool CanUseBodyStance => parent is Pawn pawn && pawn.RaceProps?.Humanlike == true;

        public bool CanUseWeaponStance => parent is Pawn pawn &&
            (pawn.RaceProps?.Humanlike == true || pawn.RaceProps?.IsMechanoid == true);

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref enableBodyStance, "enableBodyStance", defaultValue: false);
            Scribe_Values.Look(ref currentBodyStance, "currentBodyStance", defaultValue: BodyStance.Prone);
            Scribe_Values.Look(ref enableWeaponStance, "enableWeaponStance", defaultValue: false);
            Scribe_Values.Look(ref currentWeaponReadyStance, "currentWeaponReadyStance", defaultValue: WeaponReadyStance.Normal);
        }

        public void Refresh()
        {
            if (parent is Pawn pawn)
            {
                pawn.Drawer?.renderer?.SetAllGraphicsDirty();
            }
        }
    }
}
