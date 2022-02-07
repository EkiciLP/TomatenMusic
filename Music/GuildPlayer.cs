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

        public ulong Guild_id {get; }
        private LavalinkExtension Lavalink;
        ILogger<BaseDiscordClient> logger = Program.Discord.Logger;


        public GuildPlayer(DiscordGuild guild, LavalinkExtension lavalink)
        {
            this.Guild_id = guild.Id;
            this.Lavalink = lavalink;

        }

        public async Task<MusicActionResponseType> PlayAsync(LavalinkTrack track)
        {
            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null) return MusicActionResponseType.NOT_CONNECTED;

            logger.LogInformation("Started playing Track {0} on Guild {1}", track.Title, guildConnection.Guild.Name);

            //TODO
        }

        public async Task<MusicActionResponse> SearchAsync(string query)
        {
            Uri uri;
            LavalinkLoadResult loadResult;
            bool isSearch = true;

            LavalinkNodeConnection node = Lavalink.GetIdealNodeConnection();

            if (node == null) return new MusicActionResponse(MusicActionResponseType.LAVA_CONN_FAILED);

            if (Uri.TryCreate(query, UriKind.RelativeOrAbsolute, out uri)) 
            {
                loadResult = await node.Rest.GetTracksAsync(uri);
                isSearch = false;
            }
            else
                loadResult = await node.Rest.GetTracksAsync(query);

            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.LoadFailed)) return new MusicActionResponse(MusicActionResponseType.FAIL);

            if (loadResult.LoadResultType.Equals(LavalinkLoadResultType.NoMatches)) return new MusicActionResponse(MusicActionResponseType.NO_MATCHES);


            return loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded && isSearch ?
                new MusicActionResponse(MusicActionResponseType.SUCCESS, loadResult.Tracks.First()) :
                loadResult.LoadResultType == LavalinkLoadResultType.TrackLoaded ?
                    new MusicActionResponse(MusicActionResponseType.SUCCESS, loadResult.Tracks.First()) :
                    new MusicActionResponse(MusicActionResponseType.SUCCESS, playlist: new LavalinkPlaylist(loadResult.PlaylistInfo.Name, loadResult.Tracks));

        }

        public async Task<MusicActionResponseType> QuitAsync()
        {
            if (Lavalink.ConnectedNodes.Any()) return MusicActionResponseType.LAVA_CONN_FAILED;

            LavalinkGuildConnection guildConnection = await GetGuildConnectionAsync();

            if (guildConnection == null) return MusicActionResponseType.NOT_CONNECTED;

            logger.LogInformation("Disconnected from Channel {0} on Guild {1}", guildConnection.Channel.Name, guildConnection.Guild.Name);

            await guildConnection.DisconnectAsync();

            return MusicActionResponseType.SUCCESS;
        }

        private async Task<MusicActionResponseType> ConnectAsync(DiscordChannel channel)
        {
            if (Lavalink.ConnectedNodes.Any()) return MusicActionResponseType.LAVA_CONN_FAILED;

            if (channel.Type != ChannelType.Voice) return MusicActionResponseType.WRONG_CHANNEL_TYPE;

            LavalinkNodeConnection node = await Task.Run(() => Lavalink.GetIdealNodeConnection());

            LavalinkGuildConnection conn = await node.ConnectAsync(channel);

            conn.PlaybackFinished += Conn_PlaybackFinished;
            logger.LogInformation("Connected to Channel {0} on Guild {1}", channel.Name, channel.Guild.Name);
            return MusicActionResponseType.SUCCESS;

        }

        private async Task Conn_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {

            if (e.Reason == DSharpPlus.Lavalink.EventArgs.TrackEndReason.LoadFailed)
            { 
                await sender.PlayAsync(e.Track);
                return;
            }
            await QuitAsync();


            logger.LogInformation("Track {0} ended on {1} with reason {2}", e.Track.Title, sender.Guild.Name, e.Reason);
        }

        public static async Task<GuildPlayer> GetGuildPlayerAsync(DiscordGuild guild)
        {
            return await Task.Run(() => guildPlayers.Find(player => player.Guild_id == guild.Id));
        }

        private async Task<LavalinkGuildConnection> GetGuildConnectionAsync()
        {
            return Lavalink.GetIdealNodeConnection().GetGuildConnection(await Program.Discord.GetGuildAsync(Guild_id));
            
        } 
        
    }
}
