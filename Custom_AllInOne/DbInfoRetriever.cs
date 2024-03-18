using Llama;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_AllInOne
{
    public static class DbInfoRetriever
    {
        static object dbLocker = new object();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="plantGroupId"></param>
        /// <returns>plant path e.g. \G0\00 - ...</returns>
        public static string GetPlantPath(LMADataSource dataSource, string plantGroupId)
        {
            string forReturn = "";
            var tuple = GetConnectionString(dataSource);
            string connectionString = tuple.Item2;
            string projName = tuple.Item1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.CommandText = "SELECT Dir_Path FROM " + projName + ".T_PlantGroup WHERE SP_ID = " + "\'" + plantGroupId + "\'";
                    command.Connection = connection;
                    forReturn = (string)command.ExecuteScalar();


                }
                connection.Close();
            }

            return forReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="plantShortPath">like \G0\00</param>
        /// <param name="projName">ULGPP or ULLNG</param>
        /// <returns>next TagSeqNo (string)</returns>
        public static string GetNextTagSeqNo(LMADataSource dataSource, string plantShortPath, string projName, string valveId)
        {
            string forReturn = "";
            var tuple = GetConnectionString(dataSource);
            string connectionString = tuple.Item2;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                /*object res; 
                using (SqlCommand command = new SqlCommand())
                {
                    command.CommandText = "SELECT valve_id from " + projName + ".NumberedValves" + " WHERE valve_id = '" + valveId + "\'";
                    command.Connection = connection;
                    res  = command.ExecuteScalar();
                }*/
                /*if (res == null)
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandText = "INSERT INTO " + projName + ".NumberedValves " + "(valve_id) VALUES ('" + valveId + "')";
                        command.Connection = connection;
                        command.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandText = "DECLARE @IncrementValue int SET @IncrementValue = 1 UPDATE " + projName + ".CustomCounters" + " SET valves_counter = valves_counter + @IncrementValue OUTPUT INSERTED.valves_counter WHERE unit_code = " + "\'" + plantShortPath + "\'";
                        command.Connection = connection;
                        forReturn = ((int)command.ExecuteScalar()).ToString();
                    }
                }*/
                using (SqlCommand command = new SqlCommand())
                {
                    command.CommandText = "DECLARE @IncrementValue int SET @IncrementValue = 1 UPDATE " + projName + ".CustomCounters" + " SET valves_counter = valves_counter + @IncrementValue OUTPUT INSERTED.valves_counter WHERE unit_code = " + "\'" + plantShortPath + "\'";
                    command.Connection = connection;
                    forReturn = ((int)command.ExecuteScalar()).ToString();
                }
                connection.Close();
            }
            if (forReturn == null || forReturn == "") return "";
            else return forReturn;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="plantGroupId"></param>
        /// <returns>projName,connectionString</returns>
        public static Tuple<string,string> GetConnectionString(LMADataSource dataSource)
        {
            string connectionString = "";
            string projName = dataSource.GetActiveProject().Attributes["Name"].get_Value().ToString();
            if (Environment.UserName == "Admin_SP")
            {
                if (projName == "ULGPP")
                {
                    connectionString = "Data Source=WIN-TEST-2023\\MEGAPID;Initial Catalog=ULGPP;Integrated Security=SSPI;";
                }

                else
                {
                    connectionString = "Data Source=WIN-TEST-2023\\MEGAPID;Initial Catalog=ULLNG;Integrated Security=SSPI;";
                }
            }

            else
            {
                if (projName == "ULGPP")
                {
                    connectionString = "Data Source=SQL-DATA1;Initial Catalog=ULGPP;User ID=admin;Password=admin;";
                }

                else
                {
                    connectionString = "Data Source=SQL-DATA1;Initial Catalog=ULLNG;User ID=admin;Password=admin;";
                }
            }

            return new Tuple<string, string>(projName, connectionString);
        }
    }
}
