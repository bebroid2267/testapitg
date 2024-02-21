using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using HtmlAgilityPack;
using System.Net;
using System.Linq;
using System.IO;
using Telegram.Bots.Requests;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using System.Text;
using YandexMusicApi.Api;
using YandexMusicApi.Network;
using Newtonsoft.Json.Linq;
using TagLib;
using System.Runtime.CompilerServices;




namespace tgbot_testApi
{
    internal class Program
    {
        private readonly HttpClient client;
        static string  token = "y0_AgAAAAAhA-lPAAG8XgAAAADRAzhwMJfIR5gWTGaeeX27VkE5XiImapI";

        public static NetworkParams networkParams = new NetworkParams() { };
        public  static Default defApi = new(networkParams, token);
        public static Track track = new(networkParams,token);
        public static Artist artist = new Artist(networkParams, token);

        private enum BotState
        {
            Main,
            SearchMusic,
            OutOfSearch,
            MyTracks


        }
        private static BotState currentState;
        static void Main(string[] args)
        {
            var bot = new TelegramBotClient("6743819346:AAG93-5vfAVPX8n7yr3pb6s9OFY2gTirl6Q");
            bot.StartReceiving(Update, Error);
            Console.ReadLine();
        }


        async private static Task Update(ITelegramBotClient bot, Update update, CancellationToken cts) 
        {
            var message = update.Message;
            if (update.Type == UpdateType.Message && update?.Message?.Text != null || update.Type == UpdateType.CallbackQuery)
            {
                if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                {
                    await HandleMessage(bot,update.Message);
                }
                if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallBackQueary(bot, update.Message, update.CallbackQuery);
                }

            }


        }

        async static Task HandleCallBackQueary(ITelegramBotClient bot,Message message, CallbackQuery? callbackQueary)
        {
            if (callbackQueary.Data != null)
            {

                if (callbackQueary.Data.StartsWith("like"))
                {
                    var trackId = callbackQueary.Data.Substring(5);

                    var res = DataBase.AddToLikeTracks(callbackQueary.Message.Chat.Id.ToString(), trackId);

                        if (res)
                        {
                            await bot.SendTextMessageAsync(callbackQueary.Message.Chat.Id, "<i>👀 Трек добавлен в избранные!</i>" ,parseMode: ParseMode.Html);

                            var path = await GetDownload(bot, callbackQueary, trackId);
                        
                                if (path != string.Empty && path != null)
                                {
                                    byte[] fileContent = System.IO.File.ReadAllBytes(path);

                                }

                        }

                        else if (res == false)
                        {


                            await bot.SendTextMessageAsync(callbackQueary.Message.Chat.Id, "<i>⛔️ Трек уже есть в избранных!</i>", parseMode: ParseMode.Html);

                        }

                }

                else if (callbackQueary.Data.StartsWith("Dis"))
                {
                    var trackId = callbackQueary.Data.Substring(8);
                    //var path = $@"C:\Users\porka\source\repos\testapitg\sex\tgbot_testApi\tracksStorage\{trackId}.mp3";
                    var path = $"/root/trackStorage/{trackId}.mp3";

                    var res = DataBase.IfExistsTrackLike(callbackQueary.Message.Chat.Id.ToString(), trackId);

                        if (res)
                        {     
                                await bot.SendTextMessageAsync(callbackQueary.Message.Chat.Id, "<i>⚡️Трек был успешно убран из избранных!</i>", parseMode: ParseMode.Html);

                                DataBase.DeleteLikeTrack(callbackQueary.Message.Chat.Id.ToString(),trackId);

                                System.IO.File.Delete(path);


                        }
                        else if (res == false)
                        {
                                await bot.SendTextMessageAsync(callbackQueary.Message.Chat.Id, "<i>❗️Трек отсутствует в избранных</i>",parseMode: ParseMode.Html);

                        }

                }
                else if (callbackQueary.Data.StartsWith("artist"))
                {
                    var artistId = callbackQueary.Data.Substring(7);
                   TracksList tracks = await GetInfoBestTracksArtistOnYandex(artistId);
                    var artistInfo = DataBase.GetMetadataTrack(artistId);
                    if (tracks != null)
                    {
                        await bot.SendPhotoAsync(callbackQueary.Message.Chat.Id, InputFile.FromUri(new Uri(artistInfo[3])),caption:  artistInfo[1], replyMarkup: GetButtonTrack(tracks));

                    }
                    else { await bot.SendTextMessageAsync(message.Chat.Id, "Ничего не нашлось"); }

                }

                else if (!callbackQueary.Data.Contains("like") && !callbackQueary.Data.Contains("artist"))
                {
                        var trackId = callbackQueary.Data;
                    

                        if (DataBase.IfExistsTrackLike(callbackQueary.Message.Chat.Id.ToString(),trackId))
                        {
                                //var pathh = $@"C:\Users\porka\source\repos\testapitg\sex\tgbot_testApi\tracksStorage\{trackId}.mp3";
                                var pathh = $"/root/trackStorage/{trackId}.mp3";

                                byte[] fileContent = System.IO.File.ReadAllBytes(pathh);

                                var urlPic = DataBase.GetMetadataTrack(callbackQueary.Data);

                                await bot.SendAudioAsync(
                                    callbackQueary.Message.Chat.Id,
                                    InputFile.FromStream(new MemoryStream(fileContent)), thumbnail: InputFile.FromUri(urlPic[3]), replyMarkup: GetButtonLikeTrack(trackId)
                                    );
                        }
                        else
                        {
                                var path = await GetDownload(bot, callbackQueary, trackId);

                                if (path != string.Empty && path != null)
                                {
                                        byte[] fileContent = System.IO.File.ReadAllBytes(path);

                                        var urlPic = DataBase.GetMetadataTrack(callbackQueary.Data);

                                        await bot.SendAudioAsync(
                                            callbackQueary.Message.Chat.Id,
                                            InputFile.FromStream(new MemoryStream(fileContent)), thumbnail: InputFile.FromUri(urlPic[3]), replyMarkup: GetButtonLikeTrack(trackId)
                                            );
                                        System.IO.File.Delete(path);

                                }
                        }
                    
                    
                }        

            }

        }

