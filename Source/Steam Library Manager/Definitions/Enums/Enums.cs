using Steam_Library_Manager.Framework;
using System.ComponentModel;

namespace Steam_Library_Manager.Definitions.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AppSortingMethod
    {
        [LocalizedDescription("Enums_Name")]
        appName,

        [LocalizedDescription("Enums_AppID")]
        appID,

        [LocalizedDescription("Enums_SizeOnDisk")]
        sizeOnDisk,

        [LocalizedDescription("Enums_BackupType")]
        backupType,

        [LocalizedDescription("Enums_LastUpdate")]
        LastUpdated,

        [LocalizedDescription("Enums_LastPlayed")]
        LastPlayed
    }

    public enum AppSizeCalculationMethod
    {
        ACF,
        Enumeration
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ArchiveSizeCalculationMethod
    {
        [LocalizedDescription("Enums_ArchiveSize")]
        compressed,

        [LocalizedDescription("Enums_ArchiveFileSize")]
        Uncompressed
    }

    public enum CompressionLevel
    {
        Optimal = 0,
        Fastest = 1,
        NoCompression = 2
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum LibraryStyle
    {
        [LocalizedDescription("Enums_GridView")]
        Grid,

        [LocalizedDescription("Enums_ListView")]
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TaskType
    {
        [LocalizedDescription("Enums_Copy")]
        Copy,

        [LocalizedDescription("Enums_Delete")]
        Delete,

        [LocalizedDescription("Enums_Compress")]
        Compress
    }

    public enum ThemeAccents
    {
        Red,
        Green,
        Blue,
        Purple,
        Orange,
        Lime,
        Emerald,
        Teal,
        Cyan,
        Cobalt,
        Indigo,
        Violet,
        Pink,
        Magenta,
        Crimson,
        Amber,
        Yellow,
        Brown,
        Olive,
        Steel,
        Mauve,
        Taupe,
        Sienna
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum BaseTheme
    {
        [LocalizedDescription("Enums_Light")]
        BaseLight,

        [LocalizedDescription("Enums_Dark")]
        BaseDark
    }
}