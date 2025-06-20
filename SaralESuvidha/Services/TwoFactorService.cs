using System.Data.SqlClient;
using System.Data;
using System;

namespace SaralESuvidha.Services;

public class TwoFactorService
{
    private readonly string _connectionString;

    public TwoFactorService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void SaveSecret(string userId, string secret)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SaveTwoFactorSecret", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@SecretKey", secret);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void EnableTwoFactor(string userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("EnableTwoFactor", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@UserId", userId);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public (string SecretKey, bool IsEnabled) GetSecret(string userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("GetTwoFactorData", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@UserId", userId);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return (
                reader.GetString(reader.GetOrdinal("SecretKey")),
                reader.GetBoolean(reader.GetOrdinal("IsEnabled"))
            );
        }
        throw new Exception("2FA config not found.");
    }
}

