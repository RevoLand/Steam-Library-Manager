using Steam_Library_Manager.Framework;
using System.ComponentModel;

namespace Steam_Library_Manager.Definitions.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum GameSortingMethod
    {
        [Description("Sort by appName")]
        appName = 0,
        [Description("Sort by appID")]
        appID = 1,
        [Description("Sort by app size on disk")]
        sizeOnDisk = 2,
        [Description("Sort by backup type")]
        backupType = 3
    }

    public enum gameSizeCalculationMethod
    {
        ACF = 0,
        Enumeration = 1
    }

    public enum archiveSizeCalculationMethod
    {
        compressed = 0,
        unUncompressed = 1
    }

    public enum menuVisibility
    {
        NotVisible,
        Visible
    }
}