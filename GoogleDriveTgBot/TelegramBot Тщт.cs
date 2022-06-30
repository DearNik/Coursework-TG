/*using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace GoogleDriveTgBot
{
    internal class TelegramBot
    {

        private const string token = "5430389947:AAFhnONgcR5TArYi5buBG0scPoEsbda_Yg8";

        public string apikey = "AIzaSyAxenoj-I9n-dZXMBoDsm6fPyC3T_2zt8k";

        private static TelegramBotClient client;

        private const string textb1 = "Список файлов";
        private const string textb2 = "🔍 Поиск файла";
        private const string textb3 = "Открыть доступ";
        private const string textb4 = "Удалить файл";

        [Obsolete]
        static void Main(string[] args)
        {
            client = new TelegramBotClient(token) { Timeout = TimeSpan.FromSeconds(10) };
            var me = client.GetMeAsync().Result;
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadKey();
            client.StopReceiving();
        }
        [Obsolete]

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            IList<Google.Apis.Drive.v3.Data.File> files = null;
            GoogleOAuth gd = new GoogleOAuth();
            gd.GoogleDriveOAuth(out files);

            if (msg.Text != null)
            {
                Console.WriteLine($"Пришло сообщение {msg.Text}");
                switch (msg.Text)
                {
                    case "Список файлов":
                        foreach (var file in files)
                        {
                            var messeg = client.SendTextMessageAsync(msg.Chat.Id, $"Имя файла - {file.Name}\nhttps://drive.google.com/file/d/{file.Id}");
                        }  
                        // Display the results.
                        break;
                    case "Назад":
                        var mes = client.SendTextMessageAsync(msg.Chat.Id, "Вы попали в меню");
                        break;
                    case "Поиск Файла":
                        var mesfind = client.SendTextMessageAsync(msg.Chat.Id, "Вы попали в меню");
                        break;
                    case "Открыть доступ по ссылке":
                        break;
                }
            }
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = textb1 }, new KeyboardButton { Text = textb2 } },
                    new List<KeyboardButton> { new KeyboardButton { Text = textb3 }, new KeyboardButton { Text = textb4 } }
                },
                ResizeKeyboard = true
            };
        }
    }
}
*/