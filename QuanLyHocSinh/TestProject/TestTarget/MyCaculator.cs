using System;

namespace TestTarget
{
    public class MyCalculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Subtract(int a, int b)
        {
            return a - b;
        }

        public int Multiply(int a, int b)
        {
            return a * b;
        }

        public double Divide(double a, double b)
        {
            if (b == 0)
            {
                throw new ArgumentException("Cannot divide by zero.");
            }
            return a / b;
        }
    }

    public class AuthenticationService
    {
        public bool Login(string username, string password)
        {
            // Đây là logic đăng nhập giả định
            return username == "admin" && password == "password";
        }
    }
}