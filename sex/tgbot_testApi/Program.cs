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
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                if (update.Type == UpdateType.Message)
                {
                    await HandleMessage(bot, update.Message);

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

                }

                else if (currentState == BotState.SearchMusic)
                {

                   var path = await GetDownload(bot, message, msg);
                    byte[] fileContent = System.IO.File.ReadAllBytes(path);

                    await bot.SendAudioAsync(
                        message.Chat.Id,
                        InputFile.FromStream(new MemoryStream(fileContent))
                        ) ;
                   
                    currentState = BotState.Main;
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "ошибка");
                }

            }


        }
        private static async Task<string> GetDownload(ITelegramBotClient bot, Message message, string track)
        {
            var url = await GetMusic(track);
            Random rnd = new();
            var filename = $"{track}{rnd.Next(0, 1000)}";
            var directoryPath = $@"C:\Users\кирилл\Desktop\storagesrab\{filename}.mp3";

            try
            {
               
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

                            await bot.SendTextMessageAsync(message.Chat.Id,"Файл успешно скачан и сохранен по пути: " + directoryPath);
                            return directoryPath;
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id,"Ошибка: " + response.StatusCode);
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


            return directoryPath;
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
            if (twirdVideo.Attributes["href"].Value.EndsWith("mp3"))
            {
                bebra2 = twirdVideo.Attributes["href"].Value;
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
