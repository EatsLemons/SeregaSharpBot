using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace SeregaBot.Application.Discord
{
    public class Notifier
    {
        private readonly DiscordClient _discord;
        private DiscordChannel _channel;
        
        public Notifier(string token, string channel)
        {
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
            });
            
            _discord.ConnectAsync().Wait();
            
            _discord.Ready += async args =>
            {
                _channel = await FindNotifyChannel(args.Client.Guilds.Values, channel);
                await Task.Yield();
            };
        }

        public async Task SendNotifyAsync(string rolename, string message)
        {
            if (string.IsNullOrEmpty(rolename)) throw new ArgumentException("rolename");
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("message");
            
            if (_channel == null)
                await Task.Delay(10_000);

            if (!_channel.Guild.Roles.Any(r => r.Name == rolename))
            {
                Console.WriteLine($"{DateTime.Now} role {rolename} not found");
                return;
            }

            ulong roleId = _channel.Guild.Roles.First(r => r.Name == rolename).Id;
            await _channel.SendMessageAsync($"<@&{roleId}> {message}");
        }
        
        private static async Task<DiscordChannel> FindNotifyChannel(IEnumerable<DiscordGuild> guilds, string searchedChann)
        {
            foreach (DiscordGuild guild in guilds)
            {
                foreach (DiscordChannel channel in await guild.GetChannelsAsync())
                {
                    if (channel.Name == searchedChann)
                        return channel;
                }
            }
            
            throw new Exception($"Channel {searchedChann} not found");
        }
    }
}