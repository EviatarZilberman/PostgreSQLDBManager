using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace PostgreSQLDBManager
{
    public class DBManager
    {
        public static NpgsqlConnection ConnectionString { get; } = new NpgsqlConnection(@"User ID=postgres;Password=Popmart123!;Server=localhost;Port=5432;Database=MyLibrary;");
        //public static NpgsqlConnection ConnectionString { get; } = new NpgsqlConnection("Some Password!");
        private static DBManager? instance = null;

        private DBManager() { }

        public static DBManager Instance()
        {
            if (instance == null)
            {
                instance = new DBManager();
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
                    await ConnectionString.CloseAsync();                  // Opens the connectionstring.
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
    }
}
