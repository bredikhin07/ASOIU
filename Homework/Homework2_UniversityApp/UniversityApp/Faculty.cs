/// <summary>
/// Факультет (справочная таблица, сторона «один»)
/// </summary>
class Faculty
{
    public int Id { get; set; }
    public string Name { get; set; }

    /// <summary>Конструктор с параметрами</summary>
    public Faculty(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Faculty() : this(0, "") { }

    public override string ToString() => $"[{Id}] {Name}";
}