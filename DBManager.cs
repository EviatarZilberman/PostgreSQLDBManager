﻿using ConfigApp;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PostgreSQLDBManager
{
    public class DBManager
    {
        public static NpgsqlConnection? ConnectionString { get; set; } = null;
        private static DBManager? instance = null;

        private DBManager() { }
        public static DBManager Instance()
        {
            if (instance == null)
            {
                instance = new DBManager();
                ConnectionString = new NpgsqlConnection(CreateConnectionString());
            }
            return instance;
        }

        private static CoreReturns StringValidation(string? s)
        {
            return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s) ? CoreReturns.IS_NULL_OR_EMPTY : CoreReturns.SUCCESS;
        }

        private async static Task<CoreReturns> ExecuteQuery(string? query, string queryName)
        {
            if (StringValidation(query) != CoreReturns.SUCCESS) return CoreReturns.IS_NULL_OR_EMPTY;
            query = Colboinik.ValidateQuery(query, true);
            try
            {
                await ConnectionString.CloseAsync(); // Opens the connectionstring.
                await ConnectionString.OpenAsync();
                NpgsqlCommand cmd = new NpgsqlCommand(query, ConnectionString);
                int n = cmd.ExecuteNonQuery();
                if (n == 1)
                {
                    await ConnectionString.CloseAsync(); // Varify that the connectionstring is closed.

                    return CoreReturns.SUCCESS;
                }
            }
            catch (Exception ex)
            {
                await ConnectionString.CloseAsync(); // Varify that the connectionstring is closed.
                LogWriter.Instance().WriteLog(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Error in {queryName}: {ex.Message}, query: {query}");
                return CoreReturns.ANOTHER_ERROR_SEE_LOGS;
            }
            return CoreReturns.ERROR;
        }

        public async Task<CoreReturns> Insert(string? query)
        {
            return await ExecuteQuery(query, "Insert");
        }

        public async Task<CoreReturns> Delete(string query)
        {
            return await ExecuteQuery(query, "Delete");
        }

        public async Task<CoreReturns> Update(string query)
        {
            return await ExecuteQuery(query, "Update");
        }

        public async Task<CoreReturns> Create(string query)
        {
            return await ExecuteQuery(query, "Create");
        }

        public static async Task<CoreReturns> TestConnection()
        {
            await using (NpgsqlConnection con = ConnectionString)
            {

                await con.OpenAsync();
                if (con?.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                    return CoreReturns.SUCCESS;
                }
            }
            return CoreReturns.ERROR;
        }

        public static string CreateConnectionString() 
        {
            ConfigurationsKeeper ConfigKeeper = new ConfigurationsKeeper();
            return $@"User ID=postgres;Password=Popmart123!;Server={ConfigKeeper.Data["server"]};Port={ConfigKeeper.Data["dbport"]};Database={ConfigKeeper.Data["database"]};Include Error Detail=true;";
        }



    

/*    class Program
    {
        static void Main()
        {
            string connectionString = "Your PostgreSQL Connection String";
            PostgresHelper postgresHelper = new PostgresHelper(connectionString);

            List<string> results = postgresHelper.SelectData();

            foreach (string result in results)
            {
                Console.WriteLine(result);
            }
        }
    }*/

}
}
