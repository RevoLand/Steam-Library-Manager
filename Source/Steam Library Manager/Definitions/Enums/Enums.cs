using Steam_Library_Manager.Framework;
using System.ComponentModel;

namespace Steam_Library_Manager.Definitions.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AppSortingMethod
    {
        [Description("Sort by appName")]
        appName,
        [Description("Sort by appID")]
        appID,
        [Description("Sort by app size on disk")]
        sizeOnDisk,
        [Description("Sort by backup type")]
        backupType,
        [Description("Sort by latest update time")]
        LastUpdated
    }

    public enum AppSizeCalculationMethod
    {
        ACF,
        Enumeration
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ArchiveSizeCalculationMethod
    {
        [Description("Use archive size")]
        compressed,
        [Description("Calculate file size inside archive")]
        Uncompressed
    }

    public enum LibraryStyle
    {
        Grid,
        Listview
    }

    public enum LibraryType
    {
        Steam,
        Origin,
        Uplay,
        SLM
    }

    public enum GameType
    {
        Steam,
        Origin,
        Uplay
    }

    public enum TaskType
    {
        Copy,
        Delete
    }
}