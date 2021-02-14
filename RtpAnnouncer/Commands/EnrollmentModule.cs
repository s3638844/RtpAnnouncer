using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Interactivity.Extensions;

namespace RtpAnnouncer.Commands

{
    public class EnrollmentModule : BaseCommandModule
    {

        /// <summary>
        /// Check if role exists for monitoring and if not, create it within teh appropriate guild
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private async Task<DiscordRole> getRoles(CommandContext ctx)
        {
            var roles = ctx.Guild.Roles;
            var foundRole = false;
            DiscordRole monitorRole = null;
            // ITERATE OVER GUILD ROLES TO CHECK IF ROLE EXISTS, IF NOT CREATE IT
            Parallel.ForEach(roles, (role) =>
            {
                if (role.Value.Name.ToLower().Equals("monitor"))
                {
                    foundRole = true;
                    monitorRole = role.Value;
                }

            });
            if (!foundRole)
            {
                monitorRole = await ctx.Guild.CreateRoleAsync("monitor", Permissions.None, DiscordColor.Goldenrod, true, false, "monitor the fiveM queue simulator");
            }

            return monitorRole;
            
        }




        //TODO: add exception handling, recheck these

        [Command("register")]
        [Description("This will register you in the monitor group and enable alerting for FiveM/FDG queuing")]
        public async Task GreetCommand(CommandContext ctx)
        {
            _ = ctx.RespondAsync("I will notify you when you have joined the server");
            var roleTask = getRoles(ctx);
            var role = await roleTask;
            _ = ctx.Member.GrantRoleAsync(role, "for monitoring presence");
        }

        [Command("deregister")]
        [Description("This will deregister you from alerts relating to FiceM/FDG queuing")]
        public async Task StopCommand(CommandContext ctx)
        {
            _ = ctx.RespondAsync("I will leave you alone now");
            var roleTask = getRoles(ctx);
            var role = await roleTask;
            _ = ctx.Member.RevokeRoleAsync(role, "monitoring of presence no longer required");
        }


        
        
        
        [Command("response")]
        public async Task Response(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(message.Result.Emoji);
            
        }
    }
}