        private static async Task HandleMessage(ITelegramBotClient bot, Message message)
        {
            var msg = message.Text;
            if (message != null)
            {
                if (msg == "/start")
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "<b>🎧 Это -  бот для скачивания и прослушивания музыки! \r\n\r\nПиши:\r\n⚡️ Ник артиста \r\n " +
                        "      Или\r\n⚡️Название трека, альбома\r\n\r\n✨Также ты можешь сохранять музыку прямо в боте!" +
                        "  - Для этого нажми кнопку снизу.</b>", replyMarkup: ButtonMarkup(),parseMode: ParseMode.Html);
                    

                }
                
                
                else if (msg == "Мои треки")
                {
                    List<string> idTracks = DataBase.GetLikeTrack(message.Chat.Id.ToString());

                    if (idTracks.Count > 0)
                    {
                        int count = 0;

                        TracksList tracks = new TracksList();

                        foreach (var trackId in idTracks)
                        {
                            var infoTrack = DataBase.GetMetadataTrack(trackId);
                            tracks.AddTrack(count, infoTrack[2], infoTrack[0], infoTrack[1]);
                            count++;
                        }
                        await bot.SendTextMessageAsync(message.Chat.Id, "Ваши треки", replyMarkup: GetButtonTrack(tracks,msg));
                    }
                    else {
                        await bot.SendTextMessageAsync(message.Chat.Id, "<i>\U0001f976 Пока что, вам не понравился ни один трек. \r\nИсправим?😏 </i>",parseMode: ParseMode.Html);
                    }
                    

                }
                

