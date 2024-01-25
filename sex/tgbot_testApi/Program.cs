using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using HtmlAgilityPack;
using System.Net;



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

                    GetDownload(bot, message, msg);



                    //await bot.SendAudioAsync(
                    //    message.Chat.Id,
                    //   audio: InputFile.FromUri("https://vk.com/audio-222136525_456239020_bcff1dcc7a3bbb685f")
                    //    );

                    currentState = BotState.Main;
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "ошибка");
                }

            }


        }
        private static async void GetDownload(ITelegramBotClient bot, Message message, string track)
        {


            try
            {
                var url = await GetMusic(track);
                using (var client = new HttpClient())
                {
                    Random rnd = new();
                    var filename = $"{track}{rnd.Next(0,1000)} ";
                    var directoryPath = $@"C:\Users\кирилл\Desktop\storagesrab\{filename}";
                    Uri serverUri = new Uri("https://vk.com/audio-222136525_456239020_fd36c70d87e617429b");
                    Uri relative = new Uri(directoryPath,UriKind.Relative);
                    Uri full = new Uri(serverUri, relative);
                    using (var response = await client.GetAsync(full, HttpCompletionOption.ResponseHeadersRead))

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = System.IO.File.Create(directoryPath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }

                    await bot.SendTextMessageAsync(message.Chat.Id, $"файл скачан и сохранен  на {directoryPath}");


                }
            }
            catch (Exception)
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "ошибка в загрузке файла");
                throw;
            }





            //using (var stream = System.IO.File.OpenRead(tempFile))
            //{
            //    var input = new InputFileStream(stream);
            //    var mesage = await bot.SendAudioAsync(message.Chat.Id, input);
            //}


        }
        async private static Task<string> GetMusic(string track)
        {


            HtmlWeb web = new HtmlWeb();

            HtmlDocument document = web.Load($"https://web.ligaudio.ru/mp3/{track}");
            HtmlDocument site2 = web.Load($"https://rus.hitmotop.com/search?q={track}");
            var firstvideo = document.DocumentNode.SelectSingleNode($"(//a[@itemprop='url'])[1]");
            string bebra = string.Empty;
            if (firstvideo.Attributes["href"].Value.EndsWith(".mp3"))
            {
                bebra = firstvideo.Attributes["href"].Value + "?play";

            }

            return bebra.Substring(2);

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
