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

        public CompressionLevel Setting_CompressionLevel
        {
            get => (CompressionLevel)Enum.Parse(typeof(CompressionLevel), Properties.Settings.Default.CompressionLevel);
            set => Properties.Settings.Default.CompressionLevel = value.ToString();
        }

        public LibraryStyle Setting_LibraryStyle
        {
            get => (LibraryStyle)Enum.Parse(typeof(LibraryStyle), Properties.Settings.Default.LibraryStyle);
            set => Properties.Settings.Default.LibraryStyle = value.ToString();
        }

        public ThemeAccents Setting_ThemeAccent
        {
            get => (ThemeAccents)Enum.Parse(typeof(ThemeAccents), Properties.Settings.Default.ThemeAccent);
            set => Properties.Settings.Default.ThemeAccent = value.ToString();
        }

        public BaseTheme Setting_BaseTheme
        {
            get => (BaseTheme)Enum.Parse(typeof(BaseTheme), Properties.Settings.Default.BaseTheme);
            set => Properties.Settings.Default.BaseTheme = value.ToString();
        }

        public CompactLevel Setting_CompactLevel
        {
            get => (CompactLevel)Enum.Parse(typeof(CompactLevel), Properties.Settings.Default.DefaultCompactLevel);
            set => Properties.Settings.Default.DefaultCompactLevel = value.ToString();
        }
    }
}