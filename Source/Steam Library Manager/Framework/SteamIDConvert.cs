using System;
using System.Globalization;
using System.Text.RegularExpressions;

/* SteamID-NET
 * https://github.com/NachoReplay/SteamID-NET
 * GNU General Public License v3.0
 * https://github.com/NachoReplay/SteamID-NET/blob/master/LICENSE
 */

namespace Steam_Library_Manager.Framework
{
    /// <summary>
    /// Auth type enumeration, From SourcePawn's Clients.inc
    /// </summary>
    public enum AuthIdType
    {
        /// <summary>
        /// The game-specific auth string as returned from the engine
        /// </summary>
        AuthId_Engine = 0,

        // The following are only available on games that support Steam authentication.
        /// <summary>
        /// Steam2 rendered format, ex "STEAM_1:1:4153990"
        /// </summary>
        AuthId_Steam2,

        /// <summary>
        /// Steam3 rendered format, ex "U:1:8307981"
        /// </summary>
        AuthId_Steam3,

        /// <summary>
        /// A SteamID64 (uint64) as a String, ex "76561197968573709"
        /// </summary>
        AuthId_SteamID64,
    };

    /// <summary>
    /// SteamID Regex constents
    /// </summary>
    public static class SteamIDRegex
    {
        /// <summary>
        /// SteamID2 Regex
        /// </summary>
        public const string Steam2Regex = "^STEAM_0:[0-1]:([0-9]{1,10})$";

        /// <summary>
        /// SteamID32 Regex
        /// </summary>
        public const string Steam32Regex = "^U:1:([0-9]{1,10})$";

        /// <summary>
        /// SteamID64 Regex
        /// </summary>
        public const string Steam64Regex = "^7656119([0-9]{10})$";
    }

    /// <summary>
    /// SteamId converting class.
    /// </summary>
    public static class SteamIDConvert
    {
        /// <summary>
        /// Converts a <see cref="AuthIdType.AuthId_Steam3"/> to a <see cref="AuthIdType.AuthId_Steam2"/> format.
        /// </summary>
        /// <param name="input">String input of AuthId_Steam3</param>
        /// <returns>Returns the SteamID2(STEAM_0:1:000000) string.</returns>
        public static string Steam32ToSteam2(string input)
        {
            if (!Regex.IsMatch(input, SteamIDRegex.Steam32Regex))
            {
                return string.Empty;
            }
            long steam64 = Steam32ToSteam64(input);
            return Steam64ToSteam2(steam64);
        }

        /// <summary>
        /// Converts a <see cref="AuthIdType.AuthId_Steam2"/> to a <see cref="AuthIdType.AuthId_Steam3"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Steam2ToSteam32(string input)
        {
            if (!Regex.IsMatch(input, SteamIDRegex.Steam2Regex))
            {
                return string.Empty;
            }
            long steam64 = Steam2ToSteam64(input);
            return Steam64ToSteam32(steam64);
        }

        /// <summary>
        /// Converts our <see cref="AuthIdType.AuthId_Steam3"/> to the <see cref="AuthIdType.AuthId_SteamID64"/> format.
        /// </summary>
        /// <param name="input">AuthId_Steam3</param>
        /// <returns>Returns the SteamID64(76561197960265728) in long type</returns>
        public static long Steam32ToSteam64(string input)
        {
            long steam32 = Convert.ToInt64(input.Substring(4));
            if (steam32 < 1L || !Regex.IsMatch("U:1:" + steam32.ToString(CultureInfo.InvariantCulture), "^U:1:([0-9]{1,10})$"))
            {
                return 0;
            }
            return steam32 + 76561197960265728L;
        }

        /// <summary>
        /// Converts a <see cref="AuthIdType.AuthId_SteamID64"/> to a <see cref="AuthIdType.AuthId_Steam2"/>
        /// </summary>
        /// <param name="communityId">SteamID64(76561197960265728)</param>
        /// <returns>String.empty if error, else the string SteamID2(STEAM_0:1:000000)</returns>
        public static string Steam64ToSteam2(long communityId)
        {
            if (communityId < 76561197960265729L || !Regex.IsMatch(communityId.ToString(CultureInfo.InvariantCulture), "^7656119([0-9]{10})$"))
                return string.Empty;
            communityId -= 76561197960265728L;
            long num = communityId % 2L;
            communityId -= num;
            string input = string.Format("STEAM_0:{0}:{1}", num, (communityId / 2L));
            if (!Regex.IsMatch(input, "^STEAM_0:[0-1]:([0-9]{1,10})$"))
            {
                return string.Empty;
            }
            return input;
        }

