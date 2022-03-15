using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Commands.Checks;
using TomatenMusic.Music;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Util;

namespace TomatenMusic.Commands
{
    [SlashCommandGroup("play", "Play a song.")]
    public class PlayCommandGroup : ApplicationCommandModule
    {
        [SlashCommand("query", "Play a song with its youtube/spotify link. (or youtube search)")]
        [UserInMusicChannelCheck]
        [OnlyGuildCheck]
        public async Task PlayQueryCommand(InteractionContext ctx, [Option("query", "The song search query.")] string query)
        {

            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MusicActionResponse response = await player.SearchAsync(query); ;

            if (response.Type == MusicActionResponseType.NO_MATCHES)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ ``Your search {query} did not result in any results.``")
                      );
                return;
            }

            if (response.Type == MusicActionResponseType.FAIL || response.Type == MusicActionResponseType.LAVA_CONN_FAILED)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("❌ ``This Playlist does not exist or the Bot is currently in an erroneus state.``")

                     );
                return;
            }


            MusicActionResponseType Response = await player.ConnectAsync(ctx.Member.VoiceState.Channel);

            if (response.isPlaylist)
            {
                LavalinkPlaylist playlist = response.Playlist;
                await player.PlayPlaylistAsync(playlist);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Now Playing:").AddEmbed(
                Common.AsEmbed(playlist)
                ));

            }
            else
            {
                MultiTrack track = response.Track;

                _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(!(await player.isPlayingAsync()) ? "Now Playing:" : "Added to Queue").AddEmbed(Common.AsEmbed(track, player.PlayerQueue.LoopType, !(await player.isPlayingAsync()) ? 0 : player.PlayerQueue.Queue.Count + 1)));

                Response = await player.PlayAsync(response.Track);
                Program.Discord.Logger.LogDebug("Played with response code {0}", Response.ToString());
            }
        }

        [SlashCommand("file", "Play a song file. (mp3/mp4)")]
        [UserInVoiceChannelCheck]
        [UserInMusicChannelCheck]
        [OnlyGuildCheck]
        public async Task PlayFileCommand(InteractionContext ctx, [Option("File", "The File that should be played.")] DiscordAttachment file)
        {

            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MusicActionResponse response = await player.SearchAsync(file.Url); ;



            if (response.Type == MusicActionResponseType.NO_MATCHES)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ ``Your Url {file.Url} did not lead to any mp3/mp4 results.``")
                      );
                return;
            }

            if (response.Type == MusicActionResponseType.FAIL || response.Type == MusicActionResponseType.LAVA_CONN_FAILED)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ ``Your Url {file.Url} did not lead to any mp3/mp4 results, or the Bot is currently broken...``")
                     );
                return;
            }


            MusicActionResponseType Response = await player.ConnectAsync(ctx.Member.VoiceState.Channel);


            MultiTrack track = response.Track;

            _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(!(await player.isPlayingAsync()) ? "Now Playing:" : "Added to Queue").AddEmbed(Common.AsEmbed(track, player.PlayerQueue.LoopType, !(await player.isPlayingAsync()) ? 0 : player.PlayerQueue.Queue.Count + 1)));

            Response = await player.PlayAsync(response.Track);
            Program.Discord.Logger.LogDebug("Played with response code {0}", Response.ToString());
        }
    }
}
