using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgbot_testApi
{
    internal class TrackInfo
    {
        public string TrackTitle { get; set; }
        public string ArtistName { get; set; }

        public string TrackId { get; set;}
    }

    internal class TracksList()
    {
        public Dictionary<int, TrackInfo> tracks = new Dictionary<int, TrackInfo>();

        public void AddTrack(int trackKey,string trackId, string trackTitle, string artistName)
        {
            tracks.Add(trackKey, new TrackInfo {TrackTitle = trackTitle, ArtistName = artistName, TrackId = trackId });

        }
        public TrackInfo GetTrackInfo(int trackKey)
        {
            if (tracks.ContainsKey(trackKey))
            {
                return tracks[trackKey];

            }
            else
            {
                return null;
            }

        }


        
    }
    
}
