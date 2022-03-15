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
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web;
using DSharpPlus.Exceptions;

namespace TomatenMusic
{
    class Program
    {

        public static DiscordClient Discord { get; private set; }
        public static Spotify spotify { get; private set; }
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
            [JsonProperty("SpotifyClientId")]
            public string SpotifyClientId { get; private set; }
            [JsonProperty("SpotifyClientSecret")]
            public string SpotifyClientSecret { get; private set; }
            [JsonProperty("YoutubeApiKey")]
            public string YoutubeAPIKey { get; private set; }

        }

        public static ConfigJson config;

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
                Hostname = "116.202.92.16",
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

            spotify = new Spotify(SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(config.SpotifyClientId, config.SpotifyClientSecret)));
            
            Discord.Ready += Discord_Ready;

            //Discord.GetSlashCommands().RegisterCommands<MusicCommands>(835089895092387872);
            //Discord.GetSlashCommands().RegisterCommands<PlayCommandGroup>(835089895092387872);

            Discord.GetSlashCommands().RegisterCommands<MusicCommands>(888493810554900491);
            Discord.GetSlashCommands().RegisterCommands<PlayCommandGroup>(888493810554900491);

            slash.SlashCommandInvoked += Slash_SlashCommandInvoked;
            slash.SlashCommandErrored += Slash_SlashCommandErrored;
            Discord.ClientErrored += Discord_ClientErrored;

            await Discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);

            await Task.Delay(-1);
        }

        private Task Discord_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            Discord.Logger.LogDebug("Event {0} errored with Exception {3}", e.EventName, e.Exception);
            if (e.Exception is NotFoundException)
                Discord.Logger.LogDebug($"{ ((NotFoundException)e.Exception).JsonMessage }");
            if (e.Exception is BadRequestException)
                Discord.Logger.LogDebug($"{ ((BadRequestException)e.Exception).Errors }");
            return Task.CompletedTask;
        }

        private async Task initJson()
        {
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            config = JsonConvert.DeserializeObject<ConfigJson>(json);
        }

        

        private Task Slash_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Discord.Logger.LogDebug("Command {0} invoked by {1} on Guild {2} with Exception {3}", e.Context.CommandName, e.Context.Member, e.Context.Guild, e.Exception);
            if (e.Exception is NotFoundException)
                Discord.Logger.LogDebug($"{ ((NotFoundException)e.Exception).JsonMessage }");
            if (e.Exception is BadRequestException)
                Discord.Logger.LogDebug($"{ ((BadRequestException)e.Exception).JsonMessage }");
            return Task.CompletedTask;

        }

        private Task Slash_SlashCommandInvoked(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandInvokedEventArgs e)
        {
            Discord.Logger.LogDebug("Command {0} invoked by {1} on Guild {2}", e.Context.CommandName, e.Context.Member, e.Context.Guild);
            

            return Task.CompletedTask;
        }

        private async Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            await Discord.UpdateStatusAsync(new DiscordActivity("/ commands!", ActivityType.Watching), UserStatus.Online);

        }




    }
}
