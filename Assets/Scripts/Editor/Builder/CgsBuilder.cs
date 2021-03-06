using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace EditorBuilder
{
    public class CgsBuilder : MonoBehaviour
    {
        private static readonly string Eol = Environment.NewLine;
        private static readonly string[] Secrets = {"androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass"};

        [UsedImplicitly]
        public static void Build()
        {
            // Gather values from args
            var options = GetValidatedOptions();
            
            LogPhotonNetwork("Before Setting Photon Server Settings");

            // Set version for this build
            Console.WriteLine($"PlayerSettings.bundleVersion: {PlayerSettings.bundleVersion}");
            Console.WriteLine($"PlayerSettings.androidVersionCode: {PlayerSettings.Android.bundleVersionCode}");
            Console.WriteLine($"PlayerSettings.iOSBuildNumber: {PlayerSettings.iOS.buildNumber}");

            //var version = PlayerSettings.bundleVersion;

            //PlayerSettings.bundleVersion = version;
            //PlayerSettings.Android.bundleVersionCode = int.Parse(options["androidVersionCode"]);

            // Apply build target
            var buildTarget = (BuildTarget) Enum.Parse(typeof(BuildTarget), options["buildTarget"]);
            
            switch (buildTarget)
            {
                case BuildTarget.Android:
                {
                    EditorUserBuildSettings.buildAppBundle = options["customBuildPath"].EndsWith(".aab");
                    if (options.TryGetValue("androidKeystoreName", out string keystoreName) &&
                        !string.IsNullOrEmpty(keystoreName))
                        PlayerSettings.Android.keystoreName = keystoreName;
                    if (options.TryGetValue("androidKeystorePass", out string keystorePass) &&
                        !string.IsNullOrEmpty(keystorePass))
                        PlayerSettings.Android.keystorePass = keystorePass;
                    if (options.TryGetValue("androidKeyaliasName", out string keyaliasName) &&
                        !string.IsNullOrEmpty(keyaliasName))
                        PlayerSettings.Android.keyaliasName = keyaliasName;
                    if (options.TryGetValue("androidKeyaliasPass", out string keyaliasPass) &&
                        !string.IsNullOrEmpty(keyaliasPass))
                        PlayerSettings.Android.keyaliasPass = keyaliasPass;
                    break;
                }
                case BuildTarget.iOS:
                    break;
            }

            // Custom build
            BuildPhotonNetwork();
            LogPhotonNetwork("After Setting Photon Server Settings");
            Build(buildTarget, options["customBuildPath"]);
        }

        private static void LogPhotonNetwork(string title)
        {
            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#     {title}     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
            );
            
            // show photon id
            Console.WriteLine($"Photon HostType: {PhotonNetwork.PhotonServerSettings.HostType}");
            Console.WriteLine($"Photon Enabled Regions: {PhotonNetwork.PhotonServerSettings.EnabledRegions}");
            Console.WriteLine($"Photon Preferred Region: {PhotonNetwork.PhotonServerSettings.PreferredRegion}");
            Console.WriteLine($"Photon ID: {PhotonNetwork.PhotonServerSettings.AppID}");
            Console.WriteLine($"Photon Protocol: {PhotonNetwork.PhotonServerSettings.Protocol}");
            Console.WriteLine($"Photon PunLogging: {PhotonNetwork.PhotonServerSettings.PunLogging}");
            Console.WriteLine($"Photon NetworkLogging: {PhotonNetwork.PhotonServerSettings.NetworkLogging}");
            Console.WriteLine($"Photon RunInBackground: {PhotonNetwork.PhotonServerSettings.RunInBackground}");
            Console.WriteLine($"Photon RpcList: {PhotonNetwork.PhotonServerSettings.RpcList.Count}");
            
            foreach (var rpc in PhotonNetwork.PhotonServerSettings.RpcList)
            {
                Console.WriteLine($"Photon RpcItem: {rpc}");
            }
        }

        private static void BuildPhotonNetwork()
        {
            PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.PhotonCloud;
            PhotonNetwork.PhotonServerSettings.PreferredRegion = CloudRegionCode.jp;
            PhotonNetwork.PhotonServerSettings.AppID = "5a982d5a-00e0-447d-a2d6-6b0d8570c340";
            PhotonNetwork.PhotonServerSettings.Protocol = ConnectionProtocol.Udp;

            PhotonNetwork.PhotonServerSettings.PunLogging = PhotonLogLevel.ErrorsOnly;
            PhotonNetwork.PhotonServerSettings.NetworkLogging = DebugLevel.ERROR;

            PhotonNetwork.PhotonServerSettings.RunInBackground = true;

            PhotonNetwork.PhotonServerSettings.RpcList = new List<string>()
            {
                "Chat",
                "ColorRpc",
                "DestroyRpc",
                "DoJump",
                "Flash",
                "InstantiateRpc",
                "Marco",
                "PickupItemInit",
                "Polo",
                "PunPickup",
                "PunPickupSimple",
                "PunRespawn",
                "RequestForPickupItems",
                "RequestForPickupTimes",
                "TaggedPlayer"
            };
        }

        private static Dictionary<string, string> GetValidatedOptions()
        {
            ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);

            if (!validatedOptions.TryGetValue("projectPath", out string _))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue("buildTarget", out string buildTarget))
            {
                Console.WriteLine("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }

            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
            {
                EditorApplication.Exit(121);
            }

            if (!validatedOptions.TryGetValue("customBuildPath", out string _))
            {
                Console.WriteLine("Missing argument -customBuildPath");
                EditorApplication.Exit(130);
            }

            const string defaultCustomBuildName = "TestBuild";
            
            if (!validatedOptions.TryGetValue("customBuildName", out string customBuildName))
            {
                Console.WriteLine($"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }
            else if (customBuildName == "")
            {
                Console.WriteLine($"Invalid argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }

            return validatedOptions;
        }

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#    Parsing settings     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                string flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";
                bool secret = Secrets.Contains(flag);
                string displayValue = secret ? "*HIDDEN*" : "\"" + value + "\"";

                // Assign
                Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
                providedArguments.Add(flag, value);
            }
        }
        
        private static void Build(BuildTarget buildTarget, string filePath)
        {
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                target = buildTarget,
                locationPathName = filePath
            };

            BuildSummary buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;
            
            ReportSummary(buildSummary);
            ExitWithResult(buildSummary.result);
        }

        private static void ReportSummary(BuildSummary summary)
        {
            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#      Build results      #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}" +
                $"Duration: {summary.totalTime.ToString()}{Eol}" +
                $"Warnings: {summary.totalWarnings.ToString()}{Eol}" +
                $"Errors: {summary.totalErrors.ToString()}{Eol}" +
                $"Size: {summary.totalSize.ToString()} bytes{Eol}" +
                $"{Eol}"
            );
        }

        private static void ExitWithResult(BuildResult result)
        {
            switch (result)
            {
                case BuildResult.Succeeded:
                    Console.WriteLine("Build succeeded!");
                    EditorApplication.Exit(0);
                    break;
                case BuildResult.Failed:
                    Console.WriteLine("Build failed!");
                    EditorApplication.Exit(101);
                    break;
                case BuildResult.Cancelled:
                    Console.WriteLine("Build cancelled!");
                    EditorApplication.Exit(102);
                    break;
                case BuildResult.Unknown:
                default:
                    Console.WriteLine("Build result is unknown!");
                    EditorApplication.Exit(103);
                    break;
            }
        }
    }
}