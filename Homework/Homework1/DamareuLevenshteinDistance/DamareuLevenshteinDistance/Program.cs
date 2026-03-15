using System;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8; 

Console.WriteLine("=== ВЫЧИСЛЕНИЕ РАССТОЯНИЯ ДАМЕРАУ-ЛЕВЕНШТЕЙНА ===");
Console.WriteLine("Введите две строки для сравнения.");
Console.WriteLine("Для выхода введите 'exit' в качестве первой строки.");
Console.WriteLine();

while (true)
{
    Console.Write("Строка 1: ");
    string str1 = Console.ReadLine();

    if (str1?.ToLower() == "exit")
    {
        Console.WriteLine("Выход из программы...");
        break;
    }

    Console.Write("Строка 2: ");
    string str2 = Console.ReadLine();

    int distance = DamareuLevenshteinDistance(str1, str2);

    Console.WriteLine($"Расстояние Дамерау-Левенштейна между '{str1}' и '{str2}': {distance}");
    Console.WriteLine(new string('-', 50));
    Console.WriteLine();
}

static int DamareuLevenshteinDistance(string str1Param, string str2Param)
{
    // Проверка на null
    if (str1Param == null || str2Param == null)
        return -1;

    int n = str1Param.Length;
    int m = str2Param.Length;

    // Если строки пустые
    if (n == 0 && m == 0) return 0;
    if (n == 0) return m;
    if (m == 0) return n;

    // Приводим к верхнему регистру для регистронезависимости
    string str1 = str1Param.ToUpper();
    string str2 = str2Param.ToUpper();

    // Создаем матрицу
    int[,] matrix = new int[n + 1, m + 1];

    // Инициализация
    for (int i = 0; i <= n; i++)
        matrix[i, 0] = i;
    for (int j = 0; j <= m; j++)
        matrix[0, j] = j;

    // Заполнение матрицы
    for (int i = 1; i <= n; i++)
    {
        for (int j = 1; j <= m; j++)
        {
            // Проверка совпадения символов
            int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;

            // Минимум из вставки, удаления и замены
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1,      // удаление
                        matrix[i, j - 1] + 1),      // вставка
                matrix[i - 1, j - 1] + cost);       // замена

            // Проверка на транспозицию (перестановка соседних символов)
            if (i > 1 && j > 1 &&
                str1[i - 1] == str2[j - 2] &&
                str1[i - 2] == str2[j - 1])
            {
                matrix[i, j] = Math.Min(matrix[i, j],
                                       matrix[i - 2, j - 2] + cost);
            }
        }
    }

    

    return matrix[n, m];
}