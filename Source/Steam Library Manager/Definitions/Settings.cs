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

        public gameSizeCalculationMethod Setting_GameSizeCalculationMethod
        {
            get => (gameSizeCalculationMethod)Enum.Parse(typeof(gameSizeCalculationMethod), Properties.Settings.Default.gameSizeCalculationMethod);
            set => Properties.Settings.Default.gameSizeCalculationMethod = value.ToString();
        }

        public archiveSizeCalculationMethod Setting_ArchiveSizeCalculationMethod
        {
            get => (archiveSizeCalculationMethod)Enum.Parse(typeof(archiveSizeCalculationMethod), Properties.Settings.Default.archiveSizeCalculationMethod);
            set => Properties.Settings.Default.archiveSizeCalculationMethod = value.ToString();
        }

        public LibraryStyle Setting_LibraryStyle
        {
            get => (LibraryStyle)Enum.Parse(typeof(LibraryStyle), Properties.Settings.Default.LibraryStyle);
            set => Properties.Settings.Default.LibraryStyle = value.ToString();
        }
    }
}
