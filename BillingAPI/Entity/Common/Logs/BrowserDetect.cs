using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Common.Logs
{
    using Microsoft.AspNetCore.Http;
    using System;

    internal class Platforms
    {
        public const string Windows10 = "Windows NT 10.0";
        public const string Pixel3 = "Pixel 3";
        public const string SMT835 = "SM-T835";
        public const string iPhone = "iPhone";
        public const string iPad = "iPad";
        public const string Macintosh = "Macintosh";
    }

    public static class DeviceTypes
    {
        public const string Desktop = "Desktop";
        public const string Mobile = "Mobile";
        public const string Tablet = "Tablet";
    }
    public class OperatingSystems
    {
        public const string Windows = "Windows";
        public const string Android = "Android";
        public const string MacOSX = "OSX";
        public const string IOS = "IOS";
    }
    public static class BrowserNames
    {
        public const string Chrome = "Chrome";
        public const string Edge = "Edge";
        public const string Firefox = "Firefox";
        public const string Opera = "Opera";
        public const string Safari = "Safari";
        public const string EdgeChromium = "EdgeChromium";
        public const string InternetExplorer = "InternetExplorer";
    }

    internal class Headers
    {
        public const string UserAgent = "User-Agent";
    }



    /// <summary>
    /// A type representing the browser information.
    /// </summary>
    public interface IBrowser
    {
        /// <summary>
        /// Gets the device type.
        /// Possible values are
        /// 1. Desktop
        /// 2. Tablet
        /// 3. Mobile
        /// </summary>
        string DeviceType { get; }

        /// <summary>
        /// Gets the browser name.
        /// Ex:"Chrome"
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the operating system.
        /// Ex:"Windows"
        /// </summary>
        string OS { get; }

        /// <summary>
        /// Gets the browser version.
        /// </summary>
        string Version { get; }
    }

    internal abstract class Browser : IBrowser
    {
        private readonly string platform;

        protected Browser(ReadOnlySpan<char> userAgent, string version)
        {
            this.Version = version;

            var platform = PlatformDetector.GetPlatformAndOS(userAgent);
            this.platform = platform.Platform;
            this.OS = platform.OS;

            // Get the device type from platform info.
            this.DeviceType = this.GetDeviceType(platform);
        }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public string DeviceType { get; }

        /// <inheritdoc/>
        public string Version { get; }

        /// <inheritdoc/>
        public string OS { get; }

        /// <summary>
        /// Gets the version segment from user agent for the key passed in.
        /// </summary>
        /// <param name="userAgent">The user agent value.</param>
        /// <param name="key">The key to use for looking up the version segment.</param>
        /// <returns>The version segment.</returns>
        protected static string GetVersionIfKeyPresent(ReadOnlySpan<char> userAgent, string key)
        {
            var keyStartIndex = userAgent.IndexOf(key.AsSpan());

            if (keyStartIndex == -1)
            {
                return null;
            }

            var sliceWithVersionPart = userAgent.Slice(keyStartIndex + key.Length);

            var endIndex = sliceWithVersionPart.IndexOf(' ');
            if (endIndex > -1)
            {
                return sliceWithVersionPart.Slice(0, endIndex).ToString();
            }

            return sliceWithVersionPart.ToString();
        }

        /// <summary>
        /// Gets the device type from the platform information.
        /// </summary>
        /// <param name="platform">The platform information.</param>
        /// <returns>The device type value.</returns>
        private string GetDeviceType((string Platform, string OS, bool MobileDetected) platform)
        {
            if (this.platform == Platforms.iPhone)
            {
                return DeviceTypes.Mobile;
            }
            else if (this.platform == Platforms.iPad || this.platform == "GalaxyTabS4")
            {
                return DeviceTypes.Tablet;
            }

            // IPad also has Mobile in it. So make sure to check that first
            if (platform.MobileDetected)
            {
                return DeviceTypes.Mobile;
            }
            else if (this.platform == Platforms.Macintosh || this.platform.StartsWith("Windows NT"))
            {
                return DeviceTypes.Desktop;
            }

            // Samsung Chrome_GalaxyTabS4 does not have "Mobile", but it has Linux and Android.
            if (this.platform == "Linux" && platform.OS == "Android" && platform.MobileDetected == false)
            {
                return DeviceTypes.Tablet;
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Represents an instance of Chrome Browser
    /// has both "Safari" and "Chrome" in UA
    /// Sample user agent string: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36
    /// </summary>
    internal class Chrome : Browser
    {
        public Chrome(ReadOnlySpan<char> userAgent, string version)
            : base(userAgent, version)
        {
        }

        /// <inheritdoc/>
        public override string Name => BrowserNames.Chrome;

        /// <summary>
        /// Populates a Chrome browser object from the userAgent value passed in. A return value indicates the parsing and populating the browser instance succeeded.
        /// </summary>
        /// <param name="userAgent">User agent value</param>
        /// <param name="result">When this method returns True, the result will contain a Chrome object populated</param>
        /// <returns>True if parsing succeeded, else False</returns>
        public static bool TryParse(ReadOnlySpan<char> userAgent, out Chrome result)
        {
            var chromeIndex = userAgent.IndexOf("Chrome/".AsSpan());
            var safariIndex = userAgent.IndexOf("Safari/".AsSpan());
            var crIOS = userAgent.IndexOf("CriOS/".AsSpan());

            // Chrome should have both "Safari" and "Chrome" words in it.
            if ((safariIndex > -1 && chromeIndex > -1) || (safariIndex > -1 && crIOS > -1))
            {
                var fireFoxVersion = GetVersionIfKeyPresent(userAgent, "Chrome/");
                if (fireFoxVersion != null)
                {
                    result = new Chrome(userAgent, fireFoxVersion);
                    return true;
                }

                var chromeIosVersion = GetVersionIfKeyPresent(userAgent, "CriOS/");
                if (chromeIosVersion != null)
                {
                    result = new Chrome(userAgent, chromeIosVersion);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }

    /// <summary>
    /// Represents an instance of Edge Browser.
    /// </summary>
    internal class Edge : Browser
    {
        public Edge(ReadOnlySpan<char> userAgent, string version)
            : base(userAgent, version)
        {
        }

        /// <inheritdoc/>
        public override string Name => BrowserNames.Edge;

        /// <summary>
        /// Tries to create an Edge browser object from the user agent passed in.
        /// </summary>
        /// <param name="userAgent">The user agent.</param>
        /// <param name="result">An instance of Edge browser, if parsing was successful.</param>
        /// <returns>A boolean value indicating whether the parsing was successful.</returns>
        public static bool TryParse(ReadOnlySpan<char> userAgent, out Browser result)
        {
            var edgeVersion = GetVersionIfKeyPresent(userAgent, "Edge/");
            var edgeIosVersion = GetVersionIfKeyPresent(userAgent, "EdgiOS/");
            var edgeAndroidVersion = GetVersionIfKeyPresent(userAgent, "EdgA/");

            var version = edgeVersion ?? edgeIosVersion ?? edgeAndroidVersion;

            if (version == null)
            {
                result = null;
                return false;
            }

            result = new Edge(userAgent, version);
            return true;
        }
    }

    /// <summary>
    /// Represents an instance of EdgeChromium Browser.
    /// </summary>
    internal class EdgeChromium : Browser
    {
        public EdgeChromium(ReadOnlySpan<char> userAgent, string version)
            : base(userAgent, version)
        {
        }

        /// <inheritdoc/>
        public override string Name => BrowserNames.EdgeChromium;

        /// <summary>
        /// Tries to build a EdgeChromium browser instance from the user agent passed in and
        /// returns a value that indicates whether the parsing succeeded.
        /// </summary>
        /// <param name="userAgent">The user agent value.</param>
        /// <param name="result">An EdgeChromium browser instance.</param>
        /// <returns>A boolean value that indicates whether the parsing succeeded.</returns>
        public static bool TryParse(ReadOnlySpan<char> userAgent, out EdgeChromium result)
        {
            var edgChromiumVersion = GetVersionIfKeyPresent(userAgent, "Edg/");

            if (edgChromiumVersion != null)
            {
                result = new EdgeChromium(userAgent, edgChromiumVersion);
                return true;
            }

            result = null;
            return false;
        }
    }

    /// <summary>
    /// A type representing the FireFox browser instance.
    /// </summary>
    internal class Firefox : Browser
    {
        private Firefox(ReadOnlySpan<char> userAgent, string version)
    : base(userAgent, version)
        {
        }

        public string Platform { get; }

        /// <inheritdoc/>
        public override string Name => BrowserNames.Firefox;

        /// <summary>
        /// Tries to build a Firefox browser instance from the user agent passed in and
        /// returns a value that indicates whether the parsing succeeded.
        /// </summary>
        /// <param name="userAgent">The user agent value.</param>
        /// <param name="result">A Firefox browser instance.</param>
        /// <returns>A boolean value that indicates whether the parsing succeeded.</returns>
        public static bool TryParse(ReadOnlySpan<char> userAgent, out Firefox result)
        {
            // Desktop version of Firefox.
            var fireFoxVersion = GetVersionIfKeyPresent(userAgent, "Firefox/");
            if (fireFoxVersion != null)
            {
                result = new Firefox(userAgent, fireFoxVersion);
                return true;
            }

            // IOS version of Firefox.
            var fxiosVersion = GetVersionIfKeyPresent(userAgent, "FxiOS/");
            if (fxiosVersion != null)
            {
                result = new Firefox(userAgent, fxiosVersion);
                return true;
            }

            result = null;
            return false;
        }
    }

    /// <summary>
    /// Represents an instance of Edge Browser
    /// </summary>
    internal class InternetExplorer : Browser
    {
        public InternetExplorer(ReadOnlySpan<char> userAgent, string version)
            : base(userAgent, version)
        {
        }

        public override string Name => BrowserNames.InternetExplorer;

        /// <summary>
        /// Tries to build an instance of InternetExplorer browser from the user agent passed in and
        /// returns a value that indicates whether the parsing succeeded.
        /// </summary>
        /// <param name="userAgent">The user agent value.</param>
        /// <param name="result">An instance of EdgeChromium browser.</param>
        /// <returns>A boolean value that indicates whether the parsing succeeded.</returns>
        public static bool TryParse(ReadOnlySpan<char> userAgent, out InternetExplorer result)
        {
            var tridentVersion = GetVersionIfKeyPresent(userAgent, "Trident/");

            if (tridentVersion != null)
            {
                result = new InternetExplorer(userAgent, tridentVersion);
                return true;
            }

            result = null;
            return false;
        }
    }

    /// <summary>
    /// Represents an instance of Opera Browser.
    /// </summary>
    internal class Opera : Browser
    {
        public Opera(ReadOnlySpan<char> userAgent, string version)
            : base(userAgent, version)
        {
        }

        public override string Name => BrowserNames.Opera;

        /// <summary>
        /// Tries to build an instance of Opera browser from the user agent passed in and
        /// returns a value that indicates whether the parsing succeeded.
        /// </summary>
        /// <param name="userAgent">The user agent value.</param>
        /// <param name="result">An instance of Opera browser.</param>
        /// <returns>A boolean value that indicates whether the parsing succeeded.</returns>
        public static bool TryParse(ReadOnlySpan<char> userAgent, out Opera result)
        {
            var operaVersion = GetVersionIfKeyPresent(userAgent, "OPR/");
            var operaTouchVersion = GetVersionIfKeyPresent(userAgent, " OPT/");

            if (operaVersion != null)
            {
                result = new Opera(userAgent, operaVersion);
                return true;
            }

            if (operaTouchVersion != null)
            {
                result = new Opera(userAgent, operaVersion);
                return true;
            }

            result = null;
            return false;
        }
    }

    internal class Safari : Browser
    {
        public Safari(ReadOnlySpan<char> userAgent, string version)
            : base(userAgent, version)
        {
        }

        public override string Name => BrowserNames.Safari;

        /// <summary>
        /// Populates a Safari browser object from the userAgent value passed in. A return value indicates the parsing and populating the browser instance succeeded.
        /// </summary>
        /// <param name="userAgent">User agent value</param>
        /// <param name="result">When this method returns True, the result will contain a Safari object populated</param>
        /// <returns>True if parsing succeeded, else False</returns>
        /// <exception cref="ArgumentNullException">Thrown when userAgent parameter value is null</exception>
        public static bool TryParse(ReadOnlySpan<char> userAgent, out Safari result)
        {
            var chromeIndex = userAgent.IndexOf("Chrome/".AsSpan());
            var safariIndex = userAgent.IndexOf("Safari/".AsSpan());

            // Safari UA does not have the word "Chrome/"
            if (safariIndex > -1 && chromeIndex == -1)
            {
                var fireFoxVersion = GetVersionIfKeyPresent(userAgent, "Safari/");
                if (fireFoxVersion != null)
                {
                    result = new Safari(userAgent, fireFoxVersion);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }

    /// <summary>
    /// The browser detector.
    /// </summary>
    internal static class Detector
    {
        /// <summary>
        /// Gets an IBrowser instance from the user agent string passed in.
        /// </summary>
        /// <param name="userAgentString">The user agent string.</param>
        /// <returns>An instance of IBrowser.</returns>
        internal static IBrowser GetBrowser(ReadOnlySpan<char> userAgentString)
        {
            // Order is important, Go from most specific to generic
            // For example, The string "Chrome" is present in both Chrome and Edge,
            // So we will first check if it is Edge because Edge has something more specific we can check.
            if (Firefox.TryParse(userAgentString, out var firefox))
            {
                return firefox;
            }

            if (EdgeChromium.TryParse(userAgentString, out var edgeChromium))
            {
                return edgeChromium;
            }

            if (InternetExplorer.TryParse(userAgentString, out var ie))
            {
                return ie;
            }

            if (Opera.TryParse(userAgentString, out var opera))
            {
                return opera;
            }

            if (Edge.TryParse(userAgentString, out var edge))
            {
                return edge;
            }

            if (Chrome.TryParse(userAgentString, out var chrome))
            {
                return chrome;
            }

            if (Safari.TryParse(userAgentString, out var safari))
            {
                return safari;
            }

            return default;
        }
    }

    /// <summary>
    /// A helper to detect the platform.
    /// </summary>
    internal static class PlatformDetector
    {
        internal static (string Platform, string OS, bool MobileDetected) GetPlatformAndOS(ReadOnlySpan<char> userAgentString)
        {
            // Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.

            // Platform starts with a "(". So get it's index
            var platformSearhKeyStartIndex = " (".AsSpan();

            // The index of substring where platform part starts
            var platformSubstringStartIndex = userAgentString.IndexOf(platformSearhKeyStartIndex) + platformSearhKeyStartIndex.Length;

            // Get substring which starts with platform part (Trim out anything before platform)
            var platformSubstring = userAgentString.Slice(platformSubstringStartIndex);

            // Find end index of end character of platform part.
            var platFormPartEndIndex = platformSubstring.IndexOf(';');

            // For 32 bit, no ";" present, so get the closing ")";
            if (platFormPartEndIndex == -1)
            {
                platFormPartEndIndex = platformSubstring.IndexOf(')');
            }

            // Get the platform part slice
            var platformSlice = platformSubstring.Slice(0, platFormPartEndIndex);

            // OS part is between two ";" after platform slice
            // Get the sub slice which is after platform
            var osSubString = platformSubstring.Slice(platFormPartEndIndex + 2); // ';' length +' ' length

            // Find the end index of platform end character from the os sub slice
            var osPartEndIndex = osSubString.IndexOf(')');

            // Get the OS part slice
            var operatingSystemSlice = osSubString.Slice(0, osPartEndIndex);

            // If OS part starts with "Linux", check for next segment to get android veersion //Linux; Android 9; Pixel 3
            // Linux; Android 8.1.0; SM-T835
            var platform = platformSlice.ToString();

            var isMobileSlicePresent = userAgentString.IndexOf("Mobile".AsSpan()) > -1;

            var os = GetReadableOSName(platform, operatingSystemSlice.ToString());

            return (Platform: platform, OS: os, MobileDetected: isMobileSlicePresent);
        }

        /// <summary>
        /// Gets a readable OS name from the platform & operatingSystem info.
        /// For some cases, the "operatingSystem" param value is not enought and we rely on the platform param value.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="operatingSystem"></param>
        /// <returns>The OS name.</returns>
        private static string GetReadableOSName(string platform, string operatingSystem)
        {
            if (platform == Platforms.iPhone || platform == Platforms.iPad)
            {
                return OperatingSystems.IOS;
            }

            // If platform starts with "Android" (Firefox galaxy tab4)
            if (platform == "Android")
            {
                return OperatingSystems.Android;
            }

            if (platform == "Macintosh")
            {
                return OperatingSystems.MacOSX;
            }

            if (platform.StartsWith("Windows NT"))
            {
                return OperatingSystems.Windows;
            }

            if (platform == "Pixel 3")
            {
                return OperatingSystems.Android;
            }

            // Pixel 3
            if (platform == "Linux" && operatingSystem.IndexOf("Android", StringComparison.OrdinalIgnoreCase) > -1)
            {
                return OperatingSystems.Android;
            }

            return operatingSystem;
        }
    }
}
