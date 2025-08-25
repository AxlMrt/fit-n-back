namespace FitnessApp.Modules.Exercises.Domain.ValueObjects
{
    public sealed class Equipment : IEquatable<Equipment>
    {
        private readonly List<string> _items;

        public IReadOnlyList<string> Items => _items.AsReadOnly();

        public Equipment()
        {
            _items = new List<string>();
        }

        public Equipment(IEnumerable<string>? items)
        {
            if (items == null)
            {
                _items = new List<string>();
                return;
            }

            _items = items
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(x => x.Length <= 50) // Max length per equipment
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            if (_items.Count > 10) // Max 10 equipment items
                throw new ArgumentException("Cannot have more than 10 equipment items");
        }

        public static Equipment None => new();

        public static Equipment FromString(string? data)
        {
            if (string.IsNullOrWhiteSpace(data)) 
                return new Equipment();
            
            var parts = data.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return new Equipment(parts);
        }

        public override string ToString()
        {
            return string.Join(',', _items);
        }

        public bool HasEquipment(string equipmentName)
        {
            return _items.Any(item => string.Equals(item, equipmentName, StringComparison.OrdinalIgnoreCase));
        }

        public bool RequiresAnyEquipment() => _items.Any();

        public Equipment Add(string equipmentName)
        {
            if (string.IsNullOrWhiteSpace(equipmentName))
                throw new ArgumentException("Equipment name cannot be empty");

            var newItems = _items.ToList();
            var trimmed = equipmentName.Trim();
            
            if (!newItems.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                newItems.Add(trimmed);
            }

            return new Equipment(newItems);
        }

        public Equipment Remove(string equipmentName)
        {
            var newItems = _items.Where(item => 
                !string.Equals(item, equipmentName, StringComparison.OrdinalIgnoreCase)).ToList();
            
            return new Equipment(newItems);
        }

        // Equality implementation
        public bool Equals(Equipment? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _items.SequenceEqual(other._items, StringComparer.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => obj is Equipment other && Equals(other);

        public override int GetHashCode()
        {
            return _items.Aggregate(0, (current, item) => 
                HashCode.Combine(current, item.ToUpperInvariant().GetHashCode()));
        }

        public static bool operator ==(Equipment left, Equipment right) => Equals(left, right);
        public static bool operator !=(Equipment left, Equipment right) => !Equals(left, right);
    }
}
