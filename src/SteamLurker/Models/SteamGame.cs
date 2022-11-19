using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SteamLurker.Extensions;

namespace SteamLurker.Models
{
    public class SteamGame
    {
        #region Fields

        private string _steamExe;
        private string _folderPath;
        private FileInfo[] _exeFilePath;

        #endregion

        #region Constructors

        public SteamGame(string acfFilePath, string steamExe)
        {
            var text = File.ReadAllText(acfFilePath);

            Id = text.GetLineAfter("\"appid\"	").Replace("\"", string.Empty);
            Name = text.GetLineAfter("\"name\"	").Replace("\"", string.Empty);

            var installationFolder = text.GetLineAfter("\"installdir\"	").Replace("\"", string.Empty);

            _steamExe = steamExe;
            _folderPath = Path.Combine(Path.GetDirectoryName(acfFilePath), "common", installationFolder);
            _exeFilePath = new DirectoryInfo(_folderPath).GetFiles($"*.exe", SearchOption.AllDirectories);
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public string Id { get; set; }

        #endregion

        #region Methods

        public Task Open()
            => CliWrap.Cli.Wrap(_steamExe).WithArguments($"steam://rungameid/{Id}/").ExecuteAsync();

        public Bitmap GetIcon()
        {
            var result = (Icon)null;

            try
            {
                var exeFile = _exeFilePath.FirstOrDefault();
                result = Icon.ExtractAssociatedIcon(exeFile.FullName);
            }
            catch (System.Exception)
            {
                // Set Default Icon
            }

            return result.ToBitmap();
        }

        #endregion
    }
}
