/// <summary>
/// Студент (основная таблица, сторона «много»)
/// </summary>
class Student
{
    public int Id { get; set; }
    public int FacultyId { get; set; }
    public string Name { get; set; }

    private double _gpa;

    /// <summary>
    /// Средний балл (не может быть отрицательным)
    /// </summary>
    public double Gpa
    {
        get => _gpa;
        set
        {
            if (value < 0)
                throw new ArgumentException("Средний балл не может быть отрицательным");
            _gpa = value;
        }
    }

    /// <summary>Конструктор с параметрами</summary>
    public Student(int id, int facultyId, string name, double gpa)
    {
        Id = id;
        FacultyId = facultyId;
        Name = name;
        Gpa = gpa;          // валидация сработает здесь
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Student() : this(0, 0, "", 0.0) { }

    public override string ToString() => $"[{Id}] {Name}, факультет #{FacultyId}, балл: {Gpa:F2}";
}