using Microsoft.Data.Sqlite;
using System.Globalization;

/// <summary>
/// Управление базой данных SQLite.
/// Инкапсулирует все операции с БД: создание таблиц,
/// импорт CSV, CRUD-операции, выполнение запросов для отчётов.
/// </summary>
class DatabaseManager
{
    private string _connectionString;

    /// <summary>
    /// Конструктор. Принимает путь к файлу БД.
    /// </summary>
    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    // ---------- Инициализация ----------
    /// <summary>
    /// Создаёт таблицы (если не существуют) и загружает CSV при первом запуске
    /// </summary>
    public void InitializeDatabase(string facultyCsvPath, string studentCsvPath)
    {
        CreateTables();
        if (GetAllFaculties().Count == 0 && File.Exists(facultyCsvPath))
        {
            ImportFacultiesFromCsv(facultyCsvPath);
            Console.WriteLine($"[OK] Загружены факультеты из {facultyCsvPath}");
        }
        if (GetAllStudents().Count == 0 && File.Exists(studentCsvPath))
        {
            ImportStudentsFromCsv(studentCsvPath);
            Console.WriteLine($"[OK] Загружены студенты из {studentCsvPath}");
        }
    }

    /// <summary>Создание таблиц</summary>
    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS faculty (
                faculty_id INTEGER PRIMARY KEY AUTOINCREMENT,
                faculty_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS student (
                student_id INTEGER PRIMARY KEY AUTOINCREMENT,
                faculty_id INTEGER NOT NULL,
                student_name TEXT NOT NULL,
                gpa REAL NOT NULL,
                FOREIGN KEY (faculty_id) REFERENCES faculty(faculty_id)
            );";
        cmd.ExecuteNonQuery();
    }

    /// <summary>Импорт факультетов из CSV</summary>
    private void ImportFacultiesFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO faculty (faculty_id, faculty_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Импорт студентов из CSV</summary>
    private void ImportStudentsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO student (student_id, faculty_id, student_name, gpa)
                VALUES (@id, @facId, @name, @gpa)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@facId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            // ИСПРАВЛЕНИЕ: используем InvariantCulture для чисел с точкой
            cmd.Parameters.AddWithValue("@gpa", double.Parse(parts[3], CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
    }

    // ---------- Чтение данных ----------
    /// <summary>Получить все факультеты</summary>
    public List<Faculty> GetAllFaculties()
    {
        var result = new List<Faculty>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT faculty_id, faculty_name FROM faculty ORDER BY faculty_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Faculty(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }

    /// <summary>Получить всех студентов</summary>
    public List<Student> GetAllStudents()
    {
        var result = new List<Student>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT student_id, faculty_id, student_name, gpa FROM student ORDER BY student_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Student(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3)));
        }
        return result;
    }

    /// <summary>Получить студента по Id</summary>
    public Student GetStudentById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT student_id, faculty_id, student_name, gpa FROM student WHERE student_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Student(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3));
        }
        return null;
    }

    /// <summary>Получить студентов конкретного факультета (для фильтра)</summary>
    public List<Student> GetStudentsByFaculty(int facultyId)
    {
        var result = new List<Student>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT student_id, faculty_id, student_name, gpa FROM student WHERE faculty_id = @facId ORDER BY student_name";
        cmd.Parameters.AddWithValue("@facId", facultyId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Student(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3)));
        }
        return result;
    }

    // ---------- Изменение данных (CRUD для студентов) ----------
    /// <summary>Добавить студента (Id генерируется автоматически)</summary>
    public void AddStudent(Student student)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO student (faculty_id, student_name, gpa) VALUES (@facId, @name, @gpa)";
        cmd.Parameters.AddWithValue("@facId", student.FacultyId);
        cmd.Parameters.AddWithValue("@name", student.Name);
        cmd.Parameters.AddWithValue("@gpa", student.Gpa);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Обновить данные студента</summary>
    public void UpdateStudent(Student student)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE student SET faculty_id = @facId, student_name = @name, gpa = @gpa WHERE student_id = @id";
        cmd.Parameters.AddWithValue("@id", student.Id);
        cmd.Parameters.AddWithValue("@facId", student.FacultyId);
        cmd.Parameters.AddWithValue("@name", student.Name);
        cmd.Parameters.AddWithValue("@gpa", student.Gpa);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Удалить студента по Id</summary>
    public void DeleteStudent(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM student WHERE student_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ---------- Выполнение произвольного запроса (для ReportBuilder) ----------
    /// <summary>
    /// Выполняет SQL-запрос и возвращает имена столбцов и строки результата.
    /// Используется классом ReportBuilder.
    /// </summary>
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        // Имена столбцов
        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        // Строки данных
        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        return (columns, rows);
    }

    // ---------- Экспорт в CSV (Группа Б, опционально) ----------
    /// <summary>Экспорт обеих таблиц в CSV-файлы (с разделителем ; и точкой для дробной части)</summary>
    public void ExportToCsv(string facultyPath, string studentPath)
    {
        // Экспорт факультетов
        var facultyLines = new List<string> { "faculty_id;faculty_name" };
        foreach (var fac in GetAllFaculties())
            facultyLines.Add($"{fac.Id};{fac.Name}");
        File.WriteAllLines(facultyPath, facultyLines);

        // Экспорт студентов (используем InvariantCulture для Gpa)
        var studentLines = new List<string> { "student_id;faculty_id;student_name;gpa" };
        foreach (var s in GetAllStudents())
        {
            string gpaStr = s.Gpa.ToString(CultureInfo.InvariantCulture);
            studentLines.Add($"{s.Id};{s.FacultyId};{s.Name};{gpaStr}");
        }
        File.WriteAllLines(studentPath, studentLines);
    }
}