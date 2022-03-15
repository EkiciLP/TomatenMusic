using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Lavalink;
using System.Linq;
using Microsoft.Extensions.Logging;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Services;

namespace TomatenMusic.Music
{
    class Spotify : SpotifyClient
    {
        public Spotify(SpotifyClientConfig config) : base(config)
        {

        }

        public async Task<MusicActionResponse> ConvertURL(string url)
        {
            string trackId = url
                .Replace("https://open.spotify.com/track/", "")
                .Replace("https://open.spotify.com/album/", "")
                .Replace("https://open.spotify.com/playlist/", "")
                .Substring(0, 22);

            if (url.StartsWith("https://open.spotify.com/track"))
            {
                FullTrack sTrack = await Tracks.Get(trackId);
                Program.Discord.Logger.LogInformation($"Searching youtube from spotify with query: {sTrack.Name} {String.Join(" ", sTrack.Artists)}");

                LavalinkLoadResult loadResult = await Program.Discord.GetLavalink().GetIdealNodeConnection().Rest.GetTracksAsync($"{sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}", LavalinkSearchType.Youtube);


                if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.LoadFailed)) return new MusicActionResponse(MusicActionResponseType.FAIL);

                if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.NoMatches)) return new MusicActionResponse(MusicActionResponseType.NO_MATCHES);

                return new MusicActionResponse(MusicActionResponseType.SUCCESS, await PopulateTrackAsync(await Youtube.PopulateTrackInfoAsync(new MultiTrack(loadResult.Tracks.First()))));

            }
            else if (url.StartsWith("https://open.spotify.com/album"))
            {
                List<MultiTrack> tracks = new List<MultiTrack>();

                FullAlbum album = await Albums.Get(trackId);

                foreach (var sTrack in await PaginateAll(album.Tracks))
                {
                    Program.Discord.Logger.LogInformation($"Searching youtube from spotify with query: {sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}");

                    LavalinkLoadResult loadResult = await Program.Discord.GetLavalink().GetIdealNodeConnection().Rest.GetTracksAsync($"{sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}", LavalinkSearchType.Youtube);


                    if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.LoadFailed)) return new MusicActionResponse(MusicActionResponseType.FAIL);

                    if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.NoMatches)) return new MusicActionResponse(MusicActionResponseType.NO_MATCHES);
                    tracks.Add(await PopulateTrackAsync(await Youtube.PopulateTrackInfoAsync(new MultiTrack(loadResult.Tracks.First()))));
                }
                Uri uri;
                Uri.TryCreate(url, UriKind.Absolute, out uri);

                SpotifyPlaylist playlist = new SpotifyPlaylist(album.Name, album.Id, tracks);
                await PopulateSpotifyAlbumAsync(playlist);
                return new MusicActionResponse(MusicActionResponseType.SUCCESS, playlist: playlist);

            }
            else if (url.StartsWith("https://open.spotify.com/playlist"))
            {
                List<MultiTrack> tracks = new List<MultiTrack>();

                FullPlaylist spotifyPlaylist = await Playlists.Get(trackId);
                
                foreach (var sTrack in await PaginateAll(spotifyPlaylist.Tracks))
                {
                    LavalinkLoadResult loadResult;
                    if (sTrack.Track is FullTrack)
                    {
                        FullTrack fullTrack = (FullTrack)sTrack.Track;
                        Program.Discord.Logger.LogInformation($"Searching youtube from spotify with query: {fullTrack.Name} {String.Join(" ", fullTrack.Artists.ConvertAll(artist => artist.Name))}");
                        loadResult = await Program.Discord.GetLavalink().GetIdealNodeConnection().Rest.GetTracksAsync($"{fullTrack.Name} {String.Join(" ", fullTrack.Artists.ConvertAll(artist => artist.Name))}", LavalinkSearchType.Youtube);


                        if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.LoadFailed)) return new MusicActionResponse(MusicActionResponseType.FAIL);

                        if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.NoMatches)) return new MusicActionResponse(MusicActionResponseType.NO_MATCHES);
                        tracks.Add(await PopulateTrackAsync(await Youtube.PopulateTrackInfoAsync(new MultiTrack(loadResult.Tracks.First()))));
                    }

                }
                Uri uri;
                Uri.TryCreate(url, UriKind.Absolute, out uri);
                SpotifyPlaylist playlist = new SpotifyPlaylist(spotifyPlaylist.Name, spotifyPlaylist.Id, tracks);
                await PopulateSpotifyPlaylistAsync(playlist);
                return new MusicActionResponse(MusicActionResponseType.SUCCESS, playlist: playlist);
            }
            return null;
        }
   
        public async Task<SpotifyPlaylist> PopulateSpotifyPlaylistAsync(SpotifyPlaylist playlist)
        {
            var list = await this.Playlists.Get(playlist.Identifier);
            playlist.Description = list.Description;
            playlist.AuthorUri = new Uri(list.Owner.Uri);
            playlist.AuthorName = list.Owner.DisplayName;
            playlist.Followers = list.Followers.Total;
            playlist.Url = new Uri(list.Uri);
            playlist.AuthorThumbnail = new Uri(list.Owner.Images.First().Url);
            return playlist;
        }

        public async Task<SpotifyPlaylist> PopulateSpotifyAlbumAsync(SpotifyPlaylist playlist)
        {
            var list = await this.Albums.Get(playlist.Identifier);
            playlist.Description = list.Label;
            playlist.AuthorUri = new Uri(list.Artists.First().Uri);
            playlist.AuthorName = list.Artists.First().Name;
            playlist.Followers = list.Popularity;
            playlist.Url = new Uri(list.Uri);

            return playlist;
        }

        public async Task<MultiTrack> PopulateTrackAsync(MultiTrack track)
        {
            var spotifyTrack = await this.Tracks.Get(track.SpotifyIdentifier);
            track.SpotifyAlbum = spotifyTrack.Album;
            track.SpotifyArtists = spotifyTrack.Artists;
            track.SpotifyPopularity = spotifyTrack.Popularity;

            return track;
        }
    }
}
