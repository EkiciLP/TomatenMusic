using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using TomatenMusic.Music;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Commands.Checks;
using TomatenMusic.Util;
using Microsoft.Extensions.Logging;

namespace TomatenMusic.Commands
{
    class MusicCommands : ApplicationCommandModule
    {
        
       [SlashCommand("play", "Play a song")]
       [UserInVoiceChannelCheck]
       [OnlyGuildCheck]
       public async Task PlayCommand(InteractionContext ctx, [Option("query", "The song search query.")] string query)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MusicActionResponse response = await player.SearchAsync(query);

            if (response.Type == MusicActionResponseType.NO_MATCHES) 
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent(String.Format("Your search {1} did not result in any results.", query))
                      );
                ctx.Client.Logger.Log(LogLevel.Debug, "No matches");
                return;
            }

            


            if (response.Type == MusicActionResponseType.FAIL || response.Type == MusicActionResponseType.LAVA_CONN_FAILED)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("This Command Errored! Please contact an Administrator.")

                     );
                return;
            }


            MusicActionResponseType Response = await player.ConnectAsync(ctx.Member.VoiceState.Channel);

            ctx.Client.Logger.Log(LogLevel.Debug, "Connected with response code {0}.", Response.ToString());

            if (response.isPlaylist)
            {
                LavalinkPlaylist playlist = response.Playlist;
                await player.PlayPlaylistAsync(playlist);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Now Playing:").AddEmbed(
                new DiscordEmbedBuilder
                {
                    ImageUrl = "https://media.tomatentum.net/TMBanner.gif",
                    Title = playlist.Name,
                    Timestamp = DateTimeOffset.Now,
                    Color = new DiscordColor(0x2c2f33),
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = playlist.TrackCount + " Tracks"
                    },
                    Description = playlist.GetContentString()
                }
                ));

            }
            else
            {
                LavalinkTrack track = response.Track;

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(!(await player.isPlayingAsync()) ? "Now Playing:" : "Added to Queue").AddEmbed(
                new DiscordEmbedBuilder
                {
                    ImageUrl = "https://img.youtube.com/vi/" + track.Identifier +"/mqdefault.jpg",
                    Title = track.Title,
                    Timestamp = DateTimeOffset.Now,
                    Color = new DiscordColor(0x2c2f33),
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = track.Author
                    },
                    Url = track.Uri.ToString(),
                    Footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        Text = "Length: " + FormatUtil.GetTimestamp(track.Length)
                    }
                }
                ));

                Response = await player.PlayAsync(response.Track);
                Program.Discord.Logger.LogDebug("Played with response code {0}", Response.ToString());
            }
        }

        [SlashCommand("stop", "Stops the current Playback and clears the Queue")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task StopCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            
        }
    }
}