                else  
                {
                    
                    string AllTracks = string.Empty;

                    TracksList tracksInfo = GetInfoTrackOnYandex(msg);

                    


                    //foreach (var track in GetTenTracks(msg))
                    //{
                    //    tracks.Add(track);
                    //    AllTracks += track + "\n";

                    //}
                    if (tracksInfo != null)
                    {
                        await bot.SendTextMessageAsync(message.Chat.Id, msg, replyMarkup: GetButtonTrack(tracksInfo, msg));

                    }
                    else { await bot.SendTextMessageAsync(message.Chat.Id,"Ничего не нашлось"); }


                }
                

            }


        }
        private static async Task<string> GetDownload(ITelegramBotClient bot, CallbackQuery? callback, string trackId)
        {
            var url = GetUrlForDownloadTrack(trackId);

            
            var filename = trackId;
            var directoryPath = string.Empty;

            if (url != string.Empty && url != null)
            {


                try
                {
                    
                    //directoryPath = $@"C:\Users\кирилл\Desktop\storagesrab\{filename}.mp3";
                    //directoryPath = $@"C:\Users\porka\source\repos\testapitg\sex\tgbot_testApi\tracksStorage\{filename}.mp3";
                    directoryPath = $"/root/trackStorage/{trackId}.mp3";
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            HttpResponseMessage response = await client.GetAsync(url);

                            if (response.IsSuccessStatusCode)
                            {


                                using (FileStream fileStream = System.IO.File.Create(directoryPath))
                                {
                                    await (await response.Content.ReadAsStreamAsync()).CopyToAsync(fileStream);
                                }

                               List<string> metadataTracks =  DataBase.GetMetadataTrack(trackId);
                                var tagFile = TagLib.File.Create(directoryPath);
                                tagFile.Tag.Title = metadataTracks[0];
                                tagFile.Tag.Performers = new string[] { metadataTracks[1] };
                                
                                tagFile.Save();
                                
                                client.Dispose();
                                return directoryPath;
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Ошибка: " + response.StatusCode);
                                client.Dispose();
                                return directoryPath;
                            }
                        }
                        catch (Exception e)
                        {
                            await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Ошибка скачивания файла: " + e.Message);
                        }

                    }



                }
                catch (Exception)
                {
                    await bot.SendTextMessageAsync(callback.Message.Chat.Id, "ошибка в загрузке файла");
                    throw;
                }
            }
            else if (url == string.Empty || url != null)
            {
                await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Ничего не нашлось");

            }



            return directoryPath;
        }

        private static List<string> GetTenTracks(string track)
        {
            List<string> tracks = new List<string>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load($"https://rus.hitmotop.com/search?q={track}") ;
            int countTracks = 1;

            var nodeTitleTrack = document.DocumentNode.SelectSingleNode($"(//div[@class='track__title'])[{countTracks}]");
            var nodeArtistName = document.DocumentNode.SelectSingleNode($"(//div[@class='track__desc'])[{countTracks}]");
            string res = string.Empty;

            if (nodeTitleTrack != null && nodeArtistName != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    nodeTitleTrack = document.DocumentNode.SelectSingleNode($"(//div[contains(@class,'title')])[{countTracks}]");
                    nodeArtistName = document.DocumentNode.SelectSingleNode($"(//div[contains(@class,'desc')])[{countTracks}]");
                    
                    if (nodeTitleTrack != null && nodeArtistName != null)
                    {
                        res = nodeTitleTrack.InnerText.Trim() + " "+ nodeArtistName.InnerText.Trim();
                        
                        tracks.Add(res.TrimStart());
                    }
                    countTracks++;
                }
            }
            
            
            
            return tracks;
        }



        // private static  string GetMusic(string track)
        //{


        //    HtmlWeb web = new HtmlWeb();

        //     HtmlDocument document = web.Load($"https://web.ligaudio.ru/mp3/{track}");
        //    HtmlDocument site2 = web.Load($"https://rus.hitmotop.com/search?q={track}");

        //    var firstvideo = document.DocumentNode.SelectSingleNode($"(//a[@itemprop='url'])[1]");
        //    var twirdVideo = site2.DocumentNode.SelectSingleNode($"//a[contains(@href,'.mp3')]");
            
        //    string bebra = string.Empty;
        //    string bebra2 = string.Empty;
            
        //    if (twirdVideo != null)
        //    {
        //        if (twirdVideo.Attributes["href"].Value.EndsWith("mp3"))
        //        {
        //            bebra2 = twirdVideo.Attributes["href"].Value;
        //        }
        //    }
            
        //    return bebra2;

        //}

        private static string GetUrlForDownloadTrack(string trackId)
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

            

            //var bestsTrack = albumApi.GetTracks(5568718).Result["result"]["bests"].ToList(); // Get the top 5 tracks from the Evolve - Imagine Dragons album
            //foreach (var i in bestsTrack)
            //{
            //    var downloadInfo = trackApi.GetDownloadInfo(i.ToString()).Result; // Get a list of available codecs and bitrates

            //    var downloadInfoUrl = downloadInfo["result"][0]["downloadInfoUrl"].ToString(); // Get the necessary string to get a direct link to the track

            //    var directLink = trackApi.GetDirectLink(downloadInfoUrl).Result; // Getting a direct link to the track (30 second demo)

            //    Console.WriteLine(directLink);
            //}

        }
        private async static Task<TracksList> GetInfoBestTracksArtistOnYandex(string artistId)
        {
            TracksList idTracks = new TracksList();
            int artId = int.Parse(artistId);
            int countTracks = 0;

            var idsInfo =  await artist.GetTracks(artId);

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

        private static TracksList GetInfoArtistsOnYandex(string titleTrack)
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
                        DataBase.AddMetadataTrack(item["name"].ToString(), item["name"].ToString(),coverUri , item["id"].ToString());
                    countArtist++;
                    }
                    return artists;
                }
                else 
            { return artists; }


        }

        private static  TracksList GetInfoTrackOnYandex(string titleTrack)
        {
            TracksList tracks = new TracksList();
            List<string> trackInfo = new List<string>();
            int countTracks = 0;

            var searchResult =  defApi.Search(titleTrack, typeSearch: "track").Result;
            

            
            

                if (searchResult != null && searchResult["result"] != null && searchResult["result"]["tracks"] != null && searchResult["result"]["tracks"]["results"] != null)
                {
                    var tracksResults = searchResult["result"]["tracks"]["results"];

                    
                        foreach (var item in tracksResults)
                        {

                                tracks.AddTrack(countTracks, item["id"].ToString(), item["title"].ToString(), item["artists"][0]["name"].ToString());
                                

                                DataBase.AddMetadataTrack(item["title"].ToString(), item["artists"][0]["name"].ToString(),  "https://" + item["coverUri"].ToString().Replace("%%", "100x100"), item["id"].ToString());
                                countTracks++;
                        }
                        return tracks;

                }
                else
                {
                    return null;
                }
            

        }

        private static async Task<string> UploadTrackToApis(string url1)
        {
            
            Uri url2 = new Uri(url1);

            string url = "https://api.bytescale.com/v2/accounts/12a1yp1/uploads/url";
            string apiKey = "secret_12a1yp129VfCdeyLwi88YwFgHRuQ";
            string requestBody = $"{{\"url\": \"{url2}\"}}";


            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

                var content = new System.Net.Http.StringContent(requestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);
                

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                   
                    client.Dispose();

                    return result;
                }
                else
                {
                    string errorResult = await response.Content.ReadAsStringAsync();

                    client.Dispose();

                    return ("Error: " + errorResult);
                }
            }


        }

        private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        

        private static InlineKeyboardMarkup GetButtonLikeTrack(string trackId)
        {
            List<InlineKeyboardButton[]> button = new List<InlineKeyboardButton[]>();

            button.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "❤️", "like " + trackId),
                InlineKeyboardButton.WithCallbackData(text: "💔", "Dislike " + trackId)
            });
            return new InlineKeyboardMarkup(button);
        }

        
        

        private static InlineKeyboardMarkup GetButtonTrack(TracksList tracksInfo, string nameTrackOrArtist )
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            TracksList artistsList = GetInfoArtistsOnYandex(nameTrackOrArtist);

            for (int j = 0; j < artistsList.tracks.Count; j++)
            {
                if (j <= 5)
                {
                    TrackInfo artistsInfo = artistsList.GetTrackInfo(j);

                    var btnText = "👤 " + artistsInfo.TrackTitle;
                    string clbackData = "artist " + artistsInfo.TrackId;

                    buttonRows.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: btnText,clbackData)
                    });
                }
                
            }

            for (int i = 0; i < tracksInfo.tracks.Count; i++)
            {
                
                

                TrackInfo infoTracks = tracksInfo.GetTrackInfo(i);

                //var identTrack = Guid.NewGuid().ToString("N");
                var buttonText = "🎧 " +  infoTracks.TrackTitle + " " + infoTracks.ArtistName;
                string callbackData = infoTracks.TrackId;

                //if (identTrack.Length >= 20 )
                //{
                //    callbackData = identTrack.Substring(0,20);
                //}

                buttonRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonText,callbackData)
                } );
                    

                //if (DataBase.AddTrack(tracks[i], callbackData, chatid) == true)
                //{
                //    buttonRows.Add(new[]
                //{
                //    InlineKeyboardButton.WithCallbackData(text: buttonText, callbackData)

                //});


                //}
                 
            }
            return new InlineKeyboardMarkup(buttonRows);

        }
        private static InlineKeyboardMarkup GetButtonTrack(TracksList tracksInfo)
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            

            for (int i = 0; i < tracksInfo.tracks.Count; i++)
            {



                TrackInfo infoTracks = tracksInfo.GetTrackInfo(i);

               
                var buttonText = "🎧 " + infoTracks.TrackTitle + " " + infoTracks.ArtistName;
                string callbackData = infoTracks.TrackId;

                

                buttonRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonText,callbackData)
                });


                

            }
            return new InlineKeyboardMarkup(buttonRows);

        }


        private static ReplyKeyboardMarkup ButtonMarkup()
        {

            ReplyKeyboardMarkup reply = new(new[]
            {
                
                new KeyboardButton( "Мои треки" ),

            })
            {
                ResizeKeyboard = true
            };

            return reply;



        }
    }


}
