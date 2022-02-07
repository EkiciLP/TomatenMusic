using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using TomatenMusic.Music.Entitites;

namespace TomatenMusic.Music
{
    class MusicActionResponse
    {

        MusicActionResponseType Type { get; }
        LavalinkPlaylist Playlist { get; }
        LavalinkTrack Track { get; }
        bool isPlaylist { get; }

        public MusicActionResponse(MusicActionResponseType type, LavalinkTrack track = null, LavalinkPlaylist playlist = null)
        {
            Type = type;
            Playlist = playlist;
            Track = track;

            isPlaylist = playlist != null;
        }
    }
}
