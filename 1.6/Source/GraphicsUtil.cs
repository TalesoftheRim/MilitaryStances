using UnityEngine;
using Verse;

namespace MilitaryStances
{
    [StaticConstructorOnStartup]
    public static class GraphicsUtil
    {
        public static Texture2D BodyStanceIcon;
        public static Texture2D BodyStanceStandIcon;
        public static Texture2D BodyStanceProneIcon;
        public static Texture2D WeaponStanceIcon;
        public static Texture2D WeaponReadyHighIcon;
        public static Texture2D WeaponReadyNormalIcon;
        public static Texture2D WeaponReadyLowIcon;
        public static Texture2D ToggleDisplayIcon;

        static GraphicsUtil()
        {
            BodyStanceIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/BodyStance_Mode", reportFailure: false)
                ?? BaseContent.BadTex;

            BodyStanceStandIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/BodyStance_Stand", reportFailure: false)
                ?? BodyStanceIcon;

            BodyStanceProneIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/BodyStance_Prone", reportFailure: false)
                ?? BodyStanceIcon;

            WeaponStanceIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/WeaponStance_Mode", reportFailure: false)
                ?? BaseContent.BadTex;

            WeaponReadyHighIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/WeaponStance_High", reportFailure: false)
                ?? WeaponStanceIcon;

            WeaponReadyNormalIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/WeaponStance_Normal", reportFailure: false)
                ?? WeaponStanceIcon;

            WeaponReadyLowIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/WeaponStance_Lower", reportFailure: false)
                ?? WeaponStanceIcon;

            ToggleDisplayIcon = ContentFinder<Texture2D>.Get("UI/Icons/MilitaryStances/MilitaryStances_Toggle", reportFailure: false)
                ?? BodyStanceIcon;
        }

        public static Texture2D GetBodyStanceIcon(BodyStance stance)
        {
            return stance switch
            {
                BodyStance.Stand => BodyStanceStandIcon,
                BodyStance.Prone => BodyStanceProneIcon,
                _ => BodyStanceIcon
            };
        }

        public static Texture2D GetWeaponReadyStanceIcon(WeaponReadyStance stance)
        {
            return stance switch
            {
                WeaponReadyStance.Normal => WeaponReadyNormalIcon,
                WeaponReadyStance.High => WeaponReadyHighIcon,
                WeaponReadyStance.Low => WeaponReadyLowIcon,
                _ => WeaponStanceIcon
            };
        }
    }
}
