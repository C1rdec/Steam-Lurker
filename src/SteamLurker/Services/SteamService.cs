using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private string _steamExecutable;

        #endregion

        #region Properties

        private string RootSteamFolderPath => Path.GetDirectoryName(_steamExecutable);

        private string SteamAppsFolderPath => Path.Combine(RootSteamFolderPath, "steamapps");

        #endregion

        #region Methods

        public List<SteamGame> FindSteamGames()
        {
            if (string.IsNullOrEmpty(_steamExecutable))
            {
                throw new InvalidOperationException("Must be initialize");
            }

            var games = new List<SteamGame>();
            var acfFiles = Directory.GetFiles(SteamAppsFolderPath, "*.acf");
            foreach (var file in acfFiles)
            {
                games.Add(new SteamGame(file, _steamExecutable));
            }

            return games;
        }


        public async Task InitializeAsync()
        {
            var runningSteamProcess = Process.GetProcessesByName(ProcessName);
            if (runningSteamProcess.Any())
            {
                _steamExecutable = runningSteamProcess[0].GetMainModuleFileName();

                return;
            }

            var arguments = @".\Resources\OpenSteamLink.url";

            var command = CliWrap.Cli
                            .Wrap("cmd.exe")
                            .WithArguments($"/C {arguments}");
            await command.ExecuteAsync();

            var processService = new ProcessService(ProcessName);
            var processId = await processService.WaitForProcess(false);
            var process = Process.GetProcessById(processId);
            _steamExecutable = process.GetMainModuleFileName();

            return;
        }

        #endregion
    }
}
