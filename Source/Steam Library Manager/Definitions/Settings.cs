using Steam_Library_Manager.Definitions.Enums;
using System;

namespace Steam_Library_Manager.Definitions
{
    public class Settings
    {
        public AppSortingMethod Setting_AppSortingMethod
        {
            get => (AppSortingMethod)Enum.Parse(typeof(AppSortingMethod), Properties.Settings.Default.defaultGameSortingMethod);
            set => Properties.Settings.Default.defaultGameSortingMethod = value.ToString();
        }

        public AppSizeCalculationMethod Setting_AppSizeCalculationMethod
        {
            get => (AppSizeCalculationMethod)Enum.Parse(typeof(AppSizeCalculationMethod), Properties.Settings.Default.gameSizeCalculationMethod);
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
