using Verse;

namespace MilitaryStances
{
    public class SettingsMain : ModSettings
    {
        public const float DefaultProneWeaponSideOffset = 0.15f;
        public const float DefaultProneWeaponForwardOffset = 0.12f;
        public const float DefaultProneWeaponStanceVerticalOffset = 0.06f;

        private static SettingsMain _instance;

        public SettingsMain()
        {
            _instance = this;
        }

        public bool activated = true;
        public bool showButtons = true;
        public bool proneAffectsMoveSpeed = false;
        public bool proneWeaponBehindPawn = false;
        public bool proneWeaponOffset = true;
        public float proneWeaponSideOffset = DefaultProneWeaponSideOffset;
        public float proneWeaponForwardOffset = DefaultProneWeaponForwardOffset;
        public float proneWeaponStanceVerticalOffset = DefaultProneWeaponStanceVerticalOffset;
        public bool weaponReadyIncludeMelee = false;
        public bool ignoreEquippedAngleOffset = false;

        public static bool Activated => _instance == null || _instance.activated;

        public static bool ProneAffectsMoveSpeed => _instance != null && _instance.proneAffectsMoveSpeed;

        public static bool ProneWeaponBehindPawn => _instance != null && _instance.proneWeaponBehindPawn;

        public static bool ProneWeaponOffset => _instance == null || _instance.proneWeaponOffset;

        public static float ProneWeaponSideOffset => _instance?.proneWeaponSideOffset ?? DefaultProneWeaponSideOffset;

        public static float ProneWeaponForwardOffset => _instance?.proneWeaponForwardOffset ?? DefaultProneWeaponForwardOffset;

        public static float ProneWeaponStanceVerticalOffset => _instance?.proneWeaponStanceVerticalOffset ?? DefaultProneWeaponStanceVerticalOffset;

        public static bool WeaponReadyIncludeMelee => _instance != null && _instance.weaponReadyIncludeMelee;

        public static bool IgnoreEquippedAngleOffset => _instance != null && _instance.ignoreEquippedAngleOffset;

        public void ResetProneWeaponOffsets()
        {
            proneWeaponSideOffset = DefaultProneWeaponSideOffset;
            proneWeaponForwardOffset = DefaultProneWeaponForwardOffset;
            proneWeaponStanceVerticalOffset = DefaultProneWeaponStanceVerticalOffset;
        }

        public static bool ShowButtons
        {
            get => _instance == null || _instance.showButtons;
            set
            {
                if (_instance != null && value != _instance.showButtons)
                {
                    _instance.showButtons = value;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref activated, "activated", defaultValue: true);
            Scribe_Values.Look(ref showButtons, "showButtons", defaultValue: true);
            Scribe_Values.Look(ref proneAffectsMoveSpeed, "proneAffectsMoveSpeed", defaultValue: false);
            Scribe_Values.Look(ref proneWeaponBehindPawn, "proneWeaponBehindPawn", defaultValue: false);
            Scribe_Values.Look(ref proneWeaponOffset, "proneWeaponOffset", defaultValue: true);
            Scribe_Values.Look(ref proneWeaponSideOffset, "proneWeaponSideOffset", defaultValue: DefaultProneWeaponSideOffset);
            Scribe_Values.Look(ref proneWeaponForwardOffset, "proneWeaponForwardOffset", defaultValue: DefaultProneWeaponForwardOffset);
            Scribe_Values.Look(ref proneWeaponStanceVerticalOffset, "proneWeaponStanceVerticalOffset", defaultValue: DefaultProneWeaponStanceVerticalOffset);
            Scribe_Values.Look(ref weaponReadyIncludeMelee, "weaponReadyIncludeMelee", defaultValue: false);
            Scribe_Values.Look(ref ignoreEquippedAngleOffset, "ignoreEquippedAngleOffset", defaultValue: false);
        }
    }
}
