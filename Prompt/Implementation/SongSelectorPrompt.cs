using System;
using System.Collections.Generic;
using System.Text;
using TomatenMusic.Prompt.Model;
using DSharpPlus;
using DSharpPlus.Lavalink;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;
using TomatenMusic.Util;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Music;
using System.Linq;


namespace TomatenMusic.Prompt.Implementation
{
    sealed class SongSelectorPrompt : PaginatedSelectPrompt<MultiTrack>
    {
        public bool IsConfirmed { get; set; }
        public Func<List<MultiTrack>, Task> ConfirmCallback { get; set; } = (tracks) =>
        {
            return Task.CompletedTask;
        };

        public IEnumerable<MultiTrack> Tracks { get; private set; }
        public string Title { get; set; }

        public SongSelectorPrompt(string title, IEnumerable<MultiTrack> tracks, DiscordPromptBase lastPrompt = null, List<DiscordEmbed> embeds = null) : base(title, tracks.ToList(), lastPrompt, embeds)
        {
            Title = title;
            Tracks = tracks;
            AddOption(new ButtonPromptOption
            {
                Emoji = new DiscordComponentEmoji("✔️"),
                Row = 3,
                Style = ButtonStyle.Success,
                Run = async (args, client, option) =>
                {
                    if (SelectedItems.Count == 0)
                    {
                        await args.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Please Select a Song!"));
                        return;
                    }
                    IsConfirmed = true;
                    _ = ConfirmCallback.Invoke(SelectedItems);
                }
            });
        }
        public override Task<PaginatedSelectMenuOption<MultiTrack>> ConvertToOption(MultiTrack item)
        {
            return Task.FromResult<PaginatedSelectMenuOption<MultiTrack>>(new PaginatedSelectMenuOption<MultiTrack>
            {
                Label = item.Title,
                Description = item.YoutubeAuthorName
            });

        }

        public override Task OnSelect(MultiTrack item, ComponentInteractionCreateEventArgs args, DiscordClient sender)
        {
            logger.LogDebug($"Added {item.Title}, {SelectedItems}");
            return Task.CompletedTask;
        }

        public override Task OnUnselect(MultiTrack item, ComponentInteractionCreateEventArgs args, DiscordClient sender)
        {
            logger.LogDebug($"Removed {item.Title}");
            return Task.CompletedTask;

        }

        public async Task<List<MultiTrack>> AwaitSelectionAsync()
        {
            return await Task.Run(() =>
            {
                while (!IsConfirmed) 
                {
                    if (State == PromptState.INVALID)
                        throw new InvalidOperationException("Prompt has been Invalidated");
                }
                IsConfirmed = false;
                return SelectedItems;
            });
        }

        protected override DiscordMessageBuilder PopulateMessage(DiscordEmbedBuilder builder)
        {

            builder.WithTitle(Title);
            builder.WithDescription(Common.TrackListString(PageManager.GetPage(CurrentPage)));
            List<DiscordEmbed> embeds = new List<DiscordEmbed>();
            embeds.Add(builder.Build());

            if (Embeds != null)
                embeds.AddRange(Embeds);

            return new DiscordMessageBuilder().AddEmbeds(embeds);
        }
    }
}
