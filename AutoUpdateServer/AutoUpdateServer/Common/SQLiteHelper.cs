using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace AutoUpdateServer.Common
{
    public class SQLiteHelper : IDisposable
    {
        private readonly string dbname;
        private SQLiteConnection db;
        public int YearMonth { get; }
        private static object _lock = new object();
        public SQLiteHelper(DirectoryInfo dir)
        {
            CoreFileCopy();

            if (!dir.Exists)
            {
                dir.Create();
            }
            dbname = Path.Combine(dir.FullName, "Users.db");
            db = new SQLiteConnection(dbname);
            CreateTables();
        }

        private static void CoreFileCopy()
        {
            var in64Bit = (IntPtr.Size == 8);
            File.WriteAllBytes("sqlite3.dll",
                in64Bit ? Properties.Resources.sqlite3_X64 : Properties.Resources.sqlite3_X86);
        }

        private void CreateTables()
        {
            db.CreateTable<UserModel>("Users");
        }

        public List<T> Query<T>( int limit,string name=null)
            where T : UserModel, new()
        {
            var query = db.Table<T>();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name == name);
            }
            //if (lastLoginTime != null)
            //    query = query.Where(info => info.LastLoginTime > lastLoginTime.Value);
            return query.Take(limit).OrderBy(info => info.LastLoginTime).ToList();
        }


        public bool Insert<T>(T info)
            where T : UserModel
        {
            try
            {
                lock (_lock)
                {

                    db.Insert(info);
                }
                return true;
            }
            catch (Exception)
            {
                //Logger.log.Error("写数据库错误:" + e.Message + e.StackTrace);
                return false;
            }
        }

        public bool Insert<T>(IEnumerable<T> info)
            where T : UserModel
        {
            try
            {
                lock (_lock)
                {
                    db.InsertAll(info);
                }
                return true;
            }
            catch (Exception)
            {
                //Logger.log.Error("写数据库错误:" + e.Message + e.StackTrace);
                return false;
            }
        }

        public bool Insert<T>(string tableName, T[] info)
           where T : UserModel
        {

            try
            {
                lock (_lock)
                {
                    if (info == null || info.Length == 0)
                    {
                        return false;
                    }
                    if (info.Length == 1)
                    {
                        db.Insert(tableName, info.FirstOrDefault());
                    }
                    else
                    {
                        db.InsertAll(tableName, info);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                //Logger.log.Error("写数据库错误:" + e.Message + e.StackTrace);
                return false;
            }
        }

        public bool Update<T>(T info)
        {
            try
            {
                db.Update(info);
                return true;
            }
            catch (Exception)
            {
                //Logger.log.Error("写数据库错误:" + e.Message + e.StackTrace);
                return false;
            }
        }

        public bool Delete<T>(T info)
        {

            try
            {
                db.Delete(info);
                return true;
            }
            catch (Exception)
            {
                //Logger.log.Error("写数据库错误:" + e.Message + e.StackTrace);
                return false;
            }
        }
        ~SQLiteHelper()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            ((IDisposable)db).Dispose();
        }

    }
}