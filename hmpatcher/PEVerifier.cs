using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Light.Tests.Helpers
{
    public static class PEVerifier
    {
        private static readonly string PEVerifyPath = Path.Combine(
           @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools",
            "PEVerify.exe"
        );

        public static void Verify(string path)
        {
            var start = new ProcessStartInfo(PEVerifyPath, "\"" + path + "\"")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(start))
            {
                //process.WaitForExit();

                var output = process.StandardOutput.ReadToEnd();
                if (process.ExitCode != 0)
                {
                    //throw new Exception(Environment.NewLine + string.Join(Environment.NewLine, ParseVerificationResults(output, assembly).ToArray()));
                }
            }
        }

        private static IEnumerable<string> ParseVerificationResults(string output, Assembly assembly)
        {
            var lines = Regex.Matches(output, @"^\[.+", RegexOptions.Multiline).Cast<Match>().Select(m => m.Value);
            return lines.Select(l => ResolveTokens(l, assembly));
        }

        private static string ResolveTokens(string peVerifyLine, Assembly assembly)
        {
            return Regex.Replace(peVerifyLine, @"(?<!offset )0x([\da-f]+)", match =>
            {
                try
                {
                    var token = int.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
                    var member = assembly.GetModules()[0].ResolveMember(token);
                    return member.Name;
                }
                catch (Exception)
                {
                    return match.Value;
                }
            });
        }
    }
}