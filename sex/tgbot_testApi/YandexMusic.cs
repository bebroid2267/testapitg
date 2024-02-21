using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YandexMusicApi.Network;
using YandexMusicApi.Api;

namespace tgbot_testApi
{
    static class YandexMusic
    {
        public static string GetUrlForDownloadTrack(string trackId)
        {

            var downloadInfo = track.GetDownloadInfoWithToken(trackId).Result;

            if (downloadInfo["result"] != null && downloadInfo["result"][0] != null && downloadInfo["result"][0]["downloadInfoUrl"] != null)
            {
                var downloadInfoUrl = downloadInfo["result"][0]["downloadInfoUrl"].ToString();
                var directLink = track.GetDirectLink(downloadInfoUrl).Result;

                return directLink;
            }
            else
            {
                return null;
            }


        }

        public async static Task<TracksList> GetInfoBestTracksArtistOnYandex(string artistId)
        {
            TracksList idTracks = new TracksList();
            int artId = int.Parse(artistId);
            int countTracks = 0;

            var idsInfo = await artist.GetTracks(artId);

            if (idsInfo["result"] != null && idsInfo["result"]["tracks"] != null)
            {
                var bestTracks = idsInfo["result"]["tracks"];

                foreach (var track in bestTracks)
                {
                    idTracks.AddTrack(countTracks, track["id"].ToString(), track["title"].ToString(), track["artists"][0]["name"].ToString());

                    DataBase.AddMetadataTrack(track["title"].ToString(), track["artists"][0]["name"].ToString(), "https://" + track["coverUri"].ToString().Replace("%%", "100x100"), track["id"].ToString());

                    countTracks++;
                }
                return idTracks;
            }

            else
            { return null; }

        }

        public static TracksList GetInfoArtistsOnYandex(string titleTrack)
        {
            TracksList artists = new TracksList();

            int countArtist = 0;


            var searhResultArtist = defApi.Search(titleTrack, typeSearch: "artist").Result;



            if (searhResultArtist != null && searhResultArtist["result"] != null && searhResultArtist["result"]["artists"] != null && searhResultArtist["result"]["artists"]["results"] != null)
            {
                var artistResult = searhResultArtist["result"]["artists"]["results"];


                foreach (var item in artistResult)
                {
                    artists.AddTrack(countArtist, item["id"].ToString(), item["name"].ToString(), item["name"].ToString());

                    var coverUri = string.Empty;

                    if (item["cover"] != null && item["cover"]["uri"] != null)
                    {
                        coverUri = "https://" + item["cover"]["uri"].ToString().Replace("%%", "100x100");
                    }
                    DataBase.AddMetadataTrack(item["name"].ToString(), item["name"].ToString(), coverUri, item["id"].ToString());
                    countArtist++;
                }
                return artists;
            }
            else
            { return artists; }


        }

        public static TracksList GetInfoTrackOnYandex(string titleTrack)
        {
            TracksList tracks = new TracksList();
            List<string> trackInfo = new List<string>();
            int countTracks = 0;

            var searchResult = defApi.Search(titleTrack, typeSearch: "track").Result;





            if (searchResult != null && searchResult["result"] != null && searchResult["result"]["tracks"] != null && searchResult["result"]["tracks"]["results"] != null)
            {
                var tracksResults = searchResult["result"]["tracks"]["results"];


                foreach (var item in tracksResults)
                {

                    tracks.AddTrack(countTracks, item["id"].ToString(), item["title"].ToString(), item["artists"][0]["name"].ToString());


                    DataBase.AddMetadataTrack(item["title"].ToString(), item["artists"][0]["name"].ToString(), "https://" + item["coverUri"].ToString().Replace("%%", "100x100"), item["id"].ToString());
                    countTracks++;
                }
                return tracks;

            }
            else
            {
                return null;
            }


        }


        static readonly string token = "y0_AgAAAAAhA-lPAAG8XgAAAADRAzhwMJfIR5gWTGaeeX27VkE5XiImapI";

        private static readonly NetworkParams networkParams = new NetworkParams() { };

        private static readonly Default defApi = new(networkParams, token);

        private static readonly Track track = new(networkParams, token);

        private static readonly Artist artist = new Artist(networkParams, token);
    }
}
