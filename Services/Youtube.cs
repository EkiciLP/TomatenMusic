using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Logging;

namespace TomatenMusic.Services
{
    class Youtube
    {
        public static YouTubeService Service { get; }
        static Youtube()
        {
            Service = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Program.config.YoutubeAPIKey,
                ApplicationName = "TomatenMusic"
            });
        }

        public static async Task<MultiTrack> PopulateTrackInfoAsync(MultiTrack track)
        {
            var video = await GetVideoAsync(track.YoutubeIdentifier);
            var channel = await GetChannelAsync(video.Snippet.ChannelId);
            if (channel.Statistics.SubscriberCount != null)
                track.YoutubeAuthorSubs = (ulong) channel.Statistics.SubscriberCount;
            track.YoutubeAuthorThumbnail = new Uri(channel.Snippet.Thumbnails.High.Url);
            track.YoutubeAuthorUri = new Uri($"https://www.youtube.com/channel/{channel.Id}");
            track.YoutubeDescription = video.Snippet.Description;
            if (video.Statistics.LikeCount != null)
                track.YoutubeLikes = (ulong) video.Statistics.LikeCount;
            track.YoutubeTags = video.Snippet.Tags;
            track.YoutubeThumbnail = new Uri(video.Snippet.Thumbnails.High.Url);
            track.YoutubeUploadDate = (DateTime)video.Snippet.PublishedAt;
            track.YoutubeViews = (ulong)video.Statistics.ViewCount;
            track.YoutubeCommentCount = (ulong)video.Statistics.CommentCount;
            return track;
        }

        public static async Task<List<MultiTrack>> PopulateMultiTrackListAsync(IEnumerable<MultiTrack> tracks)
        {
            List<MultiTrack> newTracks = new List<MultiTrack>();
            foreach (var track in tracks)
                newTracks.Add(await Youtube.PopulateTrackInfoAsync(track));

            return newTracks;
        }
        public static async Task<LavalinkPlaylist> PopulatePlaylistAsync(YoutubePlaylist playlist)
        {
            var list = await GetPlaylistAsync(playlist.Identifier);
            var channel = await GetChannelAsync(list.Snippet.ChannelId);

            playlist.Description = list.Snippet.Description;
            playlist.Thumbnail = new Uri(list.Snippet.Thumbnails.High.Url);
            playlist.CreationTime = (DateTime)list.Snippet.PublishedAt;
            playlist.YoutubeItem = list;
            playlist.AuthorThumbnail = new Uri(channel.Snippet.Thumbnails.High.Url);
            playlist.AuthorUri = new Uri($"https://www.youtube.com/playlist?list={playlist.Identifier}");

            return playlist;
        }

        public static async Task<Video> GetVideoAsync(string id)
        {
            var search = Service.Videos.List("contentDetails,id,liveStreamingDetails,localizations,player,recordingDetails,snippet,statistics,status,topicDetails");
            search.Id = id;
            var response = await search.ExecuteAsync();
            return response.Items.First();
        }

        public static async Task<Channel> GetChannelAsync(string id)
        {
            var search = Service.Channels.List("brandingSettings,contentDetails,contentOwnerDetails,id,localizations,snippet,statistics,status,topicDetails");
            search.Id = id;
            var response = await search.ExecuteAsync();

            return response.Items.First();
        }
        public static async Task<Playlist> GetPlaylistAsync(string id)
        {
            var search = Service.Playlists.List("snippet,contentDetails,status");
            search.Id = id;
            var response = await search.ExecuteAsync();

            return response.Items.First();
        }

        public static async Task<SearchResult> GetRelatedVideoAsync(string id)
        {
            var search = Service.Search.List("snippet");
            search.RelatedToVideoId = id;
            search.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.Moderate;
            search.MaxResults = 1;
            var response = await search.ExecuteAsync();
            return response.Items.First();
        }

    }
}
