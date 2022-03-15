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
            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(ctx.Guild);
            bool allowed = await player.AreActionsAllowedAsync(ctx.Member);

            if (!allowed)
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("Please connect to the Bots Channel to use this Command").AsEphemeral(true));
            return allowed;
        }
    }
}
