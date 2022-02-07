using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using System.Linq;
using DSharpPlus.SlashCommands;
using TomatenMusic.Commands;

namespace TomatenMusic
{
    class Program
    {

        public static DiscordClient Discord { get; set; }

        static void Main(string[] args)
        {

            new Program().InitBotAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        async Task InitBotAsync(string[] args)
        {
            Discord = new DiscordClient(new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = "NzY2OTM5ODMxMjkyMTMzMzc2.X4qqYA.JF2Snfi5aOLHvNpUz2ttxqjpUfU",
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.All

            });

            var lavaEndPoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "SGWaldsolms9",
                RestEndpoint = lavaEndPoint,
                SocketEndpoint = lavaEndPoint
            };

            var lavalink = Discord.UseLavalink();

            var slash = Discord.UseSlashCommands();

            Discord.MessageCreated += Discord_MessageCreated;
            Discord.Ready += Discord_Ready;
            Discord.GetSlashCommands().RegisterCommands<MusicCommands>(835089895092387872);

            await Discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);


            await Task.Delay(-1);
        }

        private async Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            await Discord.UpdateStatusAsync(new DiscordActivity("In Development", ActivityType.Playing), UserStatus.Online);

        }

        private async Task Discord_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (!e.Message.Content.StartsWith(",")) return;

            var lava = sender.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await e.Message.RespondAsync("The Lavalink connection is not established");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();
            var member = await e.Guild.GetMemberAsync(e.Author.Id);

            if (member.VoiceState == null || member.VoiceState.Channel.Type != ChannelType.Voice)
            {
                await e.Message.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(member.VoiceState.Channel);

        }


    }
}
