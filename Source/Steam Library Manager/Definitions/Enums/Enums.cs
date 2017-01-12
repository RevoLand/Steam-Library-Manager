using Steam_Library_Manager.Framework;
using System.ComponentModel;

namespace Steam_Library_Manager.Definitions.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum GameSortingMethod
    {
        [Description("Sort by appName")]
        appName,
        [Description("Sort by appID")]
        appID,
        [Description("Sort by app size on disk")]
        sizeOnDisk,
        [Description("Sort by backup type")]
        backupType
    }

    public enum GameSizeCalculationMethod
    {
        ACF,
        Enumeration
    }

    public enum ArchiveSizeCalculationMethod
    {
        compressed,
        Uncompressed
    }

    public enum MenuVisibility
    {
        NotVisible,
        Visible
    }

    public enum LibraryStyle
    {
        Grid,
        Listview
    }
}