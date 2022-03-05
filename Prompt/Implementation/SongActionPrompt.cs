using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Prompt.Buttons;
using TomatenMusic.Prompt.Model;

namespace TomatenMusic.Prompt.Implementation
{
    class SongActionPrompt : ButtonPrompt
    {
        public MultiTrack Track { get; set; }
        public SongActionPrompt(MultiTrack track, DiscordMember requestMember, List<DiscordEmbed> embeds = null)
        {
            Embeds = embeds;
            Track = track;

            AddOption(new AddToQueueButton(new List<MultiTrack>() { track }, 1, requestMember));
        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            return new DiscordMessageBuilder().AddEmbeds(Embeds);
        }
    }
}
