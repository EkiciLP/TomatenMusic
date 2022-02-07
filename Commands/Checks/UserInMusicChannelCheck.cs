using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.EventArgs;
using DSharpPlus;
using TomatenMusic.Music;
using Emzi0767.Utilities;

namespace TomatenMusic.Commands.Checks
{
    class UserInMusicChannelCheck : SlashCheckBaseAttribute
    {

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("You are not in a Voice Channel.").AsEphemeral(true));
                return false;
            }

            if ((await (await GuildPlayer.GetGuildPlayerAsync(ctx.Guild)).GetChannelAsync()) != ctx.Member.VoiceState.Channel)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("You are not in the same Channel as the Bot").AsEphemeral(true));
                return false;
            }

            return true;
        }
    }
}
