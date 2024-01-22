using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
            bot.StartReceiving(Update,Error);
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
                    await bot.SendTextMessageAsync(message.Chat.Id,"Здарова. Ботик тест api привествует", replyMarkup: ButtonMarkup());
                    currentState = BotState.Main;

                }
                else if (msg == "Поиск трека")
                {
                    await bot.SendTextMessageAsync(message.Chat.Id,"Напиши имя артиста или название трека");
                    currentState = BotState.SearchMusic;
                }
                else if (msg == "Выйти из поиска")
                {
                    currentState = BotState.Main;

                }

                else if (currentState == BotState.SearchMusic)
                {
                   var res = await GetMusic(msg);
                    await bot.SendTextMessageAsync(message.Chat.Id,$"{res}");
                    currentState = BotState.Main;
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat.Id,"ошибка");
                }

            }


        }
        async private static Task<string> GetMusic(string term)
        {
            
            
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
