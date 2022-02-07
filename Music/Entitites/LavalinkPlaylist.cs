using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using System.Linq;

namespace TomatenMusic.Music.Entitites
{
    class LavalinkPlaylist
    {
        string Name { get; }
        IEnumerable<LavalinkTrack> Tracks { get; }
        int TrackCount { get; }
        bool isCustom { get; } = false;


        public LavalinkPlaylist(string name, IEnumerable<LavalinkTrack> tracks)
        {
            Name = name;
            Tracks = tracks;
            TrackCount = tracks.Count();
        }
    }
}
