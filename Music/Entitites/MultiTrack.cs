using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Lavalink;
using TomatenMusic.Services;
using System.Linq;
using SpotifyAPI.Web;

namespace TomatenMusic.Music.Entitites
{
    class MultiTrack
    {

        public static List<MultiTrack> ToMultiTrackList(IEnumerable<LavalinkTrack> tracks)
        {
            List<MultiTrack> newTracks = new List<MultiTrack>();

            foreach (var track in tracks)
            {
                newTracks.Add(new MultiTrack(track));
            }

            return newTracks;
        }

        public MultiTrack(LavalinkTrack track, string spotifyIdentifier = null)
        {
            LavalinkTrack = track;
            TrackString = track.TrackString;
            YoutubeIdentifier = track.Identifier;
            IsSeekable = track.IsSeekable;
            YoutubeAuthorName = track.Author;
            Length = track.Length;
            IsStream = track.IsStream;
            Position = track.Position;
            Title = track.Title;
            Uri = track.Uri;
            SpotifyIdentifier = spotifyIdentifier;
        }

        //
        // Summary:
        //     Gets or sets the ID of the track to play.
        public string TrackString { get; set;}
        //
        // Summary:
        //     Gets the identifier of the track.
        public string YoutubeIdentifier { get; private set; }
        //
        // Summary:
        //     Gets whether the track is seekable.
        public bool IsSeekable { get; private set; }
        //
        // Summary:
        //     Gets the track's duration.
        public TimeSpan Length { get; private set; }
        //
        // Summary:
        //     Gets whether the track is a stream.
        public bool IsStream { get; private set; }
        //
        // Summary:
        //     Gets the starting position of the track.
        public TimeSpan Position { get; private set; }
        //
        // Summary:
        //     Gets the title of the track.
        public string Title { get; private set; }
        //
        // Summary:
        //     Gets the source Uri of this track.
        public Uri Uri { get; private set; }
        public LavalinkTrack LavalinkTrack { get; private set; }
        public bool IsFile { get; set; }
        public string PlayId { get; set; }
        public string YoutubeDescription { get; set; }
        public IEnumerable<string> YoutubeTags { get; set; }
        public ulong YoutubeViews { get; set; }
        public ulong YoutubeLikes { get; set; }
        public Uri YoutubeThumbnail { get; set; }
        public DateTime YoutubeUploadDate { get; set; }
        //
        // Summary:
        //     Gets the author of the track.
        public string YoutubeAuthorName { get; private set; }
        public Uri YoutubeAuthorThumbnail { get; set; }
        public ulong YoutubeAuthorSubs { get; set; }
        public Uri YoutubeAuthorUri { get; set; }
        public ulong YoutubeCommentCount { get; set; }
        public string SpotifyIdentifier { get; }
        public SimpleAlbum SpotifyAlbum { get; set; }
        public List<SimpleArtist> SpotifyArtists { get; set; }
        public int SpotifyPopularity { get; set; }


        public async Task<MultiTrack> GetRelatedVideo()
        {
            var video = await Youtube.GetRelatedVideoAsync(YoutubeIdentifier);
            LavalinkLoadResult loadResult = await Program.Discord.GetLavalink().GetIdealNodeConnection().Rest.GetTracksAsync(new Uri($"https://youtu.be/{video.Id}"));

            return await Youtube.PopulateTrackInfoAsync(new MultiTrack(loadResult.Tracks.First()));
        }



    }
}
