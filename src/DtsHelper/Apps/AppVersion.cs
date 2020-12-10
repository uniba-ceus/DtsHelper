// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Apps
{
    using System;

    /// <summary>
    ///     Version of this application build by an auto generated assembly version declared like 1.0.*
    ///     This version string is separated in 4 parts (Example 1.0.232.9594):
    ///     - Major Version: 1
    ///     - Minor Version: 0
    ///     - Build Number (Days from 1.1.2000): 232
    ///     - Revision Number (Seconds from 12:00am UTC divided by 2): 9594
    /// </summary>
    public class AppVersion
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AppVersion" /> class.
        /// </summary>
        /// <param name="versionString">The version string.</param>
        /// <exception cref="System.ArgumentException">
        ///     @The version string must contain 4 numbers separated by 3 dots like
        ///     1.0.232.9594;versionString
        /// </exception>
        public AppVersion(string versionString)
        {
            var versionParts = versionString.Split('.');
            if (versionParts.Length != 4)
            {
                throw new ArgumentException(
                    @"The version string must contain 4 numbers separated by 3 dots like 1.0.232.9594",
                    nameof(versionString));
            }

            if (int.TryParse(versionParts[0], out var majorVersion))
            {
                MajorVersion = majorVersion;
            }
            else
            {
                throw new ArgumentException(@"First part of version string (=MajorVersion) is no number",
                    nameof(versionString));
            }

            if (int.TryParse(versionParts[1], out var minorVersion))
            {
                MinorVersion = minorVersion;
            }
            else
            {
                throw new ArgumentException(@"Second part of version string (=MinorVersion) is no number",
                    nameof(versionString));
            }

            if (int.TryParse(versionParts[2], out var buildNumber))
            {
                BuildNumber = buildNumber;
            }
            else
            {
                throw new ArgumentException(@"Third part of version string (=BuildNumber) is no number",
                    nameof(versionString));
            }

            if (int.TryParse(versionParts[3], out var revisionNumber))
            {
                RevisionNumber = revisionNumber;
            }
            else
            {
                throw new ArgumentException(@"Fourt part of version string (=revisionNumber) is no number",
                    nameof(versionString));
            }

            BuildTime =
                TimeZoneInfo.ConvertTimeToUtc(new DateTime(2000, 1, 1))
                    .AddDays(Convert.ToDouble(buildNumber))
                    .AddSeconds(Convert.ToDouble(revisionNumber) * 2)
                    .ToLocalTime();
        }

        /// <summary>
        ///     Gets the build number.
        /// </summary>
        /// <value>
        ///     The build number.
        /// </value>
        public int BuildNumber { get; }

        /// <summary>
        ///     Gets the build time.
        /// </summary>
        /// <value>
        ///     The build time.
        /// </value>
        public DateTime BuildTime { get; }

        /// <summary>
        ///     Gets the major version.
        /// </summary>
        /// <value>
        ///     The major version.
        /// </value>
        public int MajorVersion { get; }

        /// <summary>
        ///     Gets the minor version.
        /// </summary>
        /// <value>
        ///     The minor version.
        /// </value>
        public int MinorVersion { get; }

        /// <summary>
        ///     Gets the revision number.
        /// </summary>
        /// <value>
        ///     The revision number.
        /// </value>
        public int RevisionNumber { get; }

        /// <summary>
        ///     Casts the specified version string.
        /// </summary>
        /// <param name="versionString">The version string.</param>
        /// <returns></returns>
        public static AppVersion Cast(string versionString)
        {
            return new AppVersion(versionString);
        }

        /// <summary>
        ///     Converts the version to string like {MajorVersion}.{MinorVersion}.{BuildNumber}.{RevisionNumber} ({BuildTime})
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}.{BuildNumber}.{RevisionNumber} ({BuildTime})";
        }
    }
}