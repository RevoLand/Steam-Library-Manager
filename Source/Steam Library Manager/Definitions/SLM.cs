using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Definitions
{
    // Definitions about Steam Library Manager (SLM)
    class SLM
    {
        public static List.Library selectedLibrary;
        public static string userSteamID64;

        public static List<Task> taskList = new List<Task>();
    }
}
