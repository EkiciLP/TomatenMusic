using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using System.Linq;
using TomatenMusic.Util;

namespace TomatenMusic.Music.Entitites
{
    class LavalinkPlaylist
    {
        public string Name { get; }
        public IEnumerable<LavalinkTrack> Tracks { get; }
        public int TrackCount { get; }
        public bool isCustom { get; } = false;
        public Uri Url { get; }


        public LavalinkPlaylist(string name, IEnumerable<LavalinkTrack> tracks, Uri url = null)
        {
            Name = name;
            Tracks = tracks;
            TrackCount = tracks.Count();
            Url = url;
        }

        public string GetContentString()
        {
            StringBuilder builder = new StringBuilder();
            int count = 1;
            foreach (LavalinkTrack track in Tracks)
            {

                if (count > 15)
                {
                    builder.Append(String.Format("***And %s more...***", Tracks.Count() - 15));
                    break;
                }

                builder.Append(count).Append(": ").Append(track.Title.Equals("Unknown title") ? track.Identifier : track.Title).Append(" [").Append(FormatUtil.GetTimestamp(track.Length)).Append("]\n");
                count++;
            }
            return builder.ToString();
        }
    }
}
