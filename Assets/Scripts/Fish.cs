public struct Fish
{
    public readonly string Name;
    public readonly int Price;
    public readonly string Description;

    public Fish(string name, string desc, int price)
    {
        Name = name;
        Description = desc;
        Price = price;
    }
}