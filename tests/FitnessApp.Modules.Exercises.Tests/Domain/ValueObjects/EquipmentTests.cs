using FitnessApp.Modules.Exercises.Domain.ValueObjects;

namespace FitnessApp.Modules.Exercises.Tests.Domain.ValueObjects;
public class EquipmentTests
{
    [Fact]
    public void Equipment_DefaultConstructor_ShouldCreateEmptyEquipment()
    {
        // Act
        var equipment = new Equipment();

        // Assert
        Assert.Empty(equipment.Items);
        Assert.False(equipment.RequiresAnyEquipment());
    }

    [Fact]
    public void Equipment_Constructor_ShouldCreateEquipmentWithItems()
    {
        // Arrange
        var items = new[] { "Dumbbells", "Bench", "Barbell" };

        // Act
        var equipment = new Equipment(items);

        // Assert
        Assert.Equal(3, equipment.Items.Count);
        Assert.Contains("Dumbbells", equipment.Items);
        Assert.Contains("Bench", equipment.Items);
        Assert.Contains("Barbell", equipment.Items);
        Assert.True(equipment.RequiresAnyEquipment());
    }

    [Fact]
    public void Equipment_Constructor_ShouldFilterOutNullOrWhitespaceItems()
    {
        // Arrange
        var items = new[] { "Dumbbells", "", "  ", "Bench" }.Where(s => s != null);

        // Act
        var equipment = new Equipment(items);

        // Assert
        Assert.Equal(2, equipment.Items.Count);
        Assert.Contains("Dumbbells", equipment.Items);
        Assert.Contains("Bench", equipment.Items);
    }

    [Fact]
    public void Equipment_Constructor_ShouldRemoveDuplicates()
    {
        // Arrange
        var items = new[] { "Dumbbells", "dumbbells", "DUMBBELLS", "Bench" };

        // Act
        var equipment = new Equipment(items);

        // Assert
        Assert.Equal(2, equipment.Items.Count);
        Assert.Contains("Dumbbells", equipment.Items);
        Assert.Contains("Bench", equipment.Items);
    }

    [Fact]
    public void Equipment_Constructor_ShouldTrimItems()
    {
        // Arrange
        var items = new[] { "  Dumbbells  ", " Bench " };

        // Act
        var equipment = new Equipment(items);

        // Assert
        Assert.Equal(2, equipment.Items.Count);
        Assert.Contains("Dumbbells", equipment.Items);
        Assert.Contains("Bench", equipment.Items);
    }

    [Fact]
    public void Equipment_Constructor_ShouldSortItemsAlphabetically()
    {
        // Arrange
        var items = new[] { "Zebra", "Apple", "Banana" };

        // Act
        var equipment = new Equipment(items);

        // Assert
        Assert.Equal("Apple", equipment.Items[0]);
        Assert.Equal("Banana", equipment.Items[1]);
        Assert.Equal("Zebra", equipment.Items[2]);
    }

    [Fact]
    public void Equipment_Constructor_ShouldFilterOutTooLongItems()
    {
        // Arrange
        var longItem = new string('a', 51); // More than 50 characters
        var items = new[] { "Dumbbells", longItem, "Bench" };

        // Act
        var equipment = new Equipment(items);

        // Assert
        Assert.Equal(2, equipment.Items.Count);
        Assert.Contains("Dumbbells", equipment.Items);
        Assert.Contains("Bench", equipment.Items);
        Assert.DoesNotContain(longItem, equipment.Items);
    }

    [Fact]
    public void Equipment_Constructor_ShouldThrowException_WhenTooManyItems()
    {
        // Arrange
        var items = Enumerable.Range(1, 11).Select(i => $"Item{i}").ToArray(); // 11 items

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Equipment(items));
    }

    [Fact]
    public void Equipment_Constructor_WithNullItems_ShouldCreateEmpty()
    {
        // Act
        var equipment = new Equipment(null);

        // Assert
        Assert.Empty(equipment.Items);
    }

    [Fact]
    public void Equipment_FromString_ShouldParseCommaSeparatedString()
    {
        // Arrange
        var equipmentString = "Dumbbells,Bench,Barbell";

        // Act
        var equipment = Equipment.FromString(equipmentString);

        // Assert
        Assert.Equal(3, equipment.Items.Count);
        Assert.Contains("Dumbbells", equipment.Items);
        Assert.Contains("Bench", equipment.Items);
        Assert.Contains("Barbell", equipment.Items);
    }

    [Fact]
    public void Equipment_FromString_WithEmptyString_ShouldCreateEmpty()
    {
        // Act
        var equipment = Equipment.FromString("");

        // Assert
        Assert.Empty(equipment.Items);
    }

    [Fact]
    public void Equipment_FromString_WithNull_ShouldCreateEmpty()
    {
        // Act
        var equipment = Equipment.FromString(null);

        // Assert
        Assert.Empty(equipment.Items);
    }

