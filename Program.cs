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
using DSharpPlus.SlashCommands.EventArgs;
using TomatenMusic.Commands;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TomatenMusic.Music;

namespace TomatenMusic
{
    class Program
    {

        public static DiscordClient Discord { get; set; }

        static void Main(string[] args)
        {

            new Program().InitBotAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }


        public struct ConfigJson
        {
            [JsonProperty("TOKEN")]
            public string Token { get; private set; }

            [JsonProperty("LavaLinkPassword")]
            public string LavaLinkPassword { get; private set; }
        }

        public ConfigJson config;

        private async Task InitBotAsync(string[] args)
        {

            await initJson();
            Discord = new DiscordClient(new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = config.Token,
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
                Password = config.LavaLinkPassword,
                RestEndpoint = lavaEndPoint,
                SocketEndpoint = lavaEndPoint
            };

            var lavalink = Discord.UseLavalink();

            var slash = Discord.UseSlashCommands();

            Discord.Ready += Discord_Ready;
            Discord.GetSlashCommands().RegisterCommands<MusicCommands>(835089895092387872);

            slash.SlashCommandInvoked += Slash_SlashCommandInvoked;
            slash.SlashCommandErrored += Slash_SlashCommandErrored;

            await Discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);


            await Task.Delay(-1);
        }

        private async Task initJson()
        {
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            config = JsonConvert.DeserializeObject<ConfigJson>(json);
        }

        private async Task Slash_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Discord.Logger.LogDebug("Command {0} invoked by {1} on Guild {2} with Exception {3}", e.Context.CommandName, e.Context.Member, e.Context.Guild, e.Exception);
        }

        private async Task Slash_SlashCommandInvoked(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandInvokedEventArgs e)
        {
            Discord.Logger.LogDebug("Command {0} invoked by {1} on Guild {2}", e.Context.CommandName, e.Context.Member, e.Context.Guild);
            GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(e.Context.Guild);
            IServiceCollection services = new ServiceCollection().AddSingleton<GuildPlayer>()

            e.Context.Services = services.BuildServiceProvider();

            //TODO
        }

        private async Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            await Discord.UpdateStatusAsync(new DiscordActivity("In Development", ActivityType.Playing), UserStatus.Online);

        }




    }
}
