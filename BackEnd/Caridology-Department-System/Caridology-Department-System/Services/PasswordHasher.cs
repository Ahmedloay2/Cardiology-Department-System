using BCrypt.Net;
using Caridology_Department_System;
public class PasswordHasher
{
    private const int WorkFactor = 12; // Good balance between security and performance

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}