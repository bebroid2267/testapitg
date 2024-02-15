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
        public static readonly string connectionString = @"Data Source = /root/bot2/dbForMusicBot.db";

        private static bool IfExistTrack(string id)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = new SqliteCommand();
                command.Connection = connection;
                
                command.CommandText = $"select  count(*) FROM MetadataTracks WHERE Id LIKE '{id}'";

                var count = (long)command.ExecuteScalar();
                if (count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public static bool AddMetadataTrack(string title, string artist, string pictureUrl,string id)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = new SqliteCommand();
                command.Connection = connection;

                if (IfExistTrack(id) == false)
                {
                    command.CommandText = $"insert into MetadataTracks(Title, Artist, Id, PictureUrl) values (@Title, @Artist, @Id, @PictureUrl)";

                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Artist", artist);
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@PictureUrl", pictureUrl);
                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                else
                {
                    return false;
                }
                
            }


        }
        public static List<string> GetMetadataTrack(string id)
        {
            List<string> metadataTrack = new List<string>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = new SqliteCommand();  
                command.Connection = connection;

                command.CommandText = $"select * FROM MetadataTracks WHERE Id LIKE '{id}'";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    for (int i = 0; i < 4; i++)
                    {
                        metadataTrack.Add(reader.GetString(i));
                    }
                    
                }
                connection.Close();
                return metadataTrack;
            }

        }


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
        public static bool AddToLikeTracks(string chatId, string trackId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = new SqliteCommand();
                command.Connection = connection;

                if (IfExistsTrackLike(chatId,trackId) == false)
                {
                    command.CommandText = $"insert into LikeTracks (ChatId,TrackId) values (@ChatId, @TrackId)";

                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@TrackId", trackId);

                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                else
                {
                    return false;
                }


            }

        }
        public  static bool DeleteLikeTrack(string chatId, string trackId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = new SqliteCommand();
                command.Connection = connection;
                if (IfExistsTrackLike(chatId, trackId) == true)
                {
                    command.CommandText = $"DELETE FROM LikeTracks WHERE ChatId LIKE '{chatId}' AND TrackId LIKE '{trackId}'";

                    command.ExecuteNonQuery();
                    return true;
                }
                else
                {
                    return false;

                }

                



            }


        }

        public static List<string> GetLikeTrack(string chatId)
        {
            List<string> idTracks = new List<string>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT TrackId FROM LikeTracks WHERE ChatId LIKE '{chatId}'";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    idTracks.Add(reader.GetString(0));
                }
                connection.Close();
                return idTracks;

            }
        
        }

        public static bool IfExistsTrackLike(string chatId, string trackId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT COUNT(*) FROM LikeTracks WHERE ChatId LIKE '{chatId}' AND TrackId LIKE '{trackId}'";

                var count = (long)command.ExecuteScalar();
                if (count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }


        }

    }
}
