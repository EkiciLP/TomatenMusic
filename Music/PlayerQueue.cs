using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus;
using TomatenMusic.Music.Entitites;
using System.Threading.Tasks;
using System.Linq;
using TomatenMusic.Util;
using Microsoft.Extensions.Logging;


namespace TomatenMusic.Music
{
    class PlayerQueue
    {

        public Queue<LavalinkTrack> Queue { get; private set; } = new Queue<LavalinkTrack>();

        public LavalinkPlaylist CurrentPlaylist { get; private set; }

        public bool Repeating { get; set; }

        public LavalinkTrack lastTrack { get; private set; }

        public MusicActionResponseType QueueTrack(LavalinkTrack track)
        {

            Queue.Enqueue(track);
            Program.Discord.Logger.LogInformation("Queued Track {0}", track.Title);
            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> QueuePlaylistAsync(LavalinkPlaylist playlist)
        {
            return await Task.Run(() =>
            {
                CurrentPlaylist = playlist;
                Program.Discord.Logger.LogInformation("Queued Playlist {0}", playlist.Name);
                foreach (LavalinkTrack track in playlist.Tracks)
                    QueueTrack(track);
                return MusicActionResponseType.SUCCESS;
            });

        }

        public MusicActionResponseType Clear()
        {
            if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;

            Queue.Clear();
            return MusicActionResponseType.SUCCESS;
        }

        public MusicActionResponseType Reset()
        {
            if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;

            Queue.Clear();
            Repeating = false;
            return MusicActionResponseType.SUCCESS;
        }

        public MusicActionResponse NextTrack()
        {
            if (Queue.Count == 0) return new MusicActionResponse(MusicActionResponseType.QUEUE_EMPTY);

            if (Repeating)
                lastTrack = Queue.Dequeue();

            return new MusicActionResponse(MusicActionResponseType.SUCCESS, Queue.Dequeue());

        }

        public async Task<MusicActionResponseType> Shuffle()
        {
            return await Task.Run(() =>
           {
               if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;

               List<LavalinkTrack> tracks = new List<LavalinkTrack>(Queue);
               tracks.Shuffle();
               Queue = new Queue<LavalinkTrack>(tracks);

               return MusicActionResponseType.SUCCESS;
           });
        }

        public string GetQueueString()
        {
            StringBuilder builder = new StringBuilder();
            int count = 1;
            foreach (LavalinkTrack track in Queue)
            {

                if (count > 15)
                {
                    builder.Append(String.Format("***And %s more...***", Queue.Count() - 15));
                    break;
                }

                builder.Append(count).Append(": ").Append(track.Title.Equals("Unknown title") ? track.Identifier : track.Title).Append(" [").Append(FormatUtil.GetTimestamp(track.Length)).Append("]\n");
                count++;
            }
            return builder.ToString();
        }
    }
}
