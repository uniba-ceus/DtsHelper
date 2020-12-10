// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net;
    using PoorMansTSqlFormatterLib;
    using PoorMansTSqlFormatterLib.Formatters;

    /// <summary>
    ///     Collections of useful functions for files..
    /// </summary>
    public static class FileUtils
    {
        private static readonly Encoding _destEncoding = new UTF8Encoding(true);
        private static readonly ILog _log = LogManager.GetLogger(typeof(FileUtils));
        private const string FormatterWarningMsg = "--WARNING!ERRORS ENCOUNTERED DURING SQL PARSING!\r\n";

        /// <summary>
        ///     Converts encoding of file to UTF8.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public static void ConvertFileEncodingToUTF8(string fileName)
        {
            if (!File.Exists(fileName))
            {
                _log.Error($"Die Datei '{fileName}' existiert nicht.");
            }
            else
            {
                var srcEncoding = GetFileEncoding(fileName);

                // convert only in case code page is different or BOM does not exist.
                if (srcEncoding.CodePage != _destEncoding.CodePage ||
                    srcEncoding.CodePage == _destEncoding.CodePage && !srcEncoding.GetPreamble().Any())
                {
                    var codeBytes = File.ReadAllBytes(fileName);
                    var utf8Bytes = Encoding.Convert(srcEncoding, _destEncoding, codeBytes);
                    var utf8String = Encoding.UTF8.GetString(utf8Bytes);

                    _log.Info($"Konvertiere '{fileName}' von {srcEncoding.GetInfoString()} zu {_destEncoding.GetInfoString()}");

                    File.WriteAllText(fileName, utf8String, _destEncoding);
                }
                else
                {
                    _log.Debug($"Keine Konvertierung der Datei '{fileName}' zu {_destEncoding.GetInfoString()} notwendig.");
                }
            }
        }

        /// <summary>
        ///     Formats the SQL file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="formatOptions">The format options.</param>
        public static void FormatSqlFile(string fileName, TSqlStandardFormatterOptions formatOptions)
        {
            if (!File.Exists(fileName))
            {
                _log.Error($"Die Datei '{fileName}' existiert nicht.");
            }
            else
            {
                var standardFormatter = new TSqlStandardFormatter(formatOptions);
                var sqlFormatManager = new SqlFormattingManager
                {
                    Formatter = standardFormatter
                };

                _log.Info($"Formatiere SQL der Datei '{fileName}' und speichere als {_destEncoding.GetInfoString()}.");
                var sql = File.ReadAllText(fileName);
                var errorsEncountered = false;
                sql = sqlFormatManager.Format(sql, ref errorsEncountered);

                if (errorsEncountered)
                {
                    _log.Warn("Es sind Fehler während der Formatierung aufgetreten!");
                    if (sql.StartsWith(FormatterWarningMsg))
                    {
                        sql = sql.Replace(FormatterWarningMsg, "");
                    }
                }

                File.WriteAllText(fileName, sql, _destEncoding);
            }
        }

        /// <summary>
        ///     Gets the file encoding.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Encoding of file.</returns>
        public static Encoding GetFileEncoding(string fileName)
        {
            Trace.TraceInformation("Call: GetFileEncoding");

            // ASCII bzw. ANSI als Standard-Encoding.
            // Codepage 1252 ANSI Latin 1; Western European (Windows).
            var enc = Encoding.Default;

            // read first 3 bytes (Byte Order Mark)
            var bom = new byte[4];
            using (var file = new FileStream(fileName, FileMode.Open))
            {
                file.Read(bom, 0, 4);
            }

            if (bom[0] == 0x2B && bom[1] == 0x2F && bom[2] == 0x76)
            {
                // Codepage 65000 Unicode UTF-7 mit Byte Order Mark.
                enc = Encoding.UTF7;
            }
            else if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
            {
                // Codepage 65001 Unicode UTF-8 mit Byte Order Mark.
                enc = new UTF8Encoding(true);
            }
            else if (bom[0] == 0xFF && bom[1] == 0xFE)
            {
                // Codepage 1200 Unicode UTF-16, Little Endian Byte Order Mark.
                enc = Encoding.GetEncoding(1200);
            }
            else if (bom[0] == 0xFE && bom[1] == 0xFF)
            {
                // Codepage 1201 Unicode UTF-16, Big Endian Byte Order Mark.
                enc = Encoding.BigEndianUnicode;
            }
            else if (bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0 && bom[3] == 0)
            {
                // Codepage 12000 Unicode UTF-32, Little Endian Byte Order Mark.
                enc = Encoding.UTF32;
            }
            else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xFE && bom[3] == 0xFF)
            {
                // Codepage 12001 Unicode UTF-32, Big Endian Byte Order Mark.
                enc = Encoding.UTF32;
            }
            else if (IsUTF8NoBOM(fileName))
            {
                // NOTE: ANSI/ASCII and UTF-8 without BOM is hard to distinguish

                // Codepage 65001 Unicode UTF-8 without BOM.
                enc = new UTF8Encoding(false);
            }

            return enc;
        }

        /// <summary>
        ///     Info about BOM encoding of file.
        /// </summary>
        public static string GetInfoString(this Encoding enc)
        {
            return $"{enc.EncodingName} ({enc.CodePage}){(enc.GetPreamble().Any() ? " with BOM" : " without BOM")}";
        }

        /// <summary>
        ///     Checks whether file is UTF without BOM.
        /// </summary>
        /// <param name="fileName">The full name of file inclusive path.</param>
        /// <returns>True in case the file is has encoding UTF8 without BOM.</returns>
        public static bool IsUTF8NoBOM(string fileName)
        {
            Trace.TraceInformation("Call: IsUTF8NoBOM");

            var isUTF8NoBOM = false;
            string textANSI;

            // read as ANSI with default encoding
            using (var streamReader = new StreamReader(fileName, Encoding.Default, false))
            {
                textANSI = streamReader.ReadToEnd();
            }

            // check file is UTF-8 without BOM.
            if (textANSI.Contains("Ã") || textANSI.Contains("±"))
            {
                isUTF8NoBOM = true;
            }

            return isUTF8NoBOM;
        }

        /// <summary>
        ///     Variables are not allowed to have hyphen, blanks or brackets. First character must be a letter or underscore.
        /// </summary>
        /// <param name="taskName">Name of the Task.</param>
        /// <returns>The masked task name</returns>
        public static string MaskTaskName(string taskName)
        {
            Trace.TraceInformation("Call: MaskTaskName");

            taskName = taskName.Replace(" ", "_")
                .Replace("-", "_")
                .Replace("(", "_")
                .Replace(")", "_");

            var regex = new Regex("[0-9]", RegexOptions.IgnoreCase);

            if (regex.Match(taskName.Substring(0, 1)).Success)
            {
                taskName = "_" + taskName;
            }

            return taskName;
        }

        /// <summary>
        ///     Prints all entries of dictionary.
        /// </summary>
        public static void PrintDictionary(Dictionary<string, string> sqlFiles)
        {
            Trace.TraceInformation("Call: PrintDictionary");

            foreach (var sqlFile in sqlFiles)
            {
                Console.WriteLine("====================");
                Console.WriteLine("Sql-Script-Name: '" + sqlFile.Key + "'");
                Console.WriteLine("Sql-Code:");
                Console.WriteLine(sqlFile.Value);
            }
        }
    }
}