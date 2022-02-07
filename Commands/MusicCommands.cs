using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace TomatenMusic.Commands
{
    class MusicCommands : ApplicationCommandModule
    {
        
       [SlashCommand("play", "Play a song")]
       public async Task PlayCommand(InteractionContext ctx, [Option("query", "The song search query.")] string query)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Hello Im playing!")
                .AsEphemeral(true));
            
        }
    }
}
