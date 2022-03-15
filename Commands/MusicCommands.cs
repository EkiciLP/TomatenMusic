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
using TomatenMusic.Prompt;
using TomatenMusic.Prompt.Model;
using TomatenMusic.Prompt.Implementation;
using TomatenMusic.Prompt.Option;
using System.Linq;

namespace TomatenMusic.Commands
{
    class MusicCommands : ApplicationCommandModule
    {

        [SlashCommand("stop", "Stops the current Playback and clears the Queue")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task StopCommand(InteractionContext ctx)
        {

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);
            MusicActionResponseType response = await player.QuitAsync();

            if (response == MusicActionResponseType.NOT_CONNECTED)
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder
                {
                    Content = "The Bot is not Connected.",
                    IsEphemeral = true
                });
            else
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder
                {
                    Content = "The Bot has successfully Disconnected and reset it's settings",
                    IsEphemeral = true
                });
        }


        [SlashCommand("skip", "Skips the current song and plays the next one in the queue")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task SkipCommand(InteractionContext ctx)
        {

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);
            LavalinkGuildConnection conn = await player.GetGuildConnectionAsync();
            LavalinkTrack oldTrack = conn.CurrentState.CurrentTrack;
            try
            {
            }catch (Exception e) 
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"⛔ Could not Skip Song, Queue Empty!").AsEphemeral(true));
                return;
            }

            MusicActionResponseType response = await player.SkipAsync();



            _ = ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Skipped From Song ``{oldTrack.Title}`` To Song:").AddEmbed(Common.AsEmbed(new MultiTrack(conn.CurrentState.CurrentTrack), loopType: player.PlayerQueue.LoopType)).AsEphemeral(true));
        }

        [SlashCommand("fav", "Shows the favorite Song Panel")]
        [OnlyGuildCheck]
        public async Task FavCommand(InteractionContext ctx)
        {
            
        }

        [SlashCommand("search", "Searches for a specific query")]
        [OnlyGuildCheck]
        public async Task SearchCommand(InteractionContext ctx, [Option("query", "The Search Query")] string query)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MusicActionResponse response = await player.SearchAsync(query, true);

            if (response.Type == MusicActionResponseType.SUCCESS)
            {
                var prompt = new SongSelectorPrompt($"Search results for {query}", response.Tracks);
                prompt.ConfirmCallback = async (tracks) =>
                {
                    var selectPrompt = new SongListActionPrompt(tracks, ctx.Member, prompt);
                    await selectPrompt.UseAsync(prompt.Interaction, prompt.Message);
                };

                await prompt.UseAsync(ctx.Interaction, await ctx.GetOriginalResponseAsync());
            } else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Search failed with response code: {response.Type}"));
        }

        [SlashCommand("time", "Sets the playing position of the current Song.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task TimeCommand(InteractionContext ctx, [Option("time", "The time formatted like this: Hours: 1h, Minutes: 1m, Seconds 1s")] string time)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            TimeSpan timeSpan;

            try
            {
                timeSpan = TimeSpan.Parse(time);
            }
            catch (Exception e)
            {

                try
                {
                    timeSpan = Common.ToTimeSpan(time);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("😔 An Error occured when parsing your input."));
                    return;
                }
            }

            MusicActionResponseType response = await player.SeekAsync(timeSpan);
            if (response == MusicActionResponseType.NOTHING_PLAYING)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("😔 Currently no Song is playing"));
            else if (response == MusicActionResponseType.FAIL)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("😔 The Specified Timespan is out of bounds."));
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"😀 You successfully set the Song to {time}."));
            
        }

        [SlashCommand("pause", "Pauses or Resumes the current Song.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task PauseCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MusicActionResponseType response = await player.TogglePauseAsync();

            if (response == MusicActionResponseType.NOTHING_PLAYING)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("😔 There's currently no Song playing"));
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"😀 You {(player.Paused ? "successfully paused the Track" : "successfully resumed the Track")}"));

        }

        [SlashCommand("shuffle", "Shuffles the Queue.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task ShuffleCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MusicActionResponseType response = await player.ShuffleAsync();

            if (response == MusicActionResponseType.QUEUE_EMPTY)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("😔 The Queue is currently Empty"));
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"😀 You shuffled the Queue."));

        }

        [SlashCommand("loop", "Sets the loop type of the current player.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task LoopCommand(InteractionContext ctx, [Option("Looptype", "The loop type which the player should be set to")] LoopType type)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MusicActionResponseType response = await player.SetLoopAsync(type);


            if(response == MusicActionResponseType.NOTHING_PLAYING)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"😔 The Bot is currently Idle"));
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"😀 You have set the Loop to ``{type.ToString()}``."));

        }

        [SlashCommand("queue", "Shows the Queue")]
        [OnlyGuildCheck]
        public async Task QueueCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);

            MultiTrack track = player.CurrentSong;

            if (track == null)
            {
                _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ ``Theres currently nothing playing``"));
                return;
            }

            QueuePrompt prompt = new QueuePrompt(player);

            _ = prompt.UseAsync(ctx.Interaction, await ctx.GetOriginalResponseAsync());
        }

    }
}
