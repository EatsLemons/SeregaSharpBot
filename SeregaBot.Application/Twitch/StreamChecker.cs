using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.V5.Models.Streams;
using TwitchLib.Api.V5.Models.Users;

namespace SeregaBot.Application.Twitch
{
    public class StreamChecker
    {
        private readonly TwitchAPI _twitch;
        
        /// <summary>
        /// login => channelId
        /// </summary>
        private Dictionary<string, string> _streamers = new Dictionary<string, string>();

        public StreamChecker(TwitchAPI twitch, IReadOnlyList<string> streamers)
        {
            _twitch = twitch;

            foreach (string streamer in streamers)
            {
                _streamers.Add(streamer, string.Empty);
            }
        }

        public async IAsyncEnumerable<string> WaitForStreamerOnline()
        {
            await FindChannelsIds();

            List<string> streamerIds = _streamers.Values.ToList();
            Dictionary<string, bool> liveStatus = new Dictionary<string, bool>();

            foreach (string channelId in _streamers.Values)
            {
                liveStatus.Add(channelId, true);
            }

            while (true)
            {
                LiveStreams liveStreams = null;
                try
                { 
                    liveStreams = await _twitch.V5.Streams.GetLiveStreamsAsync(streamerIds);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
                
                var liveStreamsIds = liveStreams.Streams.Select(s => s.Channel.Id);
                foreach (string channelId in _streamers.Values)
                {
                    if (!liveStreamsIds.Contains(channelId))
                    {
                        liveStatus[channelId] = false;
                        continue;
                    }

                    if (!liveStatus[channelId])
                    {
                        liveStatus[channelId] = true;
                        yield return _streamers.First(kvp => kvp.Value == channelId).Key;
                    }
                }

                await Task.Delay(30_000);
            }
        }

        private async Task FindChannelsIds()
        {
            for (var i = 0; i < _streamers.Count; i++)
            {
                var streamer = _streamers.Keys.ToArray()[i];

                Users usersResp = await _twitch.V5.Users.GetUserByNameAsync(streamer);
                if (usersResp.Total > 0)
                {
                    _streamers[streamer] = usersResp.Matches[0].Id;
                }
            }
        }
    }
}