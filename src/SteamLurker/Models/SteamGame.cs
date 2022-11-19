using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FuzzySharp;
using SteamLurker.Extensions;

namespace SteamLurker.Models
{
    public class SteamGame
    {
        #region Fields

        private string _steamExe;
        private string _folderPath;
        private string _exeFilePath;

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
            var exeFiles = new DirectoryInfo(_folderPath).GetFiles($"*.exe", SearchOption.AllDirectories);

            var matches = exeFiles.Select(e => new 
            { 
                FilePath = e.FullName,
                Ratio = Fuzz.Ratio(e.Name.Replace(".exe", string.Empty).ToLower(), Name.ToLower()) 
            });

            var bestmatch = matches.MaxBy(r => r.Ratio);
            _exeFilePath = bestmatch.FilePath;
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
                result = Icon.ExtractAssociatedIcon(_exeFilePath);
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
