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




namespace tgbot_testApi
{
    internal class Program
    {
        private readonly HttpClient client;
        static string  token = "y0_AgAAAAAhA-lPAAG8XgAAAADRAzhwMJfIR5gWTGaeeX27VkE5XiImapI";

        private enum BotState
        {
            Main,
            SearchMusic,
            OutOfSearch


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
                var trackId = callbackQueary.Data;

                var downloadUrl = GetUrlForDownloadTrack(trackId);

                if (downloadUrl != null)
                {
                    //var res = await UploadTrackToApis(downloadUrl);

                    //var urlTrack = res.Substring(res.LastIndexOf("fileUrl") + 10);
                    //var url = urlTrack.Substring(0, urlTrack.Length - 2);
                    var urlCover = "https://avatars.yandex.net/get-music-content/10210263/31c6830d.a.28033940-1/100x100";
                    await bot.SendDocumentAsync(
                        callbackQueary.Message.Chat.Id,
                        InputFileUrl.FromUri(downloadUrl),thumbnail: InputFile.FromUri(urlCover) );

                    //await bot.SendTextMessageAsync(callbackQueary.Message.Chat.Id, );
                }
                else
                {
                    await bot.SendTextMessageAsync(callbackQueary.Message.Chat.Id,"ошибка в handlecallback");
                }

               //var track =  DataBase.GetTitleTrack(callbackQueary.Data);
                //var path = await GetDownload(bot,callbackQueary, track);
                //if (path != string.Empty && path != null)
                //{
                //    byte[] fileContent = System.IO.File.ReadAllBytes(path);

