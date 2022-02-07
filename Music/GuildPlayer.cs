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
        PlayerQueue PlayerQueue { get;} = new PlayerQueue();

        public GuildPlayer(DiscordGuild guild, LavalinkExtension lavalink)
        {
            this.Guild_id = guild.Id;
            this.Lavalink = lavalink;
            guildPlayers.Add(this);
        }

        public async Task<MusicActionResponseType> PlayAsync(LavalinkTrack track)
        {
            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null || !guildConnection.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            PlayerQueue.QueueTrack(track);

            if ( !(await isPlayingAsync()))
                await guildConnection.PlayAsync(PlayerQueue.NextTrack().Track);

            Logger.LogInformation("Started playing Track {0} on Guild {1}", track.Title, guildConnection.Guild.Name);

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> PlayPlaylistAsync(LavalinkPlaylist playlist)
        {
            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null || !guildConnection.IsConnected) return MusicActionResponseType.NOT_CONNECTED;

            Logger.LogInformation("Started playing Playlist {0} on Guild {1}", playlist.Name, guildConnection.Guild.Name);

            await PlayerQueue.QueuePlaylistAsync(playlist);


            if (! await isPlayingAsync())
                await guildConnection.PlayAsync(PlayerQueue.NextTrack().Track);

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponse> SearchAsync(string query)
        {
            Uri uri;
            LavalinkLoadResult loadResult;
            bool isSearch = true;

            LavalinkNodeConnection node = Lavalink.GetIdealNodeConnection();

            if (node == null) return new MusicActionResponse(MusicActionResponseType.LAVA_CONN_FAILED);

            if (Uri.TryCreate(query, UriKind.Absolute, out uri)) 
            {
                loadResult = await node.Rest.GetTracksAsync(uri);
                isSearch = false;
                Logger.LogDebug("Searching for URI {0}", uri.ToString());
            }
            else
                loadResult = await node.Rest.GetTracksAsync(query, LavalinkSearchType.Youtube);
            

            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.LoadFailed)) return new MusicActionResponse(MusicActionResponseType.FAIL);

            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.NoMatches)) return new MusicActionResponse(MusicActionResponseType.NO_MATCHES);


            if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded && !isSearch)
                return new MusicActionResponse(MusicActionResponseType.SUCCESS, playlist: new LavalinkPlaylist(loadResult.PlaylistInfo.Name, loadResult.Tracks, uri));
            else 
                return new MusicActionResponse(MusicActionResponseType.SUCCESS, loadResult.Tracks.First());



        }

        public async Task<MusicActionResponseType> QuitAsync()
        {

            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null) return MusicActionResponseType.NOT_CONNECTED;

            await guildConnection.DisconnectAsync();

            PlayerQueue.Clear();

            guildConnection.PlaybackFinished -= Conn_PlaybackFinished;

            Logger.LogInformation("Disconnected from Channel {0} on Guild {1}", guildConnection.Channel.Name, guildConnection.Guild.Name);

            return MusicActionResponseType.SUCCESS;
        }

        public async Task<MusicActionResponseType> ConnectAsync(DiscordChannel channel)
        {
            if (!Lavalink.ConnectedNodes.Any()) return MusicActionResponseType.LAVA_CONN_FAILED;

            if (channel.Type != ChannelType.Voice) return MusicActionResponseType.WRONG_CHANNEL_TYPE;

            if ((await GetGuildConnectionAsync()) != null)
            { 
                if ((await GetGuildConnectionAsync()).IsConnected)
                    return MusicActionResponseType.ALREADY_CONNECTED;
            }

            LavalinkNodeConnection node = Lavalink.GetIdealNodeConnection();

            LavalinkGuildConnection conn = await node.ConnectAsync(channel);

            conn.PlaybackFinished += Conn_PlaybackFinished;

            Logger.LogInformation("Connected to Channel {0} on Guild {1}", channel.Name, channel.Guild.Name);
            return MusicActionResponseType.SUCCESS;

        }

        private async Task Conn_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            Logger.LogInformation("Track {0} ended on {1} with reason {2}", e.Track.Title, sender.Guild.Name, e.Reason);
            if (e.Reason == DSharpPlus.Lavalink.EventArgs.TrackEndReason.LoadFailed)
            { 
                await sender.PlayAsync(e.Track); 
                Logger.LogWarning("Track {0} failed to load on {1}. Restarting...", e.Track.Title, sender.Guild.Name);
                return;
            }

            MusicActionResponse response = PlayerQueue.NextTrack();

            if (response.Type == MusicActionResponseType.QUEUE_EMPTY)
                await QuitAsync();
            else
                await PlayAsync(response.Track);


            
        }

        private async Task<LavalinkGuildConnection> GetGuildConnectionAsync()
        {
            return Lavalink.GetIdealNodeConnection().GetGuildConnection(await Program.Discord.GetGuildAsync(Guild_id));
            
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

    }
}
