namespace FitnessApp.SharedKernel.Enums;

[Flags]
public enum Equipment
{
    None = 0,
    
    // Free weights
    Dumbbells = 1 << 0,      // 1
    Barbells = 1 << 1,       // 2
    Kettlebells = 1 << 2,    // 4
    
    // Bodyweight support
    PullUpBar = 1 << 3,      // 8
    DipBars = 1 << 4,        // 16
    
    // Gym equipment
    Bench = 1 << 5,          // 32
    InclineBench = 1 << 6,   // 64
    Cable = 1 << 7,          // 128
    Machine = 1 << 8,        // 256
    
    // Cardio
    Treadmill = 1 << 9,      // 512
    Bike = 1 << 10,          // 1024
    Rower = 1 << 11,         // 2048
    
    // Accessories
    Mat = 1 << 12,           // 4096
    ResistanceBands = 1 << 13, // 8192
    MedicineBall = 1 << 14,   // 16384
    FoamRoller = 1 << 15,     // 32768
    
    // Combined common sets
    FreeWeights = Dumbbells | Barbells | Kettlebells,
    GymEquipment = Bench | InclineBench | Cable | Machine,
    CardioEquipment = Treadmill | Bike | Rower
}
