using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music;
using TomatenMusic.Music.Entitites;

namespace TomatenMusic.Prompt.Buttons
{
    class AddToQueueButton : ButtonPromptOption
    {
        public List<MultiTrack> Tracks { get; set; }

        public AddToQueueButton(List<MultiTrack> tracks, int row, DiscordMember requestMember)
        {
            Tracks = tracks;
            Emoji = new DiscordComponentEmoji("▶️");
                Row = row;
                Style = DSharpPlus.ButtonStyle.Primary;
                UpdateMethod = (prompt) =>
                {
                    if (requestMember.VoiceState == null || requestMember.VoiceState.Channel == null)
                        prompt.Disabled = true;

                    return Task.FromResult(prompt);
                };
            Run = async (args, sender, option) =>
            {
                GuildPlayer player = await GuildPlayer.GetGuildPlayerAsync(args.Guild);

                MusicActionResponseType Response = await player.ConnectAsync(((DiscordMember)args.User).VoiceState.Channel);

                if (Response == MusicActionResponseType.SUCCESS)
                {
                    Response = await player.PlayTracksAsync(Tracks);
                }
            };
        }
    }
}
