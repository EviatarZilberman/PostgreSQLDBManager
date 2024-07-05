using ConfigApp;
using Npgsql;
using System.Data;
using UtilitiesAndHelpers.Interfaces;
using UtilitiesAndHelpers.Models;
using UtilitiesAndHelpers.Enums;
using System.Reflection;


namespace PostgreSQLDBManager
{
    public class DBManager<T>
    {
        private static IColboinik _Colboinik { get; set; } = new UtilitiesAndHelpers.Classes.Colboinik();
        //public static NpgsqlConnection ConnectionString { get; set; } = new NpgsqlConnection(CreateConnectionString());
        public static NpgsqlConnection? ConnectionString { get; set; } = null;
        private static DBManager<T>? instance = null;

        private DBManager() { }
        public static DBManager<T> Instance()
        {
            if (instance == null)
            {
                instance = new DBManager<T>();
                ConnectionString = new NpgsqlConnection(CreateConnectionString());
            }
            return instance;
        }

        private static CoreReturns StringValidation(string? s)
        {
            return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s) ? CoreReturns.IS_NULL_OR_EMPTY : CoreReturns.SUCCESS;
        }

        private async static Task<CoreReturns> ExecuteQuery(string? query, string? queryName)
        {
            if (StringValidation(query) != CoreReturns.SUCCESS) return CoreReturns.IS_NULL_OR_EMPTY;
            //query = _Colboinik.ValidateQuery(query, true);
            try
            {
                await ConnectionString.CloseAsync();
                await ConnectionString.OpenAsync(); // Opens the connectionstring.
                NpgsqlCommand cmd = new (query, ConnectionString);
                int n = await cmd.ExecuteNonQueryAsync();
                if (n == 1)
                {
                    await ConnectionString.CloseAsync(); // Varify that the connectionstring is closed.

                    return CoreReturns.SUCCESS;
                }
            }
            catch (Exception ex)
            {
                await ConnectionString.CloseAsync(); // Varify that the connectionstring is closed.
                //LogWriter.Instance().WriteLog(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Error in {queryName}: {ex.Message}, query: {query}");
                return CoreReturns.ANOTHER_ERROR_SEE_LOGS;
            }
            return CoreReturns.ERROR;
        }

        private async static Task<T?> ExecuteSelectQuery<T>(string? query) where T : class,  ISql<T>, new()
        {
            if (StringValidation(query) != CoreReturns.SUCCESS) return default;
            //query = _Colboinik.ValidateQuery(query, true);
            try
            {
                await ConnectionString.CloseAsync();
                await ConnectionString.OpenAsync(); // Opens the connectionstring.
                NpgsqlCommand cmd = new (query, ConnectionString);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    T result = new();
                    T mappedResult = result.CreateInstanceFromMap(reader);
                    await ConnectionString.CloseAsync();
                    return mappedResult;
                }
            }
            catch (Exception ex)
            {
                await ConnectionString.CloseAsync(); // Verify that the connectionstring is closed.
                LogWriter.Instance().WriteLog(MethodBase.GetCurrentMethod()?.Name, $"Error in SelectFactory: {ex.Message}, query: {query}");
            }
            return default;
        }


        public async Task<CoreReturns> Insert(string? query)
        {
            return await ExecuteQuery(query, "Insert");
        }

        public async Task<CoreReturns> Delete(string query)
        {
            return await ExecuteQuery(query, "DeleteFactory");
        }

        public async Task<CoreReturns> Update(string query)
        {
            return await ExecuteQuery(query, "UpdateFactory");
        }

        public async Task<T?> Select<T>(string? query) where T : class, ISql<T>, new()
        {
            return await ExecuteSelectQuery<T>(query);
        }

        public static async Task<CoreReturns> TestConnection()
        {
            await using (NpgsqlConnection? con = ConnectionString)
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

        private static string CreateConnectionString()
        {
            ConfigurationsKeeper ConfigKeeper = new();
            return $@"User ID=postgres;Password=Popmart123!;Server={ConfigKeeper.Data["server"]};Port={ConfigKeeper.Data["dbport"]};Database={ConfigKeeper.Data["database"]};Include Error Detail=true;";
        }
    }
}
