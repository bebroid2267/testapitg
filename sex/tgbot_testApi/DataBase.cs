using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;

namespace tgbot_testApi
{
    internal static class DataBase
    {
        //public static readonly string connectionString = @"Data Source = C:\Users\porka\OneDrive\Рабочий стол\dbForMusicBot.db";
        public static readonly string connectionString = @"Data Source = C:\Users\кирилл\Desktop\dbForMusicBot.db";

        public static string GetTitleTrack(string identif)
        {
            string title = string.Empty;
            using (var connection = new SqliteConnection(connectionString))
            {
                
                    connection.Open();
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    
                    command.CommandText = $"select TitleTrck FROM Tracks WHERE IdentifTrack LIKE   '{identif}'";
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        title = reader.GetString(0);
                    }
                    connection.Close();
                    return title;
                


            }


        }
        public static bool AddTrack(string title, string identif, string ChatId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                
                    connection.Open();
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = $"insert into Tracks(TitleTrck,IdentifTrack,ChatId) values (@Title, @Identif,@ChatId)";
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@Identif",identif);
                command.Parameters.AddWithValue("@ChatId", ChatId);
                command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                
                
            }


        }
    }
}
