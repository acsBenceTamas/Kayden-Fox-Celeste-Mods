using Celeste;
using Celeste.Mod;
using Monocle;
using System.Collections.Generic;
using System.IO;

namespace KaydenCommands.Entities
{
    [Tracked(false)]
    class PlaybackRecorder : Entity
    {
        public string Filename;

        private Player _player;
        private List<Player.ChaserState> _states = new List<Player.ChaserState>();
        private bool _isRecording;

        public PlaybackRecorder(Player player, string path)
        {
            _player = player;
            Filename = path;
        }

        public override void Update()
        {
            base.Update();
            if (_isRecording && _player != null && !_player.Dead)
            {
                Player.ChaserState state = new Player.ChaserState( _player )
                {
                    TimeStamp = Scene.RawTimeActive
                };
                _states.Add( state );
            }
        }

        public void StartRecording()
        {
            _states.Clear();
            _isRecording = true;
        }

        public void StopRecording()
        {
            _isRecording = false;
            DirectoryInfo exportDirectory = new DirectoryInfo(Path.Combine(Everest.Content.PathContentOrig, "Tutorials/CustomPlaybacks"));
            if (!exportDirectory.Exists)
            {
                exportDirectory.Create();
            }
            PlaybackData.Export(_states, Path.Combine(Everest.Content.PathContentOrig, "Tutorials/CustomPlaybacks", Filename + ".bin"));
        }
    }
}
