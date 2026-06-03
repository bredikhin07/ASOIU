using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string dbPath = "university.db";
string facultyCsv = Path.Combine(AppContext.BaseDirectory, "faculty.csv");
string studentCsv = Path.Combine(AppContext.BaseDirectory, "student.csv");

var db = new DatabaseManager(dbPath);
db.InitializeDatabase(facultyCsv, studentCsv);
Console.WriteLine();

string choice;
do
{
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║        УПРАВЛЕНИЕ СТУДЕНТАМИ         ║");
    Console.WriteLine("╠══════════════════════════════════════╣");
    Console.WriteLine("║ 1 — Показать все факультеты          ║");
    Console.WriteLine("║ 2 — Показать всех студентов          ║");
    Console.WriteLine("║ 3 — Добавить студента                ║");
    Console.WriteLine("║ 4 — Редактировать студента           ║");
    Console.WriteLine("║ 5 — Удалить студента                 ║");
    Console.WriteLine("║ 6 — Отчёты                           ║");
    Console.WriteLine("║ 7 — Фильтр по факультету             ║");
    Console.WriteLine("║ 8 — Экспорт в CSV                    ║");
    Console.WriteLine("║ 0 — Выход                            ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");
    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowFaculties(db); break;
        case "2": ShowStudents(db); break;
        case "3": AddStudent(db); break;
        case "4": EditStudent(db); break;
        case "5": DeleteStudent(db); break;
        case "6": ReportsMenu(db); break;
        case "7": FilterByFaculty(db); break;
        case "8": ExportCsv(db); break;
        case "0": Console.WriteLine("До свидания!"); break;
        default: Console.WriteLine("Неверный пункт меню."); break;
    }
    Console.WriteLine();
} while (choice != "0");

// ------------------------------------------------------------------
// Реализация функций пунктов меню
// ------------------------------------------------------------------

static void ShowFaculties(DatabaseManager db)
{
    Console.WriteLine("---- Все факультеты ----");
    var faculties = db.GetAllFaculties();
    foreach (var f in faculties) Console.WriteLine($" {f}");
    Console.WriteLine($"Итого: {faculties.Count}");
}

static void ShowStudents(DatabaseManager db)
{
    Console.WriteLine("---- Все студенты ----");
    var students = db.GetAllStudents();
    foreach (var s in students) Console.WriteLine($" {s}");
    Console.WriteLine($"Итого: {students.Count}");
}

static void AddStudent(DatabaseManager db)
{
    Console.WriteLine("---- Добавление студента ----");
    Console.WriteLine("Доступные факультеты:");
    var faculties = db.GetAllFaculties();
    foreach (var f in faculties) Console.WriteLine($" {f}");

    Console.Write("ID факультета: ");
    if (!int.TryParse(Console.ReadLine(), out int facId))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    Console.Write("Имя студента: ");
    string name = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(name)) { Console.WriteLine("Ошибка: имя не может быть пустым."); return; }

    Console.Write("Средний балл (gpa): ");
    if (!double.TryParse(Console.ReadLine(), out double gpa))
    { Console.WriteLine("Ошибка: введите число (например, 4.5)."); return; }

    try
    {
        var student = new Student(0, facId, name, gpa);
        db.AddStudent(student);
        Console.WriteLine("Студент добавлен.");
    }
    catch (ArgumentException ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
}

static void EditStudent(DatabaseManager db)
{
    Console.WriteLine("---- Редактирование студента ----");
    Console.Write("Введите ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    var student = db.GetStudentById(id);
    if (student == null) { Console.WriteLine($"Студент с ID={id} не найден."); return; }

    Console.WriteLine($"Текущие данные: {student}");
    Console.WriteLine("(Нажмите Enter, чтобы оставить без изменений)");

    Console.Write($"Имя [{student.Name}]: ");
    string input = Console.ReadLine()?.Trim();
    if (!string.IsNullOrEmpty(input)) student.Name = input;

    Console.Write($"ID факультета [{student.FacultyId}]: ");
    input = Console.ReadLine()?.Trim();
    if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newFacId))
        student.FacultyId = newFacId;

    Console.Write($"Средний балл [{student.Gpa:F2}]: ");
    input = Console.ReadLine()?.Trim();
    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newGpa))
    {
        try { student.Gpa = newGpa; }
        catch (ArgumentException ex) { Console.WriteLine($"Ошибка: {ex.Message}"); return; }
    }

    db.UpdateStudent(student);
    Console.WriteLine("Данные обновлены.");
}

