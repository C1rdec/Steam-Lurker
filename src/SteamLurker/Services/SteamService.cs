using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using SteamLurker.Extensions;
using SteamLurker.Models;

namespace SteamLurker.Services
{
    public class SteamService
    {
        #region Fields

        private string _steamExecutable;
        private List<SteamGame> _games;

        #endregion

        #region Constructors

        public SteamService()
        {
            _steamExecutable = FindSteamExe();
            _games = FindSteamGames();
        }

        #endregion

        #region Properties

        public string RootSteamFolderPath => Path.GetDirectoryName(_steamExecutable);

        public string SteamAppsFolderPath => Path.Combine(RootSteamFolderPath, "steamapps");

        #endregion

        #region Methods

        public object GetIcon()
        {
            var icon = Icon.ExtractAssociatedIcon(@"C:\Program Files (x86)\Steam\steamapps\common\Among Us\Among Us.exe");
            return icon.ToBitmap();
        }

        private string FindSteamExe()
        {
            var runningSteamProcess = Process.GetProcessesByName("steam");
            if (runningSteamProcess.Length > 0)
            {
                return runningSteamProcess[0].GetMainModuleFileName();
            }

            // Check Program Files
            return "";
        }

        private List<SteamGame> FindSteamGames()
        {
            var games = new List<SteamGame>();
            var acfFiles = Directory.GetFiles(SteamAppsFolderPath, "*.acf");
            foreach(var file in acfFiles)
            {
                games.Add(new SteamGame(file));
            }

            return games;
        }

        public static Icon IconFromFilePath(string filePath)
        {
            var result = (Icon)null;

            try
            {
                result = Icon.ExtractAssociatedIcon(filePath);
            }
            catch (System.Exception)
            {
                // swallow and return nothing. You could supply a default Icon here as well
            }

            return result;
        }

        #endregion
    }
}
