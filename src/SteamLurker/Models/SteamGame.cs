using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lurker.Common.Extensions;
using Lurker.Common.Models;

namespace Lurker.Steam.Models
{
    public class SteamGame : GameBase
    {
        #region Fields

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

        public override Dictionary<string, string> Alias => new()
        {
            { "1938090", "cod" },
            { "488310", "pong" },
        };

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
            
            SetExeFile(gameFolder);
        }

        #endregion
    }
}
