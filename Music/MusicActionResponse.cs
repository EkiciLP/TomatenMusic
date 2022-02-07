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
        public LavalinkTrack Track { get; }
        public bool isPlaylist { get; }

        public MusicActionResponse(MusicActionResponseType type, LavalinkTrack track = null, LavalinkPlaylist playlist = null)
        {
            Type = type;
            Playlist = playlist;
            Track = track;

            isPlaylist = playlist != null;
        }
    }
}
