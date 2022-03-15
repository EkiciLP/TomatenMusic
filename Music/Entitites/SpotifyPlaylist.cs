using System;
using System.Collections.Generic;
using System.Text;

namespace TomatenMusic.Music.Entitites
{
    class SpotifyPlaylist : LavalinkPlaylist
    {
        public string Name { get; }
        public IEnumerable<MultiTrack> Tracks { get; }
        public Uri Url { get; set; }
        public string AuthorName { get; set; }
        public Uri AuthorUri { get; set; }
        public string Description { get; set; }
        public int Followers { get; set; }
        public string Identifier { get; }
        public Uri AuthorThumbnail { get; set; }


        public SpotifyPlaylist(string name, string id, IEnumerable<MultiTrack> tracks)
        {
            Name = name;
            Identifier = id;
            Tracks = tracks;
        }
    }
}
