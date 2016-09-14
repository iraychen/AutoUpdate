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
        private static string dbname;
        private static SQLiteConnection db;
        public int YearMonth { get; }
        private static object _lock = new object();
        public SQLiteHelper()
        {

        }

        public static void Init(DirectoryInfo dir)
        {
            CoreFileCopy();

            if (!dir.Exists)
            {
                dir.Create();
            }
            dbname = Path.Combine(dir.FullName, "AutoUpdate.db");
            db = new SQLiteConnection(dbname);
            CreateTables();
        }

        private static void CoreFileCopy()
        {
            var in64Bit = (IntPtr.Size == 8);
            File.WriteAllBytes("sqlite3.dll",
                in64Bit ? Properties.Resources.sqlite3_X64 : Properties.Resources.sqlite3_X86);
        }

        public static void CreateTables()
        {
            lock (_lock)
            {
                db.CreateTable<UserModel>("User");
                db.CreateTable<VersionModel>("Version");
                db.CreateTable<HospitalModel>("Hospital");
                db.CreateTable<DLLModel>("DLL");
            }
        }

        public static List<UserModel> UserQuery(int limit, string name = null)
        {
            var query = db.Table<UserModel>();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name == name);
            }
            return query.Take(limit).OrderBy(info => info.LastLoginTime).ToList();
        }

        public static List<VersionModel> VersionQuery(int limit, int hospitalID)
        {
            var query = db.Table<VersionModel>();
            query = query.Where(p => p.HospitalID == hospitalID);
            return query.Take(limit).ToList();
        }
        public static List<VersionModel> VersionQuery(string ID)
        {
            var query = db.Table<VersionModel>();
            query = query.Where(p => p.ID == ID);
            return query.ToList();
        }

        public static List<DLLModel> DLLQuery(string versionID)
        {
            var query = db.Table<DLLModel>();
            query = query.Where(p => p.VersionID == versionID);
            return query.ToList();
        }

        public static List<HospitalModel> HospitalQuery(int limit, string name = null)
        {
            var query = db.Table<HospitalModel>();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name == name);
            }
            return query.Take(limit).ToList();
        }

        public static List<HospitalModel> HospitalQuery(int ID)
        {
            var query = db.Table<HospitalModel>();
            query = query.Where(p => p.ID == ID);
            return query.ToList();
        }

        public static bool Insert<T>(T info)
        {
            try
            {
                lock (_lock)
                {
                    db.Insert(info);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Insert<T>(IEnumerable<T> info)
            where T : DLLModel
        {
            try
            {
                lock (_lock)
                {
                    db.InsertAll(info);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Logger.log.Error("写数据库错误:" + e.Message + e.StackTrace);
                return false;
            }
        }

        public static bool Insert<T>(string tableName, T[] info)
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

        public static bool Update<T>(T info)
        {
            try
            {
                db.Update(info);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Delete<T>(T info)
        {

            try
            {
                db.Delete(info);
                return true;
            }
            catch (Exception)
            {
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