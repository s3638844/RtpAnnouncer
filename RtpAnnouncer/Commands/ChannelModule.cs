using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace RtpAnnouncer.Commands
{
    public class ChannelModule : BaseCommandModule
    {
        
        [Command("monitor")]
        [Description("Will add you to the queue monitoring channel")]
        public async Task Join(CommandContext ctx)
        {
            var joinEmbed = new DiscordEmbedBuilder
            {
                Title = "Would you like to join the queue watcher?",
                Color = DiscordColor.Goldenrod,
                ImageUrl = ctx.Client.CurrentUser.AvatarUrl,
                Description = "Joining this role will add you to a new channel" +
                              "that monitors your presence to determine when" +
                              "you have finished queueing and joined the FDG server"
            };

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed)
                .ConfigureAwait(false);

            var approveEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var declineEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await joinMessage.CreateReactionAsync(approveEmoji)
                .ConfigureAwait(false);
            await joinMessage.CreateReactionAsync(declineEmoji)
                .ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();
            var result = await interactivity.WaitForReactionAsync(
                x => x.Message == joinMessage &&
                    x.User == ctx.User &&
                     (x.Emoji == approveEmoji || x.Emoji == declineEmoji))
                .ConfigureAwait(false);
            
            var roleKey = findRole(ctx.Guild.Roles);
            if (result.Result.Emoji == approveEmoji)
            {
                // lookup role key and grant user role
                await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(roleKey)).ConfigureAwait(false);
                //@TODO: remove users no emoji
            }
            else if (result.Result.Emoji == declineEmoji)
            {
                await ctx.Member.RevokeRoleAsync(ctx.Guild.GetRole(roleKey)).ConfigureAwait(false);
                //@TODO: remove users yes emoji
            }
            else
            {
                //@TODO: send message calling user an idiot and remove the emoji
            }
        }

        private ulong findRole(IReadOnlyDictionary<ulong, DiscordRole> discordRoles)
        {
            // @TODO investigate better way to do this
            ulong roleKey = 0;
            Parallel.ForEach(discordRoles, pair =>
            {
                if (pair.Value.Name == "queue monitor")
                {
                    roleKey = pair.Key;
                }
            });
            return roleKey;
        }
    }
}