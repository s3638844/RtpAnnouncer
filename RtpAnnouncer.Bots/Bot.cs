using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RtpAnnouncer.Bots.Commands;

namespace RtpAnnouncer.Bots
{
    public class Bot
    {
        public CommandsNextExtension Commands;

        /// <summary>
        ///     creates bot
        /// </summary>
        /// <param name="services"></param>
        public Bot(IServiceProvider services)
        {
            _ = Main(services);
        }

        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        //public Dictionary<ulong, DiscordMember> Members { get; private set; }
        public DiscordChannel AlertsChannel { get; private set; }
        public DiscordRole MonitorRole { get; private set; }

        public Dictionary<ulong, string> ActivityHistory { get; private set; }

        /// <summary>
        ///     Main application logic
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private async Task Main(IServiceProvider services)
        {
            Console.WriteLine("starting bot");
            string json;

            await using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                json = await sr.ReadToEndAsync();
            }

            var configJson = JsonConvert.DeserializeObject<JSONConfigurator>(json);
            Console.WriteLine("configuring bot");
            var configuration = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot, // is a bot
                MinimumLogLevel = LogLevel.Debug, // once prod set to error
                LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt", // timestamp format for log
                Intents = DiscordIntents.GuildPresences // intents for monitoring presence
                          | DiscordIntents.GuildMessages // sending messages to channel
                          | DiscordIntents.DirectMessages // sending DM to members
                          | DiscordIntents.Guilds // general guild events/info
            };

            Console.WriteLine("creating bot");
            Client = new DiscordClient(configuration);

            Client.Ready += OnClientReady;
            Client.GuildAvailable += OnGuildAvailable;
            Client.PresenceUpdated += OnPresenceUpdated;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] {configJson.Prefix},
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,
                Services = services,
                CaseSensitive = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Console.WriteLine("registering commands for bot");
            Commands.RegisterCommands<EnrollmentModule>();

            Console.WriteLine("commands registered for bot");
            await Client.ConnectAsync().ConfigureAwait(false);
            Console.WriteLine("connected bot");
        }

        /// <summary>
        ///     Returns a Task Completion when called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnClientReady(DiscordClient s, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Sets up the Guild when it connects to this bot
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnGuildAvailable(DiscordClient s, GuildCreateEventArgs e)
        {
            Console.WriteLine("CONNECTED TO GUILD");
            var foundRole = false;
            var roles = e.Guild.Roles;
            var membersKeys = e.Guild.Members.Keys;
            // ITERATE OVER GUILD ROLES TO CHECK IF ROLE EXISTS, IF NOT CREATE IT
            Parallel.ForEach(roles, role =>
            {
                var (_, value) = role;
                if (!value.Name.ToLower().Equals("queue monitor")) return;
                foundRole = true;
                Console.WriteLine(value);
                MonitorRole = value;
            });

            Parallel.ForEach(membersKeys, member =>
            {
                ActivityHistory.Add(member, "");
            });
            
            if (!foundRole)
                MonitorRole = await e.Guild.CreateRoleAsync("queue monitor", Permissions.None, DiscordColor.Goldenrod);

            AlertsChannel = e.Guild.Channels[812917777924489267];
        }


        private async Task OnPresenceUpdated(DiscordClient s, PresenceUpdateEventArgs a)
        {
            var userId = a.User.Id;
            Console.WriteLine(userId);
            Console.WriteLine(a.User.Username);
            var guild = a.User.Presence.Guild;
            var role = a.User.Presence.Guild.Members[userId].Roles.FirstOrDefault(x => x.Name.ToLower() == "queue monitor");
            // check of the user is monitoring queue, status was previpously fivem, and is now in fat fuck gaming
            // @TODO make this work in such a way that the user can specify what is being monitored
            Console.WriteLine(role.Name);
            if (role.Name.ToLower().Equals(MonitorRole.Name.ToLower())
                && a.User.Presence.Activity.Name.ToLower() == "fat duck gaming"
                && ActivityHistory[userId].ToLower() != "fat duck gaming")
            {
                ActivityHistory[userId] = a.User.Presence.Activity.Name.ToLower();
                var userMentionString = a.User.Mention;
                var sb = new StringBuilder();
                sb.Append(userMentionString)
                    .Append(" you are now playing ")
                    .Append(a.User.Presence.Activity.Name);
                var st = sb.ToString();
                Console.WriteLine(st);
                
                // @TODO: find why this doesnt work
                await s.SendMessageAsync(AlertsChannel, st).ConfigureAwait(false);
            }
            else if (role.Name.ToLower().Equals(MonitorRole.Name.ToLower()) &&
                     a.User.Presence.Activity.Name.Length > 1)
            {
                Console.WriteLine("interim update: " + a.User.Presence.Activity.Name);
                ActivityHistory[userId] = a.User.Presence.Activity.Name.ToLower();
            }
        }
    }
}