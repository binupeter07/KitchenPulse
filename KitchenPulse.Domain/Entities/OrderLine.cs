namespace KitchenPulse.Domain.Entities;

public class OrderLine
{
    private readonly List<Ingredient> _removed = new();
    private readonly List<Ingredient> _extras = new();
    public Dish Dish { get; private set; }
    public int Quantity { get; private set; }

    public IReadOnlyCollection<Ingredient> Removed => _removed;
    public IReadOnlyCollection<Ingredient> Extras => _extras;

    public OrderLine(Dish dish, int quantity)
    {

        ArgumentNullException.ThrowIfNull(dish);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        Dish = dish;
        Quantity = quantity;
    }

    public void RemoveIngredient(Ingredient ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        bool isInDish = Dish.RecipeLines.Any(rl => rl.Ingredient.Id == ingredient.Id);

        if (!isInDish)
        {
            throw new ArgumentException($"There is no {ingredient.Name} in  {Dish.Name}", nameof(ingredient));
        }

        if (_extras.Contains(ingredient))
        {
            throw new InvalidOperationException($"{ingredient.Name} can't be added as extra and removed  at the same time");
        }

        if (!_removed.Contains(ingredient))
        {
            _removed.Add(ingredient);
        }

    }

    public void AddExtra(Ingredient ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (_removed.Contains(ingredient))
        {
            throw new InvalidOperationException($"{ingredient.Name} can't be removed and added as an extra at the same time"); ;
        }

        if (!_extras.Contains(ingredient))
        {
            _extras.Add(ingredient);
        }
    }
}
