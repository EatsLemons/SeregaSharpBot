using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SeregaBot.Application.Discord;
using SeregaBot.Application.Twitch;
using TwitchLib.Api;

namespace SeregaBot.Application
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string twitchSecret = Environment.GetEnvironmentVariable("TWITCH_SECRET");
            string twitchClientId = Environment.GetEnvironmentVariable("TWITCH_CLIENTID");
            string discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            string discordChannel = Environment.GetEnvironmentVariable("DISCORD_CHANNEL");
            string[] streamers = Environment.GetEnvironmentVariable("STREAMERS")?.Split(',');
            
            var tw = new TwitchAPI();
            tw.Settings.Secret = twitchSecret;
            tw.Settings.ClientId = twitchClientId;

            var dsc = new Notifier(discordToken, discordChannel);
            var streamChecker = new StreamChecker(tw, streamers);

            try
            {
                await foreach (string streamer in streamChecker.WaitForStreamerOnline())
                {
                    Console.WriteLine($"{DateTime.Now} streamer {streamer} online");
                    await dsc.SendNotifyAsync(streamer, $"Запустил свой стрим");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}