using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Linq;
using TomatenMusic.Music.Entitites;
using Microsoft.Extensions.Logging;
using DSharpPlus.Lavalink.EventArgs;

namespace TomatenMusic.Music
{
    class GuildPlayer
    {

        public static List<GuildPlayer> guildPlayers { get; } = new List<GuildPlayer>();

        public static async Task<GuildPlayer> GetGuildPlayerAsync(DiscordGuild guild)
        {
            GuildPlayer guildPlayer = await Task.Run(() => guildPlayers.Find(player => player.Guild_id == guild.Id));

            if (guildPlayer == null)
                guildPlayer = new GuildPlayer(guild, Program.Discord.GetLavalink());

            return guildPlayer;
        }


        /// <summary>
        /// /////////
        /// End of static
        /// 

        public ulong Guild_id {get; }
        private LavalinkExtension Lavalink;
        ILogger<BaseDiscordClient> Logger = Program.Discord.Logger;
        public PlayerQueue PlayerQueue { get;} = new PlayerQueue();
        public bool Paused { get; set; } = false;
        private LavalinkGuildConnection GuildConnection;
        public MultiTrack CurrentSong { get; private set; }
                
        public GuildPlayer(DiscordGuild guild, LavalinkExtension lavalink)
        {
            this.Guild_id = guild.Id;
            this.Lavalink = lavalink;
            guildPlayers.Add(this);
        }

        public async Task<MusicActionResponseType> PlayAsync(MultiTrack track, bool asSkip = false)
        {

            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();
            if (guildConnection == null || !guildConnection.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            if (asSkip)
            {
                CurrentSong = track;
                await guildConnection.PlayAsync(track.LavalinkTrack);
                Logger.LogInformation("Started playing Track {0} on Guild {1}", track.Title, guildConnection.Guild.Name);
                return MusicActionResponseType.SUCCESS;

            }

            PlayerQueue.QueueTrack(track);

            if (!(await isPlayingAsync()))
            {

                MusicActionResponse response = PlayerQueue.NextTrack();
                await guildConnection.PlayAsync(response.Track.LavalinkTrack);
                CurrentSong = response.Track;
                Logger.LogInformation("Started playing Track {0} on Guild {1}", track.Title, guildConnection.Guild.Name);
            }
                

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> PlayTracksAsync(List<MultiTrack> tracks)
        {
            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null || !guildConnection.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            Logger.LogInformation("Started playing TrackList {0} on Guild {1}", tracks.ToString(), guildConnection.Guild.Name);

            await PlayerQueue.QueueTracksAsync(tracks);

            if (!await isPlayingAsync())
            {
                MultiTrack nextTrack = PlayerQueue.NextTrack().Track;
                await guildConnection.PlayAsync(nextTrack.LavalinkTrack);
                CurrentSong = nextTrack;
            }

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> PlayPlaylistAsync(LavalinkPlaylist playlist)
        {
            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null || !guildConnection.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            Logger.LogInformation("Started playing Playlist {0} on Guild {1}", playlist.Name, guildConnection.Guild.Name);

            await PlayerQueue.QueuePlaylistAsync(playlist);


            if (!await isPlayingAsync())
            {
                MultiTrack nextTrack = PlayerQueue.NextTrack().Track;
                await guildConnection.PlayAsync(nextTrack.LavalinkTrack);
                CurrentSong = nextTrack;
            }

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> RewindAsync()
        {
            MusicActionResponse response = PlayerQueue.Rewind();

            if (response.Type != MusicActionResponseType.SUCCESS)
            {
                return response.Type;
            }

            Logger.LogInformation($"Rewinded Track {(await GetGuildConnectionAsync()).CurrentState.CurrentTrack.Title} for Track {response.Track.Title}");
            await PlayAsync(response.Track, true);

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponse> SearchAsync(string query, bool withSearchResults = false)
        {
            Uri uri;
            LavalinkLoadResult loadResult;
            bool isSearch = true;

            LavalinkNodeConnection node = Lavalink.GetIdealNodeConnection();

            if (node == null) return new MusicActionResponse(MusicActionResponseType.LAVA_CONN_FAILED);

            if (query.StartsWith("https://open.spotify.com"))
            {
                return await Program.spotify.ConvertURL(query);
            }

            if (Uri.TryCreate(query, UriKind.Absolute, out uri)) 
            {
                loadResult = await node.Rest.GetTracksAsync(uri);
                isSearch = false;
            }
            else
                loadResult = await node.Rest.GetTracksAsync(query, LavalinkSearchType.Youtube);
            

            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.LoadFailed)) return new MusicActionResponse(MusicActionResponseType.FAIL);

            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.NoMatches)) return new MusicActionResponse(MusicActionResponseType.NO_MATCHES);

            if (withSearchResults)
            {
                return new MusicActionResponse(MusicActionResponseType.SUCCESS,
                    playlist: new LavalinkPlaylist(loadResult.PlaylistInfo.Name, MultiTrack.ToMultiTrackList(loadResult.Tracks), uri));
            }

            if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded && !isSearch)
                return new MusicActionResponse(MusicActionResponseType.SUCCESS,
                    playlist: new LavalinkPlaylist(loadResult.PlaylistInfo.Name, MultiTrack.ToMultiTrackList(loadResult.Tracks), uri));
            else
                return new MusicActionResponse(MusicActionResponseType.SUCCESS, new MultiTrack(loadResult.Tracks.First()));

        }

