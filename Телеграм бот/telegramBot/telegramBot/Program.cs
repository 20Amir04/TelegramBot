using System.Linq;
using System;
using System.Globalization;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Telegram.Bot.Requests;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Security.Cryptography;

namespace MyApp
{
    internal class Program
    {
        private static long TeacherChatID = 881862474;
        private const string connectionString = "Data Source= C:\\Users\\Amir\\Desktop\\Телеграм бот\\telegramBot\\telegramBot\\students.db";
        private const string TEXT1 = "Узнать мою тему практики";
        private const string TEXT2 = "Изменить мою тему практики";
        private const string TEXT3 = "Закрыть клавиатуру";



        private static HashSet<long> ActiveQuestions = new HashSet<long>();  

        static void Main(string[] args)
        {
                var client = new TelegramBotClient("5851922044:AAE4hGbEzv5nkESC6YNkBJ-AIceP5yEwnuk");
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {

            var message = update.Message;

            if (message == null)
            {
                return;
            }
           
            if (ActiveQuestions.Contains(message.Chat.Id))
            {
                ActiveQuestions.Remove(message.Chat.Id);
                var OldTopic = GetTopic(message.Chat.FirstName);
                UpdateTopic(message.Text, message.Chat.FirstName);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваша тема обновлена!", replyMarkup: GetButtons());
                await botClient.SendTextMessageAsync(TeacherChatID, text:
                    $"Студент '{message.Chat.FirstName}' изменил тему с <{OldTopic}> на <{message.Text}>");  
                return;
            }
            

            if (message.Text.ToLower().Contains("/start"))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать!");
                await botClient.SendStickerAsync(message.Chat.Id, sticker: "https://tlgrm.ru/_/stickers/ccd/a8d/ccda8d5d-d492-4393-8bb7-e33f77c24907/1.webp");
                return;
            }

            else if (message.Text.ToLower().Contains("привет") && message.Chat.Username == "UnknownCustomer")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Здравствуйте, Роман Валентинович!");
                return;
            }

            else if (message.Text.ToLower().Contains("привет"))
            {
                string client = message.Chat.FirstName;

                string[] parse = client.Split("_");

                if (parse.Length != 3)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ваше имя не отвечает стандартам практики!");
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Пример: Пупкин_ИВ_ИПЗ110К");
                    return;
                }

                string name = parse[0] + " " + parse[1];
                string group = parse[2];

                await botClient.SendTextMessageAsync(message.Chat.Id, $"Привет, студент группы {group}, {name}!");
                await botClient.SendTextMessageAsync(message.Chat.Id, text: "Выбери действие: ", replyMarkup: GetButtons());
                return;
            }

                switch (message.Text)
            {
                case TEXT1:      
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: "Ваша тема: " + GetTopic(message.Chat.FirstName), replyMarkup: GetButtons());
                    break;
                case TEXT2:
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: "Отлично :) " , replyMarkup: GetButtons());
                    ActiveQuestions.Add(message.Chat.Id);
                     if (message.Text == TEXT2)
                     {
                        var a = new ReplyKeyboardRemove();
                        await botClient.SendTextMessageAsync(message.Chat.Id, text:
                          "Введите свою новую тему практики: ", replyMarkup: a);
                        return;
                     }
                    break;
                case TEXT3:
                    var b= new ReplyKeyboardRemove();
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: "Надеюсь вы сделали все что хотели)", replyMarkup: b);
                    break;
            }
        
        }

        private static IReplyMarkup? GetButtons()
        {
           ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { TEXT1 },
                        new KeyboardButton[] { TEXT2 },
                        new KeyboardButton[] { TEXT3 },
                    })
            {
                ResizeKeyboard = true
            };
            return replyKeyboardMarkup;
        }

        private static void UpdateTopic(string topic, string firstName)
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "Update studenti SET Topic = @topic WHERE FirstName= @firstName";
            command.Parameters.AddWithValue("@topic", topic);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Prepare();
            command.ExecuteNonQuery();
        }


        private static string GetTopic(string firstName)
        {
           using var connection = new SQLiteConnection(connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "Select topic FROM Studenti WHERE FirstName= @firstName";
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Prepare();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
               return reader.GetString(0);
            }
            return null;
        }

        
        private static Task Error(ITelegramBotClient arg1, Exception error, CancellationToken arg3)
        {
            Console.WriteLine(error.ToString());
            return Task.CompletedTask;
        }      
    }



    
}