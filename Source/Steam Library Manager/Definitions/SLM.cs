namespace Steam_Library_Manager.Definitions
{
    // Definitions about Steam Library Manager (SLM)
    public static class SLM
    {
        public static Library CurrentSelectedLibrary;
        public static string UserSteamID64;

        public static int NetworkBuffer = 4096 * 1024;

        public static string DonateButtonURL = "https://github.com/RevoLand/Steam-Library-Manager/wiki/Donations";

        public static SharpRaven.RavenClient RavenClient = new SharpRaven.RavenClient("https://0ca1e59a781e4d409f934b4e1e5f9179:9e88a9d4f17a4133aad77214d5545745@sentry.io/263535");
    }
}