        public async Task<MusicActionResponse> SearchAsync(Uri fileUri)
        {

            LavalinkNodeConnection node = Lavalink.GetIdealNodeConnection();

            if (node == null) return new MusicActionResponse(MusicActionResponseType.LAVA_CONN_FAILED);

            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(fileUri);


            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.LoadFailed)) return new MusicActionResponse(MusicActionResponseType.FAIL);

            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.NoMatches)) return new MusicActionResponse(MusicActionResponseType.NO_MATCHES);


            return new MusicActionResponse(MusicActionResponseType.SUCCESS, new MultiTrack(loadResult.Tracks.First()));

        }

        public async Task<MusicActionResponseType> SkipAsync()
        {
            MusicActionResponse response = PlayerQueue.NextTrack();

            if (response.Type != MusicActionResponseType.SUCCESS)
            {
                return response.Type;
            }
            Logger.LogInformation($"Skipped Track {(await GetGuildConnectionAsync()).CurrentState.CurrentTrack.Title} for Track {response.Track.Title}");
            await PlayAsync(response.Track, true);

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> TogglePauseAsync()
        {
            LavalinkGuildConnection conn = await GetGuildConnectionAsync();

            if (conn == null || !conn.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            if (conn.CurrentState.CurrentTrack == null) return MusicActionResponseType.NOTHING_PLAYING;


            if (Paused)
                await conn.ResumeAsync();
            else
                await conn.PauseAsync();

            Paused = !Paused;
            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> SetLoopAsync(LoopType type)
        {
            LavalinkGuildConnection conn = await GetGuildConnectionAsync();

            if (conn == null || !conn.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            if (conn.CurrentState.CurrentTrack == null) return MusicActionResponseType.NOTHING_PLAYING;

            _ = PlayerQueue.SetLoopAsync(type);
            
            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> ShuffleAsync()
        {
            LavalinkGuildConnection conn = await GetGuildConnectionAsync();

            if (conn == null || !conn.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            if (conn.CurrentState.CurrentTrack == null) return MusicActionResponseType.NOTHING_PLAYING;

            MusicActionResponseType response = await PlayerQueue.ShuffleAsync();

            if (response != MusicActionResponseType.SUCCESS)
                return response;

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> QuitAsync()
        {

            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null) return MusicActionResponseType.NOT_CONNECTED;

            await guildConnection.DisconnectAsync();

            PlayerQueue.Clear();

            Paused = false;

            CurrentSong = null;

            guildConnection.PlaybackFinished -= Conn_PlaybackFinished;

            Logger.LogInformation("Disconnected from Channel {0} on Guild {1}", guildConnection.Channel.Name, guildConnection.Guild.Name);

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> ConnectAsync(DiscordChannel channel)
        {
            if (!Lavalink.ConnectedNodes.Any()) return MusicActionResponseType.LAVA_CONN_FAILED;

            if (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage) return MusicActionResponseType.WRONG_CHANNEL_TYPE;

            if ((await GetGuildConnectionAsync()) != null)
            { 
                if ((await GetGuildConnectionAsync()).IsConnected)
                    return MusicActionResponseType.ALREADY_CONNECTED;
            }

            LavalinkNodeConnection node = Lavalink.GetIdealNodeConnection();

            GuildConnection = await node.ConnectAsync(channel);

            GuildConnection.PlaybackFinished += Conn_PlaybackFinished;

            if (channel.Type == ChannelType.Stage)
            {
                DiscordStageInstance stageInstance = await channel.GetStageInstanceAsync();

                if (stageInstance == null)
                    stageInstance = await channel.CreateStageInstanceAsync("Music");

                //request stage speak!!!!
            }
                

            Logger.LogInformation("Connected to Channel {0} on Guild {1}", channel.Name, channel.Guild.Name);
            return MusicActionResponseType.SUCCESS;

        }

        public async Task<MusicActionResponseType> SeekAsync(TimeSpan timeSpan)
        {
            LavalinkGuildConnection conn = await GetGuildConnectionAsync();

            if (conn == null || !conn.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            if (conn.CurrentState.CurrentTrack == null) return MusicActionResponseType.NOTHING_PLAYING;

            if (timeSpan.CompareTo(conn.CurrentState.CurrentTrack.Length) == 1) return MusicActionResponseType.FAIL;

            await conn.SeekAsync(timeSpan);

            return MusicActionResponseType.SUCCESS;
        }

        private async Task Conn_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            Logger.LogInformation("Track {0} ended on {1} with reason {2}", e.Track.Title, sender.Guild.Name, e.Reason);

            if (( await GetChannelAsync()).Users.Count == 0)
            {
                _ = QuitAsync();
                return;
            }

            if (e.Reason == DSharpPlus.Lavalink.EventArgs.TrackEndReason.LoadFailed)
            { 
                await sender.PlayAsync(e.Track); 
                Logger.LogWarning("Track {0} failed to load on {1}. Restarting...", e.Track.Title, sender.Guild.Name);
                return;
            }

            if (e.Reason != TrackEndReason.Finished)
                return;

            MusicActionResponse response = PlayerQueue.NextTrack();

            if (response.Type == MusicActionResponseType.QUEUE_EMPTY)
               _ =  QuitAsync();
            else
               _ =  PlayAsync(response.Track, true);
        }

        /*public async Task<MusicActionResponse> GetCurrentTrack()
        {
            if ((await GetGuildConnectionAsync()) == null || (await GetGuildConnectionAsync()).CurrentState.CurrentTrack == null) return new MusicActionResponse(MusicActionResponseType.NOTHING_PLAYING);

            return new MusicActionResponse(MusicActionResponseType.SUCCESS, new MultiTrack((await GetGuildConnectionAsync()).CurrentState.CurrentTrack));
        }
        */
        public async Task<LavalinkGuildConnection> GetGuildConnectionAsync()
        {
            if (GuildConnection == null)
                GuildConnection = Lavalink.GetIdealNodeConnection().GetGuildConnection(await Program.Discord.GetGuildAsync(Guild_id));

            return GuildConnection;
            
        } 

        public async Task<bool> isPlayingAsync()
        {

            return (await GetGuildConnectionAsync()).CurrentState.CurrentTrack != null;
        }

        public async Task<DiscordChannel> GetChannelAsync()
        {
            LavalinkGuildConnection conn = await GetGuildConnectionAsync();

            return conn != null && conn.IsConnected ? conn.Channel : null;
        }

        public async Task<bool> AreActionsAllowedAsync(DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return false;
            }

            if ( await GetChannelAsync() != member.VoiceState.Channel)
            {
                return false;
            }

            return true;
        }

    }
}
