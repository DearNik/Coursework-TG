using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace GoogleDriveTgBot
{
    class TelegramBot
    {
        private const string token = "5497447474:AAGXVEsDHDTWlg5yJ4D-FGWiXXFuOByrJBw";
        static void Main(string[] args)
        {
            try
            {
                TelegramBotHelper tg = new TelegramBotHelper(token);
                tg.GetUpdates();
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}