static void DeleteStudent(DatabaseManager db)
{
    Console.WriteLine("---- Удаление студента ----");
    Console.Write("Введите ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    var student = db.GetStudentById(id);
    if (student == null) { Console.WriteLine($"Студент с ID={id} не найден."); return; }

    Console.Write($"Удалить «{student.Name}»? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower();
    if (confirm == "да")
    {
        db.DeleteStudent(id);
        Console.WriteLine("Студент удалён.");
    }
    else Console.WriteLine("Удаление отменено.");
}

// ---------- ПОДМЕНЮ ОТЧЁТОВ ----------
static void ReportsMenu(DatabaseManager db)
{
    string subChoice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine(" 1 - Список студентов с факультетами (JOIN)");
        Console.WriteLine(" 2 - Количество студентов по факультетам");
        Console.WriteLine(" 3 - Средний балл по факультетам");
        Console.WriteLine(" 4 - Сохранить отчёт №3 в файл (группа Б)");
        Console.WriteLine(" 0 - Назад");
        Console.Write("Ваш выбор: ");
        subChoice = Console.ReadLine()?.Trim() ?? "";
        switch (subChoice)
        {
            case "1": Report1_StudentsWithFaculties(db); break;
            case "2": Report2_CountByFaculty(db); break;
            case "3": Report3_AvgGpaByFaculty(db); break;
            case "4": SaveReport3ToFile(db); break;
            case "0": break;
            default: Console.WriteLine("Неверный пункт."); break;
        }
        Console.WriteLine();
    } while (subChoice != "0");
}

static void Report1_StudentsWithFaculties(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT s.student_name, f.faculty_name, s.gpa
                 FROM student s
                 JOIN faculty f ON s.faculty_id = f.faculty_id
                 ORDER BY s.student_name")
        .Title("Студенты по факультетам")
        .Header("Студент", "Факультет", "Ср. балл")
        .ColumnWidths(25, 25, 10)
        .Print();
}

static void Report2_CountByFaculty(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT f.faculty_name, COUNT(*) AS cnt
                 FROM student s
                 JOIN faculty f ON s.faculty_id = f.faculty_id
                 GROUP BY f.faculty_name
                 ORDER BY f.faculty_name")
        .Title("Количество студентов по факультетам")
        .Header("Факультет", "Кол-во")
        .ColumnWidths(30, 10)
        .Print();
}

static void Report3_AvgGpaByFaculty(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT f.faculty_name, ROUND(AVG(s.gpa), 2) AS avg_gpa
                 FROM student s
                 JOIN faculty f ON s.faculty_id = f.faculty_id
                 GROUP BY f.faculty_name
                 ORDER BY avg_gpa DESC")
        .Title("Средний балл по факультетам")
        .Header("Факультет", "Средний балл")
        .ColumnWidths(30, 15)
        .Print();
}

// ---- ДОПОЛНИТЕЛЬНОЕ ЗАДАНИЕ ГРУППЫ Б: сохранение отчёта в файл ----
static void SaveReport3ToFile(DatabaseManager db)
{
    string fileName = $"report_avg_gpa_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
    new ReportBuilder(db)
        .Query(@"SELECT f.faculty_name, ROUND(AVG(s.gpa), 2) AS avg_gpa
                 FROM student s
                 JOIN faculty f ON s.faculty_id = f.faculty_id
                 GROUP BY f.faculty_name
                 ORDER BY avg_gpa DESC")
        .Title("Средний балл по факультетам")
        .Header("Факультет", "Средний балл")
        .ColumnWidths(30, 15)
        .SaveToFile(fileName);
}

// ---------- ФИЛЬТР ПО ФАКУЛЬТЕТУ ----------
static void FilterByFaculty(DatabaseManager db)
{
    Console.WriteLine("---- Фильтр по факультету ----");
    var faculties = db.GetAllFaculties();
    foreach (var f in faculties) Console.WriteLine($" {f}");
    Console.Write("Введите ID факультета: ");
    if (!int.TryParse(Console.ReadLine(), out int facId))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    var students = db.GetStudentsByFaculty(facId);
    if (students.Count == 0)
    { Console.WriteLine("На этом факультете нет студентов."); return; }

    Console.WriteLine($"\nСтуденты факультета #{facId}:");
    foreach (var s in students) Console.WriteLine($" {s}");
    Console.WriteLine($"Итого: {students.Count}");
}

// ---------- ЭКСПОРТ В CSV ----------
static void ExportCsv(DatabaseManager db)
{
    string facPath = Path.Combine(AppContext.BaseDirectory, "faculty_export.csv");
    string studPath = Path.Combine(AppContext.BaseDirectory, "student_export.csv");
    db.ExportToCsv(facPath, studPath);
    Console.WriteLine($"Факультеты экспортированы в: {facPath}");
    Console.WriteLine($"Студенты экспортированы в: {studPath}");
}