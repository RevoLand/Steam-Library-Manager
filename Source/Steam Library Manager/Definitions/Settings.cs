using Steam_Library_Manager.Definitions.Enums;
using System;

namespace Steam_Library_Manager.Definitions
{
    public class Settings
    {
        public GameSortingMethod Setting_GameSortingMethod
        {
            get => (GameSortingMethod)Enum.Parse(typeof(GameSortingMethod), Properties.Settings.Default.defaultGameSortingMethod);
            set => Properties.Settings.Default.defaultGameSortingMethod = value.ToString();
        }

        public GameSizeCalculationMethod Setting_GameSizeCalculationMethod
        {
            get => (GameSizeCalculationMethod)Enum.Parse(typeof(GameSizeCalculationMethod), Properties.Settings.Default.gameSizeCalculationMethod);
            set => Properties.Settings.Default.gameSizeCalculationMethod = value.ToString();
        }

        public ArchiveSizeCalculationMethod Setting_ArchiveSizeCalculationMethod
        {
            get => (ArchiveSizeCalculationMethod)Enum.Parse(typeof(ArchiveSizeCalculationMethod), Properties.Settings.Default.archiveSizeCalculationMethod);
            set => Properties.Settings.Default.archiveSizeCalculationMethod = value.ToString();
        }

        public LibraryStyle Setting_LibraryStyle
        {
            get => (LibraryStyle)Enum.Parse(typeof(LibraryStyle), Properties.Settings.Default.LibraryStyle);
            set => Properties.Settings.Default.LibraryStyle = value.ToString();
        }
    }
}
