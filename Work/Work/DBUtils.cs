using MySqlConnector;

namespace Work {
    class DBUtils {
        public static MySqlConnection GetDBConnection() {
            string host = "10.0.2.2";
            int port = 3306;
            string database = "movie_list";
            string username = "root";
            string password = "rekbr1488";
            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }
    }
}
