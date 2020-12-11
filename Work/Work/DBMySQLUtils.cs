using MySqlConnector;

namespace Work {
    class DBMySQLUtils {
        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password) {
            string connString = "Server=" + host + ";Port=" + port 
                 + ";UserId=" + username + ";Password=" + password + ";Database=" + database;
            
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }

    }
}
