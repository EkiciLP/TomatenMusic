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
        
        public Queue<MultiTrack> Queue { get; private set; } = new Queue<MultiTrack>();
        public Queue<MultiTrack> PlayedTracks { get; private set; } = new Queue<MultiTrack>();


        public LavalinkPlaylist CurrentPlaylist { get; private set; }

        public LoopType LoopType { get; private set; } = LoopType.NONE;

        public MultiTrack LastTrack { get; private set; }

        public List<MultiTrack> QueueLoopList { get; private set; }

        public MusicActionResponseType QueueTrack(MultiTrack track)
        {
            CurrentPlaylist = null;
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
                foreach (MultiTrack track in playlist.Tracks)
                    Queue.Enqueue(track);
                return MusicActionResponseType.SUCCESS;
            });

        }

        public async Task<MusicActionResponseType> QueueTracksAsync(List<MultiTrack> tracks)
        {
            return await Task.Run(() =>
            {
                CurrentPlaylist = null;
                Program.Discord.Logger.LogInformation("Queued TrackList {0}", tracks.ToString());
                foreach (MultiTrack track in tracks)
                    QueueTrack(track);
                return MusicActionResponseType.SUCCESS;
            });

        }

        public MusicActionResponseType Clear()
        {
            if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;
            Queue.Clear();
            PlayedTracks.Clear();
            return MusicActionResponseType.SUCCESS;
        }

        public MusicActionResponseType Remove(string uri)
        {
            if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;
            Queue<MultiTrack> newQueue = new Queue<MultiTrack>();

            foreach (var item in Queue)
            {
                if (item.Uri.ToString() != uri)
                    newQueue.Enqueue(item);
            }
            Queue = newQueue;
            return MusicActionResponseType.SUCCESS;

        }

        public MusicActionResponseType Reset()
        {
            if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;

            Queue.Clear();
            return MusicActionResponseType.SUCCESS;
        }

        public MusicActionResponse NextTrack()
        {
            PlayedTracks = new Queue<MultiTrack>(PlayedTracks.Prepend(LastTrack));

            switch (LoopType)
            {
                case LoopType.NONE:
                    if (Queue.Count == 0) return new MusicActionResponse(MusicActionResponseType.QUEUE_EMPTY);

                    LastTrack = Queue.Dequeue();

                    return new MusicActionResponse(MusicActionResponseType.SUCCESS, LastTrack);
                case LoopType.TRACK:

                    if (LastTrack == null) return new MusicActionResponse(MusicActionResponseType.FAIL, message: "An unexpected error occured, LastTrack was Null");

                    return new MusicActionResponse(MusicActionResponseType.SUCCESS, LastTrack);
                case LoopType.QUEUE:
                    if (!Queue.Any())
                        Queue = new Queue<MultiTrack>(QueueLoopList);

                    LastTrack = Queue.Dequeue();

                    return new MusicActionResponse(MusicActionResponseType.SUCCESS, LastTrack);
                default:
                    return new MusicActionResponse(MusicActionResponseType.FAIL, message: "An unexpected error occured, LoopType was Null");
            }
        }

        public MusicActionResponse Rewind()
        {
            Program.Discord.Logger.LogDebug(PlayedTracks.ToString());
            Program.Discord.Logger.LogDebug(PlayedTracks.Count.ToString());

            if (!PlayedTracks.Any()) return new MusicActionResponse(MusicActionResponseType.QUEUE_EMPTY);

            Queue = new Queue<MultiTrack>(Queue.Prepend(LastTrack));
            LastTrack = PlayedTracks.Dequeue();
            List<MultiTrack> tracks = new List<MultiTrack>(Queue);
            tracks.Remove(tracks.Where( track => track.IsQueueLoopItem && track.YoutubeIdentifier == LastTrack.YoutubeIdentifier).FirstOrDefault());
            Queue = new Queue<MultiTrack>(tracks);

            return new MusicActionResponse(MusicActionResponseType.SUCCESS, LastTrack);
        }

        public async Task<MusicActionResponseType> ShuffleAsync()
        {
            return await Task.Run(() =>
           {
               if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;

               List<MultiTrack> tracks = new List<MultiTrack>(Queue);
               tracks.Shuffle();
               Queue = new Queue<MultiTrack>(tracks);

               return MusicActionResponseType.SUCCESS;
           });
        }

        public async Task<MusicActionResponseType> SetLoopAsync(LoopType type)
        {
            LoopType = type;

            if (type == LoopType.QUEUE)
            {
                QueueLoopList = new List<MultiTrack>(Queue);
                QueueLoopList.Add(LastTrack);
            }

            return MusicActionResponseType.SUCCESS;
        }

        public string GetQueueString()
        {
            StringBuilder builder = new StringBuilder();
            int count = 1;
            foreach (MultiTrack track in Queue)
            {

                if (count > 15)
                {
                    builder.Append(String.Format("***And {0} more...***", Queue.Count() - 15));
                    break;
                }

                builder.Append(count).Append(": ").Append($"[{track.Title}]({track.Uri})").Append(" [").Append(Common.GetTimestamp(track.Length)).Append("]\n");
                count++;
            }
            return builder.ToString();
        }
    }
}
