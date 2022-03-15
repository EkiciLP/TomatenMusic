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
            track.PlayId = RandomUtil.GenerateGuid();
            Queue.Enqueue(track);
            Program.Discord.Logger.LogInformation("Queued Track {0}", track.Title);
            if (LoopType == LoopType.QUEUE)
                QueueLoopList.Add(track);
            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> QueuePlaylistAsync(LavalinkPlaylist playlist)
        {
            return await Task.Run(() =>
            {
                if (CurrentPlaylist == null)
                    CurrentPlaylist = playlist;

                Program.Discord.Logger.LogInformation("Queued Playlist {0}", playlist.Name);
                foreach (MultiTrack track in playlist.Tracks)
                {
                    Queue.Enqueue(track);
                    track.PlayId = RandomUtil.GenerateGuid();
                }



                if (LoopType == LoopType.QUEUE)
                    QueueLoopList.AddRange(playlist.Tracks);
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
                {
                    Queue.Enqueue(track);
                    track.PlayId = RandomUtil.GenerateGuid();
                }
                if (LoopType == LoopType.QUEUE)
                    QueueLoopList.AddRange(tracks);
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

        public MusicActionResponseType Remove(string playid)
        {
            if (Queue.Count == 0) return MusicActionResponseType.QUEUE_EMPTY;
            Queue<MultiTrack> newQueue = new Queue<MultiTrack>();

            foreach (var item in Queue)
            {
                if (item.PlayId != playid)
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
    }
}
