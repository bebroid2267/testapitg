using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using HtmlAgilityPack;
using System.Net;
using System.Linq;
using Aspose.Html;
using Aspose.Html.Net;
using System.IO;
using Telegram.Bots.Requests;




namespace tgbot_testApi
{
    internal class Program
    {
        private readonly HttpClient client;

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
                var path = await GetDownload(bot,message, callbackQueary.Data);
                if (path != string.Empty && path != null)
                {
                    byte[] fileContent = System.IO.File.ReadAllBytes(path);

                    await bot.SendAudioAsync(
                        callbackQueary.Message.Chat.Id,
                        InputFile.FromStream(new MemoryStream(fileContent))
                        );
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
                    List<string> tracks = new List<string>();
                    string AllTracks = string.Empty;
                    foreach (var track in GetTenTracks(msg))
                    {
                        tracks.Add(track);
                        AllTracks += track + "\n";

                    }

                    await bot.SendTextMessageAsync(message.Chat.Id, $"Треки: \n {AllTracks}", replyMarkup: GetButtonTrack(tracks));

                    //var path = await GetDownload(bot, message, msg);
                    //if (path != string.Empty)
                    //{
                    //    byte[] fileContent = System.IO.File.ReadAllBytes(path);

                    //    await bot.SendAudioAsync(
                    //        message.Chat.Id,
                    //        InputFile.FromStream(new MemoryStream(fileContent))
                    //        );
                    //}




                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "ошибка");
                }

            }


        }
        private static async Task<string> GetDownload(ITelegramBotClient bot,Message message, string track)
        {
            var url = await GetMusic(track);
            Random rnd = new();
            var filename = $"{track}{rnd.Next(0, 1000)}";
            var directoryPath = string.Empty;
            if (url != string.Empty) {
                

                try
                {
                    //directoryPath = $@"/root/bot2/storage/{filename}.mp3";
                    directoryPath = $@"C:\Users\кирилл\Desktop\storagesrab\{filename}.mp3";
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

                                //await bot.SendTextMessageAsync(message.Chat.Id,"Файл успешно скачан и сохранен по пути: " + directoryPath);
                                return directoryPath;
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(message.Chat.Id, "Ошибка: " + response.StatusCode);
                                return directoryPath;
                            }
                        }
                        catch (Exception e)
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, "Ошибка скачивания файла: " + e.Message);
                        }

                    }



                }
                catch (Exception)
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "ошибка в загрузке файла");
                    throw;
                }
            }
            else if (url == string.Empty)
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "Ничего не нашлось");
                
            }
            


            return directoryPath;
        }

         private static List<string> GetTenTracks(string track)
        {
            List<string> tracks = new List<string>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load($"https://rus.hitmotop.com/search?q={track}") ;
            int countTracks = 1;

            var nodes = document.DocumentNode.SelectSingleNode($"(//div[@class='track__title'])[{countTracks}]");

            string res = string.Empty;
            if (nodes != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    nodes = document.DocumentNode.SelectSingleNode($"(//div[contains(@class,'title')])[{countTracks}]");
                    
                    if (nodes != null)
                    {
                        res = nodes.InnerText.Trim() + $" {track}";
                        tracks.Add(res.TrimStart());
                    }
                    countTracks++;
                }
            }
            
            

            return tracks;
        }


        async private static Task<string> GetMusic(string track)
        {


            HtmlWeb web = new HtmlWeb();

            HtmlDocument document = web.Load($"https://web.ligaudio.ru/mp3/{track}");
            HtmlDocument site2 = web.Load($"https://rus.hitmotop.com/search?q={track}");
            var firstvideo = document.DocumentNode.SelectSingleNode($"(//a[@itemprop='url'])[1]");
            var twirdVideo = site2.DocumentNode.SelectSingleNode($"//a[contains(@href,'.mp3')]");
            string bebra = string.Empty;
            string bebra2 = string.Empty;
            //if (firstvideo.Attributes["href"].Value.EndsWith(".mp3"))
            //{
            //    bebra = firstvideo.Attributes["href"].Value + "?play";

            //}
            if (twirdVideo != null)
            {
                if (twirdVideo.Attributes["href"].Value.EndsWith("mp3"))
                {
                    bebra2 = twirdVideo.Attributes["href"].Value;
                }
            }
            
            

            //return "http://"+ bebra.Substring(2);
            return bebra2;

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
        private static InlineKeyboardMarkup GetButtonTrack(List<string> tracks )
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < tracks.Count; i++)
            {
                var buttonText = tracks[i];
                string callbackData = buttonText;
                if (buttonText.Length >=60)
                {
                    callbackData = buttonText.Substring(0,60);
                }
                
                buttonRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonText, callbackData)

                });
                    
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
