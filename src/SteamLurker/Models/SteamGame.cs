using System.IO;
using SteamLurker.Extensions;

namespace SteamLurker.Models
{
    public class SteamGame
    {
        #region Fields

        private string _folderPath;
        private FileInfo[] _exeFilePath;

        #endregion

        #region Constructors

        public SteamGame(string acfFilePath)
        {
            var text = File.ReadAllText(acfFilePath);

            Id = text.GetLineAfter("\"appid\"	").Replace("\"", string.Empty);
            Name = text.GetLineAfter("\"name\"	").Replace("\"", string.Empty);

            var installationFolder = text.GetLineAfter("\"installdir\"	").Replace("\"", string.Empty);

            _folderPath = Path.Combine(Path.GetDirectoryName(acfFilePath), "common", installationFolder);
            _exeFilePath = new DirectoryInfo(_folderPath).GetFiles($"*.exe", SearchOption.AllDirectories);
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public string Id { get; set; }

        #endregion
    }
}
