using System;
using System.Collections.Generic;
using System.IO;
using Lurker.Common.Extensions;
using Lurker.Common.Services;
using Lurker.Steam.Models;

namespace Lurker.Steam.Services
{
    public class SteamService : ServiceBase<SteamGame>
    {
        #region Fields

        private static readonly string SteamApps = "steamapps";
        private static List<string> SteamToolIds = new() { "250820", "228980" };

        #endregion

        #region Properties

        protected override string ProcessName => "steam";

        protected override string OpenLink => throw new NotImplementedException();

        private string RootSteamFolderPath => Path.GetDirectoryName(ExecutablePath);

        private string SteamAppsFolderPath => Path.Combine(RootSteamFolderPath, SteamApps);

        private string libraryFoldersFile => Path.Combine(SteamAppsFolderPath, "libraryfolders.vdf");

        #endregion

        #region Methods

        public override List<SteamGame> FindGames()
        {
            if (string.IsNullOrEmpty(ExecutablePath))
            {
                throw new InvalidOperationException("Must be initialize");
            }

            var games = new List<SteamGame>();
            AddGames(SteamAppsFolderPath, games);

            if (File.Exists(libraryFoldersFile))
            {
                var text = File.ReadAllText(libraryFoldersFile);
                var searchTerm = "\"path\"\t\t";
                int index;

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
                var game = new SteamGame(filePath, ExecutablePath);
                game.Initialize();

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
