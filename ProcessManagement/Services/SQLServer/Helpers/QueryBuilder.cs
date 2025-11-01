using Microsoft.Data.SqlClient;
using System.Text;

namespace ProcessManagement.Services.SQLServer.Helpers
{
    /// <summary>
    /// Helper để build parameterized queries an toàn
    /// Tránh SQL Injection và tăng performance
    /// </summary>
    public static class QueryBuilder
    {
        /// <summary>
        /// Tạo SELECT query với WHERE conditions an toàn
        /// </summary>
        public static SqlCommand BuildSelectQuery(
            SqlConnection connection,
            string tableName,
            Dictionary<string, object?>? parameters = null,
            List<string>? selectColumns = null)
        {
            var command = connection.CreateCommand();

            // Build SELECT clause
            string selectClause = selectColumns != null && selectColumns.Any()
                ? string.Join(", ", selectColumns.Select(col => $"[{col}]"))
                : "*";

            var query = new StringBuilder($"SELECT {selectClause} FROM [{tableName}]");

            // Build WHERE clause nếu có parameters
            if (parameters != null && parameters.Any())
            {
                var conditions = new List<string>();
                foreach (var param in parameters)
                {
                    string paramName = $"@{SanitizeParameterName(param.Key)}";
                    conditions.Add($"[{param.Key}] = {paramName}");
                    command.Parameters.AddWithValue(paramName, param.Value ?? DBNull.Value);
                }

                if (conditions.Any())
                {
                    query.Append(" WHERE ").Append(string.Join(" AND ", conditions));
                }
            }

            command.CommandText = query.ToString();
            return command;
        }

        /// <summary>
        /// Tạo INSERT query với OUTPUT clause
        /// </summary>
        public static SqlCommand BuildInsertQuery(
            SqlConnection connection,
            string tableName,
            Dictionary<string, object?> columns,
            string? outputColumn = null)
        {
            var command = connection.CreateCommand();

            var columnNames = new List<string>();
            var parameterNames = new List<string>();

            foreach (var column in columns)
            {
                string paramName = $"@{SanitizeParameterName(column.Key)}";
                columnNames.Add($"[{column.Key}]");
                parameterNames.Add(paramName);
                command.Parameters.AddWithValue(paramName, column.Value ?? DBNull.Value);
            }

            var query = new StringBuilder($"INSERT INTO [{tableName}] ({string.Join(", ", columnNames)})");

            if (!string.IsNullOrEmpty(outputColumn))
            {
                query.Append($" OUTPUT INSERTED.[{outputColumn}]");
            }

            query.Append($" VALUES ({string.Join(", ", parameterNames)})");

            command.CommandText = query.ToString();
            return command;
        }

        /// <summary>
        /// Tạo UPDATE query
        /// </summary>
        public static SqlCommand BuildUpdateQuery(
            SqlConnection connection,
            string tableName,
            Dictionary<string, object?> setColumns,
            Dictionary<string, object?> whereConditions)
        {
            var command = connection.CreateCommand();

            var setClauses = new List<string>();
            foreach (var column in setColumns)
            {
                string paramName = $"@set_{SanitizeParameterName(column.Key)}";
                setClauses.Add($"[{column.Key}] = {paramName}");
                command.Parameters.AddWithValue(paramName, column.Value ?? DBNull.Value);
            }

            var whereClauses = new List<string>();
            foreach (var condition in whereConditions)
            {
                string paramName = $"@where_{SanitizeParameterName(condition.Key)}";
                whereClauses.Add($"[{condition.Key}] = {paramName}");
                command.Parameters.AddWithValue(paramName, condition.Value ?? DBNull.Value);
            }

            var query = new StringBuilder($"UPDATE [{tableName}] SET {string.Join(", ", setClauses)}");
            
            if (whereClauses.Any())
            {
                query.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
            }

            command.CommandText = query.ToString();
            return command;
        }

        /// <summary>
        /// Tạo IN clause cho batch queries (tránh N+1)
        /// 
        /// Tính linh động:
        /// - selectColumns: Dynamic list, không hardcode số lượng columns
        /// - Hoạt động với bất kỳ số lượng columns nào (1, 2, 10, ...)
        /// - Nếu selectColumns = null → SELECT * (đầy đủ columns)
        /// </summary>
        public static SqlCommand BuildInQuery(
            SqlConnection connection,
            string tableName,
            string columnName,
            List<object> values,
            List<string>? selectColumns = null)
        {
            var command = connection.CreateCommand();

            // Dynamic: Build select clause từ list (không hardcode số lượng)
            string selectClause = selectColumns != null && selectColumns.Any()
                ? string.Join(", ", selectColumns.Select(col => $"[{col}]"))
                : "*";

            if (!values.Any())
            {
                command.CommandText = $"SELECT {selectClause} FROM [{tableName}] WHERE 1=0"; // Return empty
                return command;
            }

            // Dynamic: Build parameters từ values list (không hardcode số lượng)
            var parameters = new List<string>();
            for (int i = 0; i < values.Count; i++)
            {
                string paramName = $"@param{i}";
                parameters.Add(paramName);
                command.Parameters.AddWithValue(paramName, values[i] ?? DBNull.Value);
            }

            command.CommandText = $"SELECT {selectClause} FROM [{tableName}] WHERE [{columnName}] IN ({string.Join(", ", parameters)})";

            return command;
        }

        /// <summary>
        /// Sanitize parameter name để tránh SQL injection
        /// </summary>
        private static string SanitizeParameterName(string name)
        {
            // Remove special characters, chỉ giữ alphanumeric và underscore
            return System.Text.RegularExpressions.Regex.Replace(name, @"[^\w]", "");
        }
    }
}

