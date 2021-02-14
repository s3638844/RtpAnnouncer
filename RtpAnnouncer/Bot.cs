using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RtpAnnouncer.Commands;

namespace RtpAnnouncer
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands;
        public Dictionary<ulong, DiscordMember> Members { get; private set; }
        public DiscordChannel AlertsChannel { get; private set; }
        public DiscordRole MonitorRole { get; private set; }
        
        /// <summary>
        /// creates bot
        /// </summary>
        /// <param name="services"></param>
        public Bot(IServiceProvider services)
        {
            _ = Main(services);
        }
        
        /// <summary>
        /// Main application logic
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private async Task Main(IServiceProvider services) {
        
            string json;

            await using (var fs = File.OpenRead("appsettings.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                json = await sr.ReadToEndAsync();
            }

            var configJson = JsonConvert.DeserializeObject<JSONConfigurator>(json);

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

            Client = new DiscordClient(configuration);

            Client.Ready += OnClientReady;
            Client.GuildAvailable += OnGuildAvailable;
            
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {configJson.Prefix},
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,
                Services = services,
                CaseSensitive = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);


            Commands.RegisterCommands<EnrollmentModule>();
            Commands.RegisterCommands<ChannelModule>();
            Commands.RegisterCommands<PollModule>();
            await Client.ConnectAsync();
        }

        /// <summary>
        /// Returns a Task Completion when called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnClientReady(DiscordClient s, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets up the Guild when it connects to this bot
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnGuildAvailable(DiscordClient s, GuildCreateEventArgs e)
        {
            AlertsChannel = await e.Guild.CreateChannelAsync(name: "queue monitor", ChannelType.Text);
            MonitorRole = await e.Guild.CreateRoleAsync("queue monitor", Permissions.None, DiscordColor.Goldenrod);
            
            var members = e.Guild.Members;
            
            foreach (var (key, value) in members)
            {
                Members.Add(key, value);
            }
          
        }
    }
}
