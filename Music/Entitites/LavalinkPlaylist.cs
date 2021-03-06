using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using System.Linq;
using TomatenMusic.Util;
using DSharpPlus.Entities;

namespace TomatenMusic.Music.Entitites
{
    interface LavalinkPlaylist
    {
        public string Name { get; }
        public IEnumerable<MultiTrack> Tracks { get; }
        public Uri Url { get; }
        public string AuthorName { get; set; }
        public Uri AuthorUri { get; set; }
        public string Description { get; set; }
        public string Identifier { get; }
        public Uri AuthorThumbnail { get; set; }

        public TimeSpan GetLength()
        {
            TimeSpan timeSpan = TimeSpan.FromTicks(0);

            foreach (var track in Tracks)
            {
                timeSpan = timeSpan.Add(track.Length);
            }

            return timeSpan;
        }
    }
}