                //    await bot.SendAudioAsync(
                //        callbackQueary.Message.Chat.Id,
                //        InputFile.FromStream(new MemoryStream(fileContent))
                //        );
                //}

            }


        }
        private static async Task HandleMessage(ITelegramBotClient bot, Message message)
        {
            var msg = message.Text;
            if (message != null)
            {
                if (msg == "/start")
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "Здарова. Ботик тест api привествует", replyMarkup: ButtonMarkup());
                    currentState = BotState.Main;

                }
                else if (msg == "Поиск трека")
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "Напиши имя артиста или название трека");
                    currentState = BotState.SearchMusic;
                }
                else if (msg == "Выйти из поиска")
                {
                    currentState = BotState.Main;
                    await bot.SendTextMessageAsync(message.Chat.Id,"вы вышли из поиска");

                }

                else if (currentState == BotState.SearchMusic)
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
                        await bot.SendTextMessageAsync(message.Chat.Id, msg, replyMarkup: GetButtonTrack(tracksInfo, message.Chat.Id.ToString()));

                    }
                    else { await bot.SendTextMessageAsync(message.Chat.Id,"Ничего не нашлось"); }


                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "ошибка");
                }

            }


        }
        //private static async Task<string> GetDownload(ITelegramBotClient bot,CallbackQuery? callback  , string track)
        //{
        //    //var url = await GetMusic(track);
            

        //    Random rnd = new();

        //    var filename = $"{track}{rnd.Next(0, 1000)}";
        //    var directoryPath = string.Empty;

        //    if (url != string.Empty) {
                

        //        try
        //        {
        //            //directoryPath = $@"/root/bot2/storage/{filename}.mp3";
        //            //directoryPath = $@"C:\Users\кирилл\Desktop\storagesrab\{filename}.mp3";
        //           directoryPath = $@"C:\Users\porka\OneDrive\Рабочий стол\str\{filename}.mp3";
        //            using (HttpClient client = new HttpClient())
        //            {
        //                try
        //                {
        //                    HttpResponseMessage response = await client.GetAsync(url);

        //                    if (response.IsSuccessStatusCode)
        //                    {
        //                        //using (FileStream fileStream = System.IO.File.Create(directoryPath))
        //                        //{
        //                        //    await (await response.Content.ReadAsStreamAsync()).CopyToAsync(fileStream);
        //                        //}

        //                        TracksList tracksInfo =  GetInfoTrackOnYandex(track);



        //                        if (tracksInfo != null)
        //                        {
        //                           url =  GetUrlForDownloadTrack(idTrack);

        //                            var result = await UploadTrackToApis(url);

        //                            if (result != null)
        //                            {

        //                                object a = JsonConvert.DeserializeObject(result);
        //                                string b = JsonConvert.SerializeObject(a, Formatting.Indented);
        //                                await bot.SendTextMessageAsync(callback.Message.Chat.Id, b);
        //                            }

        //                        }


                               

        //                        return directoryPath;
        //                    }
        //                    else
        //                    {
        //                        await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Ошибка: " + response.StatusCode);
        //                        return directoryPath;
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Ошибка скачивания файла: " + e.Message);
        //                }

        //            }



        //        }
        //        catch (Exception)
        //        {
        //            await bot.SendTextMessageAsync(callback.Message.Chat.Id, "ошибка в загрузке файла");
        //            throw;
        //        }
        //    }
        //    else if (url == string.Empty || url != null)
        //    {
        //        await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Ничего не нашлось");
                
        //    }
            


        //    return directoryPath;
        //}

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

        static string GetUrlForDownloadTrack(string trackId)
        {
            
            NetworkParams networkParams = new NetworkParams() { };
            Album albumApi = new(networkParams, token);
            Track trackApi = new Track(networkParams, token);


            var downloadInfo =  trackApi.GetDownloadInfoWithToken(trackId).Result;
            var downloadInfoUrl = downloadInfo["result"][0]["downloadInfoUrl"].ToString();
            var directLink = trackApi.GetDirectLink(downloadInfoUrl).Result;

            return directLink;

            //var bestsTrack = albumApi.GetTracks(5568718).Result["result"]["bests"].ToList(); // Get the top 5 tracks from the Evolve - Imagine Dragons album
            //foreach (var i in bestsTrack)
            //{
            //    var downloadInfo = trackApi.GetDownloadInfo(i.ToString()).Result; // Get a list of available codecs and bitrates

            //    var downloadInfoUrl = downloadInfo["result"][0]["downloadInfoUrl"].ToString(); // Get the necessary string to get a direct link to the track

            //    var directLink = trackApi.GetDirectLink(downloadInfoUrl).Result; // Getting a direct link to the track (30 second demo)

            //    Console.WriteLine(directLink);
            //}

            

           

        }


        static TracksList GetInfoTrackOnYandex(string titleTrack)
        {
            TracksList tracks = new TracksList();
            List<string> trackInfo = new List<string>();
            int countTracks = 0;

            NetworkParams networkParams = new NetworkParams() { };
            Default defApi = new(networkParams, token);

            var searchResult = defApi.Search(titleTrack, typeSearch: "track").Result;



            if (searchResult != null && searchResult["result"] != null && searchResult["result"]["tracks"] != null && searchResult["result"]["tracks"]["results"] != null)
            {
                var tracksResults = searchResult["result"]["tracks"]["results"];


                foreach (var item in tracksResults)
                {
                    tracks.AddTrack(countTracks, item["id"].ToString(), item["title"].ToString(), item["artists"][0]["name"].ToString());

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


        private static InlineKeyboardMarkup GetButtonTrack(TracksList tracksInfo,string chatid )
        {
              
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < tracksInfo.tracks.Count; i++)
            {
                TrackInfo infoTracks = tracksInfo.GetTrackInfo(i);

                //var identTrack = Guid.NewGuid().ToString("N");
                var buttonText = infoTracks.TrackTitle + " " + infoTracks.ArtistName;
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


        private static ReplyKeyboardMarkup ButtonMarkup()
        {

            ReplyKeyboardMarkup reply = new(new[]
            {
                new KeyboardButton ( "Поиск трека" ),
                new KeyboardButton( "Выйти из поиска" ),

            })
            {
                ResizeKeyboard = true
            };

            return reply;



        }
    }


}