        /// <summary>
        /// Converts a <see cref="AuthIdType.AuthId_Steam2"/> to a <see cref="AuthIdType.AuthId_SteamID64"/>
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>Retruns a <see cref="AuthIdType.AuthId_SteamID64"/></returns>
        public static long Steam2ToSteam64(string accountId)
        {
            if (!Regex.IsMatch(accountId, "^STEAM_0:[0-1]:([0-9]{1,10})$"))
            {
                return 0;
            }
            return 76561197960265728L + Convert.ToInt64(accountId.Substring(10)) * 2L + Convert.ToInt64(accountId.Substring(8, 1));
        }

        /// <summary>
        /// Converts a <see cref="AuthIdType.AuthId_SteamID64"/> to a <see cref="AuthIdType.AuthId_Steam3"/>
        /// </summary>
        /// <param name="communityId"></param>
        /// <returns>Returns a <see cref="AuthIdType.AuthId_Steam3"/> string</returns>
        public static string Steam64ToSteam32(long communityId)
        {
            if (communityId < 76561197960265729L || !Regex.IsMatch(communityId.ToString(CultureInfo.InvariantCulture), "^7656119([0-9]{10})$"))
            {
                return string.Empty;
            }
            return string.Format("U:1:{0}", communityId - 76561197960265728L);
        }
    }

    /// <summary>
    /// Here we have our engine object for conversion, manipulation, and identification of ID.
    /// </summary>
    public class SteamID_Engine
    {
        /// <summary>
        /// The SteamID we are working with.
        /// </summary>
        public string WorkingID { get; private set; }

        /// <summary>
        /// The current working steamid <see cref="AuthIdType"/> if <see cref="AuthIdType.AuthId_Engine"/> is set that means that the ID is NOT a valid account ID.
        /// </summary>
        public AuthIdType AuthType { private set; get; }

        /// <summary>
        /// The <see cref="AuthIdType.AuthId_Steam2"/> equivalent of the <see cref="WorkingID"/>
        /// </summary>
        public string Steam2 { private set; get; }

        /// <summary>
        /// The <see cref="AuthIdType.AuthId_Steam3"/> equivalent of <see cref="WorkingID"/>
        /// </summary>
        public string Steam32 { private set; get; }

        /// <summary>
        /// The <see cref="AuthIdType.AuthId_SteamID64"/> equivalent of <see cref="WorkingID"/>
        /// </summary>
        public long Steam64 { private set; get; }

        /// <summary>
        ///  Sets our objects steamid refrence to work with. <see cref="WorkingID"/>
        /// </summary>
        /// <param name="ID">An id to work with.</param>
        public SteamID_Engine(string ID)
        {
            WorkingID = ID;
            if (Regex.IsMatch(WorkingID, SteamIDRegex.Steam2Regex))
            {
                AuthType = AuthIdType.AuthId_Steam2;
                Steam2 = WorkingID;
                Steam32 = SteamIDConvert.Steam2ToSteam32(WorkingID);
                Steam64 = SteamIDConvert.Steam2ToSteam64(WorkingID);
            }
            else if (Regex.IsMatch(WorkingID, SteamIDRegex.Steam32Regex))
            {
                AuthType = AuthIdType.AuthId_Steam3;
                Steam2 = SteamIDConvert.Steam32ToSteam2(WorkingID);
                Steam32 = WorkingID;
                Steam64 = SteamIDConvert.Steam32ToSteam64(WorkingID);
            }
            else if (Regex.IsMatch(WorkingID, SteamIDRegex.Steam64Regex))
            {
                AuthType = AuthIdType.AuthId_SteamID64;
                Steam2 = SteamIDConvert.Steam64ToSteam2(Int64.Parse(WorkingID));
                Steam32 = SteamIDConvert.Steam64ToSteam32(Int64.Parse(WorkingID));
                Steam64 = Int64.Parse(WorkingID);
            }
            else
            {
                AuthType = AuthIdType.AuthId_Engine;
            }
        }

        /// <summary>
        /// Returns the string representation of the current <see cref="WorkingID"/>
        /// </summary>
        /// <returns></returns>
        public string AuthType_string()
        {
            if (AuthType == AuthIdType.AuthId_Steam2)
            {
                return "SteamID2";
            }
            else if (AuthType == AuthIdType.AuthId_Steam3)
            {
                return "SteamID32";
            }
            else if (AuthType == AuthIdType.AuthId_SteamID64)
            {
                return "SteamID64";
            }
            else if (AuthType == AuthIdType.AuthId_Engine)
            {
                return "Engine ID";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}