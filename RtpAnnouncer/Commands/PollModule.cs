using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace RtpAnnouncer.Commands
{
    public class PollModule : BaseCommandModule
    {
        [Command("poll")]
        [Description("Create a poll for voting on.  Specify time to run ie. 10s (10 seconds), emoji for voting, and what the poll is")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var optionString = emojiOptions.Select(x => x.ToString());
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Poll", //@TODO let user choose title.
                Description = string.Join(" ", optionString)
            };
            
            var pollMessage = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration)
                .ConfigureAwait(false);
        
            var results = result.Distinct().Select(x => $"{x.Emoji}:{x.Total}");
            
            //@TODO: improve this so that it shows the original poll description
            await ctx.Channel.SendMessageAsync("Result from Poll \n" + string.Join("\n", results))
                .ConfigureAwait(false);
        }
        
    }
}