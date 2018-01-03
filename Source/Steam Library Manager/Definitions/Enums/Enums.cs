using Steam_Library_Manager.Framework;
using System.ComponentModel;

namespace Steam_Library_Manager.Definitions.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AppSortingMethod
    {
        [Description("Name")]
        appName,
        [Description("AppID")]
        appID,
        [Description("Size on disk")]
        sizeOnDisk,
        [Description("Backup type")]
        backupType,
        [Description("Last updated first")]
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum LibraryStyle
    {
        [Description("Grid View")]
        Grid,
        [Description("List View")]
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
        [Description("Light")]
        BaseLight,
        [Description("Dark")]
        BaseDark
    }
}