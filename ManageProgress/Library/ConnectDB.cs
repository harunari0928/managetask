﻿using ManageProgress.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ManageProgress.Library
{
    public class ConnectDB
    {
        private string _connectString;
        public ConnectDB(string dbName)
        {
            _connectString = ConfigurationManager.ConnectionStrings[dbName].ConnectionString;
        }

        public List<ProgressModel> GetAllProgresses()
        {
            List<ProgressModel> result = new List<ProgressModel>();
            using (var conn = new SqlConnection(_connectString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"SELECT * FROM [dbo].[Progresses]";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ProgressModel
                        {
                            Id = (int)reader["Id"],
                            Title = (string)reader["Title"],
                            UserId = (string)reader["UserId"],
                            DateTimeRegistered = (DateTime)reader["DateTime_Registered"],
                            NumberOfTask = (int)reader["Number_Of_Items"]
                        });
                    }
                }
            }
            return result;
        }
        public List<ParticipantModel> GetParticipants(int progressId)
        {
            List<ParticipantModel> result = new List<ParticipantModel>();
            using (var conn = new SqlConnection(_connectString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
SELECT *
FROM 
    [dbo].[Participants]
WHERE
    ProgressId = @ProgressId";
                cmd.Parameters.Add(new SqlParameter("@ProgressId", SqlDbType.Int)).Value = progressId;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ParticipantModel
                        {
                            Id = (int)reader["Id"],
                            ProgressId = (int)reader["ProgressId"],
                            ParticipantName = (string)reader["ParticipantName"],
                            CurrentProgress = (int)reader["CurrentProgress"]
                        });
                    }
                }
                return result;

            }
        }
        public List<TaskModel> GetTasks(int progressId)
        {
            List<TaskModel> result = new List<TaskModel>();
            using (var conn = new SqlConnection(_connectString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
SELECT *
FROM 
    [dbo].[Tasks]
WHERE
    ProgressId = @ProgressId";
                cmd.Parameters.Add(new SqlParameter("@ProgressId", SqlDbType.Int)).Value = progressId;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new TaskModel
                        {
                            Id = (int)reader["Id"],
                            ProgressId = (int)reader["ProgressId"],
                            Task = (string)reader["Task"]
                        });
                    }
                }
                return result;
            }
        }

        // TODO: GETProgress(loginId) ログイン中のユーザの進捗のみを表示するメソッドを作る

        public bool IsCorrectPassword(ParticipantModel participant, string inputPassword)
        {
            using (var conn = new SqlConnection(_connectString))
            using (var cmd = conn.CreateCommand())
            {
                try
                {
                    conn.Open();
                    cmd.CommandText = @"
SELECT 
    Password
FROM 
    Progresses
WHERE
    Id = @Id;";
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int)).Value = participant.ProgressId;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if ((string)reader["Password"] == inputPassword)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                catch
                {
                    throw;
                }
            }
        }
        public bool IsExistSameName(ParticipantModel participant)
        {
            using (var conn = new SqlConnection(_connectString))
            using (var cmd = conn.CreateCommand())
            {
                try
                {
                    conn.Open();
                    cmd.CommandText = @"
SELECT
    *
FROM 
    [dbo].[Participants] 
WHERE 
    ParticipantName = @ParticipantName
AND 
    ProgressId = @ProgressId;";
                    cmd.Parameters.Add(new SqlParameter("@ParticipantName", SqlDbType.NVarChar, 50)).Value = participant.ParticipantName;
                    cmd.Parameters.Add(new SqlParameter("@ProgressId", SqlDbType.Int)).Value = participant.ProgressId;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }
        public void SetParticipant(ParticipantModel participant)
        {
            using (var conn = new SqlConnection(_connectString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                try
                {
                    cmd.CommandText = @"
INSERT 
INTO 
[dbo].[Participants]
VALUES
    (@ProgressId, 
    @ParticipantName, 
    @CurrentProgress);";

                    cmd.Parameters.Add(new SqlParameter("@ProgressId", SqlDbType.Int)).Value = participant.ProgressId;
                    cmd.Parameters.Add(new SqlParameter("@ParticipantName", SqlDbType.NVarChar, 50)).Value = participant.ParticipantName;
                    cmd.Parameters.Add(new SqlParameter("@CurrentProgress", SqlDbType.Int)).Value = participant.CurrentProgress;
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }

            }
        }
        public bool SetProgress(ProgressModel progress, List<TaskModel> tasks)
        {
            using (var conn = new SqlConnection(_connectString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                using (var sqlTran = conn.BeginTransaction())
                {
                    try
                    {
                        cmd.Transaction = sqlTran;
                        cmd.CommandText = @"
INSERT 
INTO 
[dbo].[Progresses] 
OUTPUT INSERTED.Id
VALUES
    (@UserId, 
    @Title, 
    @DateTime_Registered,
    @Number_Of_Items, 
    @Password);";
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 50)).Value = progress.UserId;
                        cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50)).Value = progress.Title;
                        cmd.Parameters.Add(new SqlParameter("@DateTime_Registered", SqlDbType.DateTime)).Value = progress.DateTimeRegistered;
                        cmd.Parameters.Add(new SqlParameter("@Number_Of_items", SqlDbType.Int)).Value = progress.NumberOfTask;
                        cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 50)).Value = progress.Password;
                        int newId = (int)cmd.ExecuteScalar();

                        foreach (var task in tasks)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = @"
INSERT
INTO [dbo].[Tasks]
VALUES
    (@ProgressId,
    @Task)";
                            cmd.Parameters.Add(new SqlParameter("@ProgressId", SqlDbType.Int)).Value = newId;
                            cmd.Parameters.Add(new SqlParameter("@Task", SqlDbType.NVarChar)).Value = task.Task;
                            cmd.ExecuteNonQuery();
                        }
                        sqlTran.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {

                        return false;
                    }
                }
            }
        }
        public bool SetTasks(int progressId, List<TaskModel> tasks)
        {
            using (var conn = new SqlConnection(_connectString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                try
                {
                    foreach (var task in tasks)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = @"
INSERT
INTO [dbo].[Tasks]
VALUES
    (@ProgressId,
    @Task)";

                        cmd.Parameters.Add(new SqlParameter("@ProgressId", SqlDbType.Int)).Value = progressId;
                        cmd.Parameters.Add(new SqlParameter("@Task", SqlDbType.Int)).Value = task.Task;
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}