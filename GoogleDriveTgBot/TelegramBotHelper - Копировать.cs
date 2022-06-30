using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;

namespace GoogleDriveTgBot
{
    internal class TelegramBotHelper
    {
        public int i = 0;
        public int filenum = 0;
        private const string textb1 = "🗓 Список файлов";
        private const string textb2 = "🔍 Поиск файла";
        private const string textb3 = "Открыть доступ";
        private const string textb4 = "Удалить файл";
        private const string bin = "🗑 Да";
        private const string arr = "⏪ Нет";
        private string _token;
        public bool search1 = false;
        public bool delete1 = false;
        Telegram.Bot.TelegramBotClient _client;
        private Dictionary<long, UserState> _clientFileRename = new Dictionary<long, UserState>();
        private Dictionary<long, UserState> _clientSearch = new Dictionary<long, UserState>();

        IList<Google.Apis.Drive.v3.Data.File> files = null;
        GoogleOAuth gd = null;
        public TelegramBotHelper(string token)
        {
            this._token = token;
        }

        internal void GetUpdates()
        {
            _client = new Telegram.Bot.TelegramBotClient(_token);
            var me = _client.GetMeAsync().Result;
            if (me != null & !string.IsNullOrEmpty(me.Username))
            {
                int offset = 0;
                while (true)
                {
                    try
                    {
                        var updates = _client.GetUpdatesAsync(offset).Result;
                        if (updates != null && updates.Count() > 0)
                        {
                            foreach (var update in updates)
                            {
                                processUpdate(update);
                                offset = update.Id + 1;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }

                    Thread.Sleep(1000);
                }
            }
        }
        private void processUpdate(Telegram.Bot.Types.Update update)
        {
            if (gd == null)  gd = new GoogleOAuth(); 
            if (files == null) gd.GoogleDriveOAuth(out files); 
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var text = update.Message.Text;
                    bool needupdate = true;
                    string imagePath = null;
                    var delete = _clientFileRename.ContainsKey(update.Message.Chat.Id) ? _clientFileRename[update.Message.Chat.Id] : null;
                    var search = _clientSearch.ContainsKey(update.Message.Chat.Id) ? _clientSearch[update.Message.Chat.Id] : null;
                    if (delete1 == false) { delete = null; };if (search1 == false) { search = null; };
                    bool result = false;
                    if (search != null && text != bin && text != arr && text != textb1 && text != textb2)
                    {
                        delete1 = true;
                        i = 0;
                        List<IReplyMarkup> fileslistbutton = new List<IReplyMarkup>();
                        foreach (var file in files)
                        {
                            Regex regex = new Regex(text.ToString());
                            MatchCollection matches = regex.Matches(@$"(\w*){file.Name}(\w *)");
                            if (matches.Count > 0)
                            {
                                int stringleight = file.Name.LastIndexOf('.');
                                string format = file.Name.Substring(0, stringleight);
                                format = file.Name.Remove(0, stringleight);
                                if (format == ".pdf")
                                {
                                    string imagePathPdf = Path.Combine(Environment.CurrentDirectory, "pdf.png");
                                    imagePath = imagePathPdf;
                                }
                                else if (format == ".docx")
                                {
                                    string imagePathDocx = Path.Combine(Environment.CurrentDirectory, "docx.png");
                                    imagePath = imagePathDocx;
                                }
                                else if (format == ".jpg")
                                {
                                    string imagePathJpg = Path.Combine(Environment.CurrentDirectory, "jpg.png");
                                    imagePath = imagePathJpg;
                                }
                                else if (format == ".png")
                                {
                                    string imagePathPng = Path.Combine(Environment.CurrentDirectory, "png.png");
                                    imagePath = imagePathPng;
                                }
                                else
                                {
                                    string imagePathPdf = Path.Combine(Environment.CurrentDirectory, "pdf.png");
                                    imagePath = imagePathPdf;
                                }
                                using (var stream = File.OpenRead(imagePath))
                                {
                                    var r = _client.SendPhotoAsync(update.Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream), caption: $"Имя файла - {file.Name}\nhttps://drive.google.com/file/d/{file.Id}", replyMarkup: GetInLineButtons2(1)).Result;
                                }
                                result = true;
                            }
                            fileslistbutton.Add(GetInLineButtons(1));
                            i += 2;

                        }
                        if (result == false)
                        {
                            _client.SendTextMessageAsync(update.Message.Chat.Id, $"Не найдено совпадений , попробуйте снова", replyMarkup: GetButtons());
                            search1 = false;
                            break;
                        }
                        else if(result == true)
                        {
                            _client.SendTextMessageAsync(update.Message.Chat.Id, $"Все что было найдено по клуч. слову {text}", replyMarkup: GetButtons());
                            search1 = false;
                            break;
                        }
                        break;
                    }
                    if (delete != null && text != textb1 && text != textb2)
                    {
                        search1 = true;
                        needupdate = false;
                        //_client.SendTextMessageAsync(update.Message.Chat.Id, $"Новое имя файла {files[filenum].Name}: {text}", replyMarkup: GetButtons());
                        switch (text)
                        {
                            case bin:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Файл {files[filenum].Name} был успешно удален", replyMarkup: GetButtons());
                                gd.DeleteFile(files[filenum].Id);
                                needupdate = true;
                                text = textb1;
                                break;
                            case arr:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Вы отменили удаление", replyMarkup: GetButtons());
                                break;

                            default:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Не верная команда");
                                break;

                        }
                        delete1 = false;
                    }
                    if (needupdate)
                    {
                        delete1 = true;
                        search1 = true;
                        switch (text)
                        {
                            case textb1:
                                i = 0;
                                List<IReplyMarkup> fileslistbutton = new List<IReplyMarkup>();
                                gd.GoogleDriveOAuth(out files);
                                foreach (var file in files)
                                {
                                    int stringleight = file.Name.LastIndexOf('.');
                                    string format = file.Name.Substring(0, stringleight);
                                    format = file.Name.Remove(0, stringleight);
                                    if (format == ".pdf")
                                    {
                                        string imagePathPdf = Path.Combine(Environment.CurrentDirectory, "pdf.png");
                                        imagePath = imagePathPdf;
                                    }
                                    else if (format == ".docx")
                                    {
                                        string imagePathDocx = Path.Combine(Environment.CurrentDirectory, "docx.png");
                                        imagePath = imagePathDocx;
                                    }
                                    else if (format == ".jpg")
                                    {
                                        string imagePathJpg = Path.Combine(Environment.CurrentDirectory, "jpg.png");
                                        imagePath = imagePathJpg;
                                    }
                                    else if (format == ".png")
                                    {
                                        string imagePathPng = Path.Combine(Environment.CurrentDirectory, "png.png");
                                        imagePath = imagePathPng;
                                    }
                                    else
                                    {
                                        string imagePathPdf = Path.Combine(Environment.CurrentDirectory, "pdf.png");
                                        imagePath = imagePathPdf;
                                    }
                                    using (var stream = File.OpenRead(imagePath))
                                    {
                                        var r = _client.SendPhotoAsync(update.Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream), caption: $"Имя файла - {file.Name}\nhttps://drive.google.com/file/d/{file.Id}", replyMarkup: GetInLineButtons2(1)).Result;
                                        fileslistbutton.Add(GetInLineButtons(1));
                                        i += 2;
                                    }
                                }
                                break;
                            case textb2:
                                _clientSearch[update.Message.Chat.Id] = new UserState { State = State.Search };
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Введите имя файла : ", replyMarkup: GetButtons());
                                break;
                            case "/start":
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"🔥 Добро пожаловать в Бот для роботы с GoogleDrive 🔥", replyMarkup: GetButtons());
                                break;
                            default:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Не извесная команда", replyMarkup: GetButtons());
                                break;
                        }
                    }
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                    {

                        int button = int.Parse(update.CallbackQuery.Data)-1;
                        filenum = button/2;
                        switch (button%2)
                        {
                            case 0:
                                
                                _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                                gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                                break;
                            case 1:
                                _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                                _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                                break;
                        }
                    }
                    break;
                default:
                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Не извесная команда");
                    break;

            }
        }

        private IReplyMarkup GetDelButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = "🗑 Да" }, new KeyboardButton { Text = "⏪ Нет" } },
                },
                ResizeKeyboard = true
            };
        }

        private IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = textb1 }, new KeyboardButton { Text = textb2 } },
                },
                ResizeKeyboard = true
            };
        }

        private IReplyMarkup GetInLineButtons(int Id)
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            buttons.Add(new InlineKeyboardButton { Text = $"Отркыть доступ", CallbackData = Id.ToString() }); ;
            buttons.Add(new InlineKeyboardButton { Text = $"Удалить", CallbackData = (Id + 1).ToString() });

            var menue = new List<InlineKeyboardButton[]>();
            menue.Add(new[] { buttons[0], buttons[1] });

            var menu = new InlineKeyboardMarkup(menue.ToArray());
            return menu;
        }
        private IReplyMarkup GetInLineButtons2(int Id)
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            buttons.Add(new InlineKeyboardButton { Text = $"Отркыть доступ", CallbackData = (Id + i).ToString() }); ;
            buttons.Add(new InlineKeyboardButton { Text = $"Удалить", CallbackData = (Id + i + 1).ToString() });

            var menue = new List<InlineKeyboardButton[]>();
            menue.Add(new[] { buttons[0], buttons[1] });

            var menu = new InlineKeyboardMarkup(menue.ToArray());
            return menu;
        }
    }
}


