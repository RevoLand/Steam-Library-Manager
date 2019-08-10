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
        [LocalizedDescription("Enums_Optimal")]
        Optimal = 0,

        [LocalizedDescription("Enums_Fastest")]
        Fastest = 1,

        [LocalizedDescription("Enums_Store")]
        NoCompression = 2
    }

    public enum CompactLevel
    {
        XPRESS4K,
        XPRESS8K,
        XPRESS16K,
        LZX
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TaskType
    {
        [LocalizedDescription("Enums_Copy")]
        Copy,

        [LocalizedDescription("Enums_Delete")]
        Delete,

        [LocalizedDescription("Enums_Compress")]
        Compress,

        [LocalizedDescription("Enums_Compact")]
        Compact
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum JunkType
    {
        [LocalizedDescription("HeadlessFolder")]
        HeadlessFolder,

        [LocalizedDescription("HeadlessWorkshopFolder")]
        HeadlessWorkshopFolder,

        [LocalizedDescription("CorruptedDataFile")]
        CorruptedDataFile,

        [LocalizedDescription("HeadlessDataFile")]
        HeadlessDataFile
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