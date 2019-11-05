using Celeste;
using KaydenCommands.Entities;
using Monocle;

namespace KaydenCommands
{
    public class RecordCommand
    {
        [Command("start_rec", "Starts capturing the player's movement. To stop recording use the stop_rec command.")]
        private static void CmdStartCapture(string filename="latestCustomRecording")
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                PlaybackRecorder recorder = Engine.Scene.Tracker.GetEntity<PlaybackRecorder>();
                if (recorder == null)
                {
                    recorder = new PlaybackRecorder(player, filename);
                    Engine.Scene.Add(recorder);
                }
                else
                {
                    recorder.Filename = filename;
                }
                recorder.StartRecording();
            }
            Engine.Commands.Log("Started recording playback.");
        }

        [Command("stop_rec", "Stops the current recording started with start_rec and saves recording.")]
        private static void CmdStopCapture()
        {
            PlaybackRecorder recorder = Engine.Scene.Tracker.GetEntity<PlaybackRecorder>();
            if (recorder != null)
            {
                recorder.StopRecording();
            }
            Engine.Commands.Log($"Saved playback as {recorder.Filename}.bin");
        }
    }
}
