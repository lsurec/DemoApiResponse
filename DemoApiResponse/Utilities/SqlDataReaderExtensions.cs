using Microsoft.Data.SqlClient;

namespace DemoApiResponse.Utilities
{
    public static class SqlDataReaderExtensions
    {

        public static T? GetValueOrDefault<T>(this SqlDataReader reader, string columnName)
        {
            // Verifica si la columna existe en el SqlDataReader
            if (reader.HasColumn(columnName))
            {
                object value = reader[columnName];
                return value != DBNull.Value ? (T)value : default;
            }

            // Si la columna no existe, devuelve null o el valor por defecto
            return default;
        }

        // Método auxiliar para verificar si la columna existe en el SqlDataReader
        private static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }


        public static T? GetValueOrDefaulInt<T>(this SqlDataReader reader, int indexColumn)
        {
            // Verifica si el índice de la columna está dentro del rango de columnas
            if (indexColumn >= 0 && indexColumn < reader.FieldCount)
            {
                object value = reader[indexColumn];
                return value != DBNull.Value ? (T)value : default;
            }

            // Si el índice de la columna es inválido, devuelve null o el valor por defecto
            return default;
        }

    }
}
