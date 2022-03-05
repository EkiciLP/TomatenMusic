using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Util;
using Microsoft.Extensions.Logging;
using TomatenMusic.Music;
using System.Threading.Tasks;

namespace TomatenMusic.Util
{
    class Common
    {

        public static DiscordEmbed AsEmbed(MultiTrack track, LoopType loopType, int position = -1)
        {

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithAuthor(track.Author)
                .WithTitle(track.Title)
                .WithUrl(track.Uri)
                .WithImageUrl("https://img.youtube.com/vi/" + track.YoutubeIdentifier + "/mqdefault.jpg")
                .WithDescription(GetMetaTagValue(track.Uri.ToString()))
                .AddField("Length", Common.GetTimestamp(track.Length), true);

            if (position != -1)
            {
                builder.AddField("Position", (position == 0 ? "Now Playing" : position.ToString()), true);
            }
            builder.AddField("Current Queue Loop", loopType.ToString(), true);

            return builder;
        }

        public static DiscordEmbed AsEmbed(MultiTrack track, int position = -1)
        {

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithAuthor(track.Author)
                .WithTitle(track.Title)
                .WithUrl(track.Uri)
                .WithImageUrl("https://img.youtube.com/vi/" + track.YoutubeIdentifier + "/mqdefault.jpg")
                .WithDescription(GetMetaTagValue(track.Uri.ToString()))
                .AddField("Length", Common.GetTimestamp(track.Length), true);

            if (position != -1)
            {
                builder.AddField("Position", (position == 0 ? "Now Playing" : position.ToString()), true);
            }

            return builder;
        }

        public static DiscordEmbed AsEmbed(LavalinkPlaylist playlist)
        {

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithAuthor(playlist.Author)
                .WithTitle(playlist.Name)
                .WithUrl(playlist.Url)
                .WithImageUrl("https://media.tomatentum.net/TMBanner.gif")
                .WithDescription(GetMetaTagValue(playlist.Url.ToString()))
                .WithFooter($"{playlist.TrackCount} Tracks | Playlist Length {Common.GetTimestamp(playlist.GetLength())}");
                
            return playlist.AddAsFooter(builder);

        }

        public static DiscordEmbed GetQueueEmbed(GuildPlayer player)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            builder.WithDescription(player.PlayerQueue.GetQueueString());
            builder.WithTitle("Current Queue");
            builder.WithAuthor($"{player.PlayerQueue.Queue.Count} Songs");

            TimeSpan timeSpan = TimeSpan.FromTicks(0);

            foreach (var track in player.PlayerQueue.Queue)
            {
                timeSpan = timeSpan.Add(track.Length);
            }

            builder.AddField("Length", GetTimestamp(timeSpan), true);
            builder.AddField("Loop Type", player.PlayerQueue.LoopType.ToString(), true);
            if (player.PlayerQueue.CurrentPlaylist != null)
                builder.AddField("Current Playlist", $"[{player.PlayerQueue.CurrentPlaylist.Name}]({player.PlayerQueue.CurrentPlaylist.Url})", true);

            return builder;
        }

        private static string GetMetaTagValue(string url)
        {
            var getHtmlDoc = new HtmlWeb();
            var document = getHtmlDoc.Load(url);
            var metaTags = document.DocumentNode.SelectNodes("//meta");
            if (metaTags != null)
            {
                foreach (var sitetag in metaTags)
                {
                    if (sitetag.Attributes["name"] != null && sitetag.Attributes["content"] != null && sitetag.Attributes["name"].Value == "description")

                    {
                        string desc = sitetag.Attributes["content"].Value;
                        
                        return desc.Replace("&quot;", "\"");
                    }
                }
            }

            return null;
        }

        public static string GetTimestamp(TimeSpan timeSpan)
        {
            if (timeSpan.Hours > 0)
                return String.Format("{0:hh\\:mm\\:ss}", timeSpan);
            else
                return String.Format("{0:mm\\:ss}", timeSpan);
        }

        public static TimeSpan ToTimeSpan(string text)
        {
            string[] input = text.Split(" ");
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(0);

            foreach (var item in input)
            {
                var l = item.Length - 1;
                var value = item.Substring(0, l);
                var type = item.Substring(l, 1);

                switch (type)
                {
                    case "d": 
                        timeSpan = timeSpan.Add(TimeSpan.FromDays(double.Parse(value)));
                        break;
                    case "h":
                        timeSpan = timeSpan.Add(TimeSpan.FromHours(double.Parse(value))); 
                        break;
                    case "m":
                        timeSpan = timeSpan.Add(TimeSpan.FromMinutes(double.Parse(value)));
                        break;
                    case "s":
                        timeSpan = timeSpan.Add(TimeSpan.FromSeconds(double.Parse(value)));
                        break;
                    case "f":
                        timeSpan = timeSpan.Add(TimeSpan.FromMilliseconds(double.Parse(value))); 
                        break;
                    case "z":
                        timeSpan = timeSpan.Add(TimeSpan.FromTicks(long.Parse(value)));
                        break;
                    default:
                        timeSpan = timeSpan.Add(TimeSpan.FromDays(double.Parse(value)));
                        break;
                }
            }

            return timeSpan;
        }

       public static string ProgressBar(int current, int max)
        {
            int percentage = (current * 100) / max;
            int rounded = (int) Math.Round(((double) percentage / 10));

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i <= 10; i++)
            {
                if (i == rounded)
                    builder.Append("🔘");
                else
                    builder.Append("➖");
            }

            return builder.ToString();
        }

        public async static Task<DiscordEmbed> CurrentSongEmbedAsync(GuildPlayer player)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            LavalinkGuildConnection conn = await player.GetGuildConnectionAsync();
            MultiTrack track = player.CurrentSong;

            string progressBar = $"|{ProgressBar((int)conn.CurrentState.PlaybackPosition.TotalSeconds, (int)track.Length.TotalSeconds)}|\n [{Common.GetTimestamp(conn.CurrentState.PlaybackPosition)}/{Common.GetTimestamp(track.Length)}]";

            builder.WithAuthor(track.Author);
            builder.WithTitle(track.Title);
            builder.WithUrl(track.Uri);
            builder.WithImageUrl("https://img.youtube.com/vi/" + track.YoutubeIdentifier + "/mqdefault.jpg");
            builder.AddField("Length", Common.GetTimestamp(track.Length), true);
            builder.AddField("Loop", player.PlayerQueue.LoopType.ToString(), true);
            builder.AddField("Progress", progressBar, true);

            return builder;
        }
    }
}
