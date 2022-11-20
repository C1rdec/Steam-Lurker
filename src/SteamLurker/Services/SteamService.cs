using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProcessLurker;
using SteamLurker.Extensions;
using SteamLurker.Models;

namespace SteamLurker.Services
{
    public class SteamService
    {
        #region Fields

        private static readonly string ProcessName = "steam";
        private static readonly string SteamApps = "steamapps";
        private string _steamExecutable;
        private static List<string> SteamToolIds = new() { "250820", "228980" };

        #endregion

        #region Properties

        private string RootSteamFolderPath => Path.GetDirectoryName(_steamExecutable);

        private string SteamAppsFolderPath => Path.Combine(RootSteamFolderPath, SteamApps);

        private string libraryFoldersFile => Path.Combine(SteamAppsFolderPath, "libraryfolders.vdf");

        #endregion

        #region Methods

        public async Task<string> InitializeAsync(string steamExe = null)
        {
            if (!string.IsNullOrEmpty(steamExe) && File.Exists(steamExe))
            {
                _steamExecutable = steamExe;
            }
            else
            {
                var runningSteamProcess = Process.GetProcessesByName(ProcessName);
                if (runningSteamProcess.Any())
                {
                    _steamExecutable = runningSteamProcess[0].GetMainModuleFileName();
                }
                else
                {
                    // Launch Steam to get the Process
                    var arguments = @".\steam.lurker\OpenSteamLink.url";

                    var command = CliWrap.Cli
                                    .Wrap("cmd.exe")
                                    .WithArguments($"/C {arguments}");
                    await command.ExecuteAsync();

                    var processService = new ProcessService(ProcessName);
                    var processId = await processService.WaitForProcess(false);
                    var process = Process.GetProcessById(processId);
                    _steamExecutable = process.GetMainModuleFileName();
                }
            }

            return _steamExecutable;
        }

        public List<SteamGame> FindGames()
        {
            if (string.IsNullOrEmpty(_steamExecutable))
            {
                throw new InvalidOperationException("Must be initialize");
            }

            var games = new List<SteamGame>();
            AddGames(SteamAppsFolderPath, games);

            if (File.Exists(libraryFoldersFile))
            {
                var text = File.ReadAllText(libraryFoldersFile);
                var index = 0;
                var searchTerm = "\"path\"\t\t";
                do
                {
                    var folderPath = text
                        .GetLineAfter(searchTerm)
                        .Replace("\"", string.Empty)
                        .Replace("\\\\", "\\");

                    if (string.IsNullOrEmpty(folderPath))
                    {
                        return games;
                    }

                    var steamApps = Path.Combine(folderPath, SteamApps);

                    if (steamApps != SteamAppsFolderPath)
                    {
                        AddGames(steamApps, games);
                    }

                    index = text.IndexOf(searchTerm);
                    if (index != -1)
                    {
                        text = text.Substring(index + searchTerm.Length);
                    }
                } while (index != -1);
            }

            return games;
        }

        private void AddGames(string folder, List<SteamGame> games)
        {
            var acfFiles = Directory.GetFiles(folder, "*.acf");
            foreach (var filePath in acfFiles)
            {
                var game = new SteamGame(filePath, _steamExecutable);

                // Steamworks Common Redistributables
                // SteamVR
                if (SteamToolIds.Contains(game.Id))
                {
                    continue;
                }

                games.Add(game);
            }
        }

        #endregion
    }
}
