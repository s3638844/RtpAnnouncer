using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using RtpAnnouncer.Bots.Commands;

namespace RtpAnnouncer.Bots.Attributes
{
    
    // only usable on methods and classes and not usable multiple times on a function
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequireCategoriesAttribute : CheckBaseAttribute
    {

        public IReadOnlyList<string> CategoryNames { get; }
        public ChannelCheckMode CheckMode { get;  }
        
        public RequireCategoriesAttribute(ChannelCheckMode checkMode, params string[] channelNames)
        {
            CheckMode = checkMode;
            CategoryNames = new ReadOnlyCollection<string>(channelNames);
        }
        
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
            {
                return Task.FromResult(false);
            }

            var contains = CategoryNames.Contains(ctx.Channel.Parent.Name, StringComparer.OrdinalIgnoreCase);

            return CheckMode switch
            {
                // if category name is within collection of allowed names true, else false.  If nothing then also false
                ChannelCheckMode.Any => Task.FromResult(contains),
                ChannelCheckMode.None => Task.FromResult(!contains),
                _ => Task.FromResult(false),
            };

        }
    }
}