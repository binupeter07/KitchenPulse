using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class Order
{

    private readonly List<OrderLine> _lines = new();
    private readonly List<Course> _collectedCourses = new();

    public Guid Id { get; private set; }
    public int OrderNumber { get; private set; }
    public string Table { get; private set; }
    public DateTime PlacedAtUtc { get; private set; }
    public bool IsCollected { get; private set; }


    public IReadOnlyCollection<OrderLine> Lines => _lines;
    public IReadOnlyCollection<Course> CollectedCourse => _collectedCourses;
    public bool IsMainsOnHold => _lines.Any(l => l.Course == Course.Starter) && !IsCourseCollected(Course.Starter);
    public Order(int orderNumber, string table, DateTime placedAtUtc)
    {

        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentException("Table cannot be null or blank.", nameof(table));
        }
        if (orderNumber <= 0)
        {
            throw new ArgumentException("Order number must be greater than zero.", nameof(orderNumber));
        }

        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        Table = table.Trim();
        PlacedAtUtc = placedAtUtc;
    }


    public OrderLine AddDish(Dish dish, int quantity, IEnumerable<Ingredient>? removeIngredients = null, Course course = Course.Main)
    {
        var line = new OrderLine(dish, quantity, course);

        foreach (var ingredient in removeIngredients ?? [])
        {
            line.RemoveIngredient(ingredient);
        }

        _lines.Add(line);

        return line;
    }


    public bool IsCourseReady(Course course)
    {
        Guard.EnumDefined(course, nameof(course));

        var kitchenLines = _lines.Where(l => l.Course == course)
                          .Where(l => l.Dish.Station != Station.Front);
        return kitchenLines.Any() && kitchenLines.All(l => l.IsDone);
    }

    public void CollectCourse(Course course)
    {
        Guard.EnumDefined(course, nameof(course));

        if (IsCourseReady(course) )
        {
            if (!_collectedCourses.Contains(course))
            {
                _collectedCourses.Add(course);
            }
        }
        else
        {
            throw new InvalidOperationException("Course is not ready");
        }
    }

    public bool IsCourseCollected(Course course)
    {
        Guard.EnumDefined(course,nameof(course));

        return _collectedCourses.Contains(course);
    }
}