[Fact]
public void Equipment_ToString_ShouldReturnCommaSeparatedString()
{
    // Arrange
    var items = new[] { "Dumbbells", "Bench" };
    var equipment = new Equipment(items);

    // Act
    var result = equipment.ToString();

    // Assert (items are sorted alphabetically)
    Assert.Equal("Bench,Dumbbells", result);
}        [Fact]
    public void Equipment_HasEquipment_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells", "Bench" });

        // Act & Assert
        Assert.True(equipment.HasEquipment("Dumbbells"));
        Assert.True(equipment.HasEquipment("dumbbells")); // Case insensitive
        Assert.True(equipment.HasEquipment("DUMBBELLS"));
    }

    [Fact]
    public void Equipment_HasEquipment_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells", "Bench" });

        // Act & Assert
        Assert.False(equipment.HasEquipment("Barbell"));
    }

    [Fact]
    public void Equipment_Add_ShouldAddNewItem()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells" });

        // Act
        var newEquipment = equipment.Add("Bench");

        // Assert
        Assert.Equal(2, newEquipment.Items.Count);
        Assert.Contains("Dumbbells", newEquipment.Items);
        Assert.Contains("Bench", newEquipment.Items);
        // Original should be unchanged
        Assert.Single(equipment.Items);
    }

    [Fact]
    public void Equipment_Add_ShouldNotAddDuplicate()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells", "Bench" });

        // Act
        var newEquipment = equipment.Add("dumbbells"); // Case insensitive

        // Assert
        Assert.Equal(2, newEquipment.Items.Count);
        Assert.Contains("Dumbbells", newEquipment.Items);
        Assert.Contains("Bench", newEquipment.Items);
    }

    [Fact]
    public void Equipment_Add_ShouldThrowException_WhenItemIsEmpty()
    {
        // Arrange
        var equipment = new Equipment();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => equipment.Add(""));
        Assert.Throws<ArgumentException>(() => equipment.Add("   "));
        Assert.Throws<ArgumentException>(() => equipment.Add("   "));
    }

    [Fact]
    public void Equipment_Remove_ShouldRemoveItem()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells", "Bench", "Barbell" });

        // Act
        var newEquipment = equipment.Remove("Bench");

        // Assert
        Assert.Equal(2, newEquipment.Items.Count);
        Assert.Contains("Dumbbells", newEquipment.Items);
        Assert.Contains("Barbell", newEquipment.Items);
        Assert.DoesNotContain("Bench", newEquipment.Items);
        // Original should be unchanged
        Assert.Equal(3, equipment.Items.Count);
    }

    [Fact]
    public void Equipment_Remove_ShouldBeCaseInsensitive()
    {
        // Arrange
        var equipment = new Equipment(new[] { "Dumbbells", "Bench" });

        // Act
        var newEquipment = equipment.Remove("BENCH");

        // Assert
        Assert.Single(newEquipment.Items);
        Assert.Contains("Dumbbells", newEquipment.Items);
        Assert.DoesNotContain("Bench", newEquipment.Items);
    }

    [Fact]
    public void Equipment_Equals_ShouldReturnTrue_WhenSameItems()
    {
        // Arrange
        var equipment1 = new Equipment(new[] { "Dumbbells", "Bench" });
        var equipment2 = new Equipment(new[] { "Bench", "Dumbbells" }); // Different order

        // Act & Assert
        Assert.True(equipment1.Equals(equipment2));
        Assert.True(equipment1 == equipment2);
        Assert.False(equipment1 != equipment2);
    }

    [Fact]
    public void Equipment_Equals_ShouldReturnFalse_WhenDifferentItems()
    {
        // Arrange
        var equipment1 = new Equipment(new[] { "Dumbbells", "Bench" });
        var equipment2 = new Equipment(new[] { "Dumbbells", "Barbell" });

        // Act & Assert
        Assert.False(equipment1.Equals(equipment2));
        Assert.False(equipment1 == equipment2);
        Assert.True(equipment1 != equipment2);
    }

    [Fact]
    public void Equipment_GetHashCode_ShouldBeSame_ForEqualObjects()
    {
        // Arrange
        var equipment1 = new Equipment(new[] { "Dumbbells", "Bench" });
        var equipment2 = new Equipment(new[] { "Bench", "Dumbbells" });

        // Act & Assert
        Assert.Equal(equipment1.GetHashCode(), equipment2.GetHashCode());
    }

    [Fact]
    public void Equipment_None_ShouldReturnEmptyEquipment()
    {
        // Act
        var equipment = Equipment.None;

        // Assert
        Assert.Empty(equipment.Items);
        Assert.False(equipment.RequiresAnyEquipment());
    }
}

