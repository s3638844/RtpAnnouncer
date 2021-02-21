using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#pragma warning disable 1998

namespace RtpAnnouncer.Bots.Commands

{
    public class EnrollmentModule : BaseCommandModule
    {
        /// <summary>
        ///     Check if role exists for monitoring and if not, create it within teh appropriate guild
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private DiscordRole GetRoles(CommandContext ctx)
        {
            var roles = ctx.Guild.Roles;
            DiscordRole monitorRole = null;
            // ITERATE OVER GUILD ROLES TO CHECK IF ROLE EXISTS, IF NOT CREATE IT
            Parallel.ForEach(roles, role =>
            {
                var (_, value) = role;
                if (value.Name.ToLower().Equals("queue monitor")) monitorRole = value;
            });
            return monitorRole;
        }

        [Command("register")]
        [Description("This will register you in the monitor group and enable alerting for FiveM/FDG queuing")]
        public async Task GreetCommand(CommandContext ctx)
        {
            _ = ctx.RespondAsync("I will notify you when you have joined the server");
            var role = GetRoles(ctx);
            if (role != null) _ = ctx.Member.GrantRoleAsync(role, "for monitoring presence");
        }

        [Command("deregister")]
        [Description("This will deregister you from alerts relating to FiceM/FDG queuing")]
        public async Task StopCommand(CommandContext ctx)
        {
            _ = ctx.RespondAsync("I will leave you alone now");
            var role = GetRoles(ctx);
            if (role != null) _ = ctx.Member.RevokeRoleAsync(role, "monitoring of presence no longer required");
        }
    }
}