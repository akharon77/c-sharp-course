using System;

class Calculator
{
    static void Main(string[] args)
    {
        Console.WriteLine("Format: <number> <operator> <number>");
        Console.WriteLine("Type 'exit' to quit");

        while (true)
        {
            Console.Write("> ");
            string command = Console.ReadLine() ?? "";

            if (string.Equals(command.ToLower(), "exit"))
            {
                break;
            }

            double result = 0;
            bool success =
                TryParseCommand(command, out double num1, out char op, out double num2) &&
                TryCalculate(num1, op, num2, out result);

            if (success)
                PrintResult(result);
            else
                PrintError("Invalid format. Use: <number> <operator> <number>");
        }
    }

    static bool TryParseCommand(string command, out double num1, out char op, out double num2)
    {
        num1 = 0;
        num2 = 0;
        op = ' ';

        string[] parts = command.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
        {
            return false;
        }

        if (!double.TryParse(parts[0], out num1))
        {
            return false;
        }

        if (parts[1].Length != 1)
        {
            return false;
        }

        op = parts[1][0];

        if (!double.TryParse(parts[2], out num2))
        {
            return false;
        }

        return true;
    }

    static bool TryCalculate(double num1, char op, double num2, out double result)
    {
        result = 0;

        switch (op)
        {
            case '+':
                result = Add(num1, num2);
                return true;
            case '-':
                result = Subtract(num1, num2);
                return true;
            case '*':
                result = Multiply(num1, num2);
                return true;
            case '/':
                return Divide(num1, num2, out result);
            default:
                return false;
        }
    }

    static double Add(double num1, double num2)
    {
        return num1 + num2;
    }

    static double Subtract(double num1, double num2)
    {
        return num1 - num2;
    }

    static double Multiply(double num1, double num2)
    {
        return num1 * num2;
    }

    static bool Divide(double num1, double num2, out double result)
    {
        result = 0;

        if (num2 == 0)
        {
            PrintError("Cannot divide by zero");
            return false;
        }

        result = num1 / num2;
        return true;
    }

    static void PrintResult(double result)
    {
        Console.WriteLine("< " + result);
    }

    static void PrintError(string message)
    {
        Console.WriteLine("< Error: " + message);
    }
}
