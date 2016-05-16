using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Definitions
{
    // Definitions about Steam Library Manager (SLM)
    class SLM
    {
        public static string paypalDonationURL = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=AL4F5252SQVR4&lc=US&item_name=Steam%20Library%20Manager&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted";

        public static Library selectedLibrary;
        public static string userSteamID64;

        public static List<Task> taskList = new List<Task>();
    }
}
