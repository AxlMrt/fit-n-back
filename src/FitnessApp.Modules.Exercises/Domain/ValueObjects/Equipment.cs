namespace FitnessApp.Modules.Exercises.Domain.ValueObjects
{
    public class Equipment
    {
        public List<string> Items { get; private set; } = new();

        public Equipment() { }

        public Equipment(IEnumerable<string> items)
        {
            Items = items?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList() ?? new List<string>();
        }

        public override string ToString()
        {
            return string.Join(',', Items);
        }

        public static Equipment FromString(string? data)
        {
            if (string.IsNullOrWhiteSpace(data)) return new Equipment();
            var parts = data.Split(',');
            return new Equipment(parts);
        }
    }
}
