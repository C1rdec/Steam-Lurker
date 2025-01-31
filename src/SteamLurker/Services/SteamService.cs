﻿using System.Collections.Generic;
using System.IO;
using Lurker.Common.Extensions;
using Lurker.Common.Services;
using Lurker.Steam.Models;

namespace Lurker.Steam.Services;

public class SteamService : ServiceBase<SteamGame>
{
    #region Fields

    private static readonly string SteamApps = "steamapps";
    private static List<string> SteamToolIds = new() { "250820", "228980" };

    #endregion

    #region Properties

    protected override string ProcessName => "steam";

    protected override string OpenUrl => @"steam://nav/games";

    private string RootSteamFolderPath => Path.GetDirectoryName(ExecutablePath);

    private string SteamAppsFolderPath => Path.Combine(RootSteamFolderPath, SteamApps);

    private string LibraryFoldersFile => Path.Combine(SteamAppsFolderPath, "libraryfolders.vdf");

    private string UserConfigFilePath => Path.Combine(RootSteamFolderPath, "config", "loginusers.vdf");

    #endregion

    #region Methods

    public string FindUsername()
    {
        var userConfigFilePath = UserConfigFilePath;
        if (File.Exists(userConfigFilePath))
        {
            var text = File.ReadAllText(userConfigFilePath);
            var searchTerm = "\"PersonaName\"\t\t";

            var personaName = text.GetLineAfter(searchTerm);

            return personaName.Substring(1, personaName.Length - 2);
        }

        return string.Empty;
    }

    public override List<SteamGame> FindGames()
    {
        var games = new List<SteamGame>();
        if (string.IsNullOrEmpty(ExecutablePath))
        {
            return games;
        }

        AddGames(SteamAppsFolderPath, games);

        if (File.Exists(LibraryFoldersFile))
        {
            var text = File.ReadAllText(LibraryFoldersFile);
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

            // SteamVR
            // Steamworks Common Redistributables
            if (SteamToolIds.Contains(game.Id))
            {
                continue;
            }

            games.Add(game);
        }
    }

    #endregion
}
