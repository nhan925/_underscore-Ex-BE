using Dapper;
using NpgsqlTypes;
using System.Data;
using System.Text.Json;

namespace student_management_api.Helpers;

public class JsonbTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = value is null ? (object)DBNull.Value : JsonSerializer.Serialize(value);
        // Ensure the correct PostgreSQL type
        if (parameter is Npgsql.NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }

    public override T? Parse(object value)
    {
        return value == null || value is DBNull
            ? default
            : JsonSerializer.Deserialize<T>(value.ToString()!);
    }
}
