using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using TomatenMusic.Music.Entitites;

namespace TomatenMusic.Music
{
    class MusicActionResponse
    {

        public MusicActionResponseType Type { get; }
        public LavalinkPlaylist Playlist { get; }
        public MultiTrack Track { get; }
        public IEnumerable<MultiTrack> Tracks { get; }
        public bool isPlaylist { get; }
        public string Message { get;}

        public MusicActionResponse(MusicActionResponseType type, MultiTrack track = null, LavalinkPlaylist playlist = null, string message = null, IEnumerable<MultiTrack> tracks = null)
        {
            Type = type;
            Playlist = playlist;
            Track = track;
            Message = message;
            isPlaylist = playlist != null;
            Tracks = tracks;
        }
    }
}
