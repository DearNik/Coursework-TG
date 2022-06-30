/*using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;

namespace GoogleDriveTgBot
{
    internal class TelegramBotHelper
    {
        public int i = 0;
        public int filenum = 0;
        private const string textb1 = "Список файлов";
        private const string textb2 = "🔍 Поиск файла";
        private const string textb3 = "Открыть доступ";
        private const string textb4 = "Удалить файл";
        private string _token;
        Telegram.Bot.TelegramBotClient _client;
        private Dictionary<long,UserState> _clientFileRename = new Dictionary<long, UserState>();

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
            IList<Google.Apis.Drive.v3.Data.File> files = null;
            GoogleOAuth gd = new GoogleOAuth();
            gd.GoogleDriveOAuth(out files);
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var text = update.Message.Text;
                    string imagePath = null;
                    var delete = _clientFileRename.ContainsKey(update.Message.Chat.Id) ? _clientFileRename[update.Message.Chat.Id] : null;
                    if (delete != null)
                    {
                        //_client.SendTextMessageAsync(update.Message.Chat.Id, $"Новое имя файла {files[filenum].Name}: {text}", replyMarkup: GetButtons());
                        if (text.Equals("Назад"))
                        {
                            break;
                        }
                        else
                        {
                            switch (text)
                            {
                                case "Да":
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, $"Файл {files[filenum].Name} был успешно удален", replyMarkup: GetButtons());
                                    gd.DeleteFile(files[filenum].Id);
                                    text = "Назад";
                                    break;
                                case "Нет":
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, $"Вы отменили удаление", replyMarkup: GetButtons());
                                    text = "Назад";
                                    break;

                                default:
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Не верная команда");
                                    break;

                            }
                        }


                    }
                    else
                    {
                        switch (text)
                        {
                            case textb1:
                                List<IReplyMarkup> fileslistbutton = new List<IReplyMarkup>();
                                foreach (var file in files)
                                {
                                    imagePath = Path.Combine(Environment.CurrentDirectory, "pdf.png");
                                    using (var stream = File.OpenRead(imagePath))
                                    {
                                        var r = _client.SendPhotoAsync(update.Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream), caption: $"Имя файла - {file.Name}\nhttps://drive.google.com/file/d/{file.Id}", replyMarkup: GetInLineButtons2(1)).Result;
                                        fileslistbutton.Add(GetInLineButtons(1));
                                        i += 2;
                                    }
                                }
                                break;
                            case textb2:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Введите имя файла : ", replyMarkup: GetButtons());
                                break;
                            case textb3:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Выберите файл для откытия доступа ", replyMarkup: GetButtons());
                                break;
                            case textb4:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Та нечего удалять еще ", replyMarkup: GetButtons());
                                break;
                            case "/start":
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Добропожаловать в Бот для роботы с ГуглДрайвом ", replyMarkup: GetButtons());
                                break;
                            default:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Не извесная команда");
                                break;
                        }
                    }
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                    switch (update.CallbackQuery.Data)
                    {
                        case "1":
                            filenum = 0;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "2":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 0;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "3":
                            filenum = 1;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "4":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 1;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "5":
                            filenum = 2;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "6":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 2;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "7":
                            filenum = 3;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "8":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 3;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "9":
                            filenum = 4;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "10":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 4;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "11":
                            filenum = 5;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "12":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 5;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "13":
                            filenum = 6;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "14":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 6;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "15":
                            filenum = 7;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "16":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 7;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "17":
                            filenum = 8;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "18":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 8;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "19":
                            filenum = 9;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "20":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 9;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "21":
                            filenum = 10;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "22":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 10;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "23":
                            filenum = 11;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "24":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 11;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "25":
                            filenum = 12;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "26":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 12;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "27":
                            filenum = 13;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "28":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 13;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "29":
                            filenum = 14;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "30":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 14;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
                        case "31":
                            filenum = 15;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Открыт доступ к файу - {files[filenum].Name}", replyMarkup: GetButtons());
                            gd.InsertPermissionAnyone(gd.GetService(), files[filenum].Id);
                            break;
                        case "32":
                            _clientFileRename[update.CallbackQuery.Message.Chat.Id] = new UserState { State = State.FileRename };
                            filenum = 15;
                            _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Вы уверены что хотите удалить файл - {files[filenum].Name}", replyMarkup: GetDelButtons());
                            break;
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
                    new List<KeyboardButton> { new KeyboardButton { Text = "Да" }, new KeyboardButton { Text = "Нет" } },
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
                    new List<KeyboardButton> { new KeyboardButton { Text = textb3 }, new KeyboardButton { Text = textb4 } }
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
            //return new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "Открыть", CallbackData = Id.ToString() });
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
            //return new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "Открыть", CallbackData = Id.ToString() });
        }
    }
}
*/