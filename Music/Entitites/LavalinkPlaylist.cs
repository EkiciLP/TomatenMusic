using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using System.Linq;
using TomatenMusic.Util;
using DSharpPlus.Entities;

namespace TomatenMusic.Music.Entitites
{
    class LavalinkPlaylist
    {
        public string Name { get; }
        public IEnumerable<MultiTrack> Tracks { get; }
        public int TrackCount { get; }
        public bool isCustom { get; } = false;
        public Uri Url { get; }
        public string Author { get; set; } = "Youtube";

        public LavalinkPlaylist(string name, IEnumerable<MultiTrack> tracks, Uri url = null)
        {
            Name = name;
            Tracks = tracks;
            TrackCount = tracks.Count();
            Url = url;
            isCustom = url == null;
        }

        public string GetContentString()
        {
            StringBuilder builder = new StringBuilder();
            int count = 1;
            foreach (MultiTrack track in Tracks)
            {

                if (count > 15)
                {
                    builder.Append(String.Format("***And {0} more...***", Tracks.Count() - 15));
                    break;
                }

                builder.Append(count).Append(": ").Append(track.Title.Equals("Unknown title") ? track.YoutubeIdentifier : track.Title).Append(" [").Append(Common.GetTimestamp(track.Length)).Append("]\n");
                count++;
            }
            return builder.ToString();
        }

        public DiscordEmbedBuilder AddAsFooter(DiscordEmbedBuilder builder)
        {
            int counter = 0;

            foreach (var item in Tracks)
            {
                if (counter > 24)
                    break;
                builder.AddField(item.Title,
                $"Author: {item.Author}, Length: {Common.GetTimestamp(item.Length)}\n" +
                $"[Go to Song]({item.Uri}])");
                counter++;
            }
            return builder;
        }

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
