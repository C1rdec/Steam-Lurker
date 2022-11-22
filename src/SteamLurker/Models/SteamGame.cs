using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FuzzySharp;
using Lurker.Common.Extensions;
using Lurker.Common.Models;

namespace Lurker.Steam.Models
{
    public class SteamGame : GameBase
    {
        #region Fields

        private static readonly Dictionary<string, string> Alias = new() 
        { 
            { "1938090", "cod" },
            { "488310", "pong" },
        };
        private string _steamExe;
        private string _acfFilePath;
        private string _id;

        #endregion

        #region Constructors

        public SteamGame(string acfFilePath, string steamExe)
        {
            _steamExe = steamExe;
            _acfFilePath = acfFilePath;
        }

        #endregion

        #region Properties

        public override string Id => _id;

        public override LauncherType Launcher => LauncherType.Steam;

        #endregion

        #region Methods

        public override Task Open()
            => CliWrap.Cli.Wrap(_steamExe).WithArguments($"steam://rungameid/{Id}/").ExecuteAsync();

        public override void Initialize()
        {
            var text = File.ReadAllText(_acfFilePath);

            _id = text.GetLineAfter("\"appid\"	").Replace("\"", string.Empty);
            Name = text.GetLineAfter("\"name\"	").Replace("\"", string.Empty);

            var installationFolder = text.GetLineAfter("\"installdir\"	").Replace("\"", string.Empty);

            var gameFolder = Path.Combine(Path.GetDirectoryName(_acfFilePath), "common", installationFolder);
            var exeFiles = new DirectoryInfo(gameFolder).GetFiles($"*.exe", SearchOption.AllDirectories);

            var searchTerm = Name;
            if (Alias.TryGetValue(Id, out var alias))
            {
                searchTerm = alias;
            }

            var matches = exeFiles.Select(e => new
            {
                FilePath = e.FullName,
                Ratio = Fuzz.Ratio(e.Name.Replace(".exe", string.Empty).ToLower(), searchTerm.ToLower())
            });

            var bestmatch = matches.MaxBy(r => r.Ratio);
            ExeFilePath = bestmatch.FilePath;
        }

        #endregion
    }
}
