using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Trinket.Models;

public enum CharmShape
{
    Moon, Star, Heart, Sunflower, EvilEye, Cherries,
    Cloud, Saturn, Mushroom, Cat, Rainbow, DiscoBall, Paw,
    Bow, TeddyBear, Daisy, CoffeeCup, GraduationCap, Initial,
    Custom
}

public static class CharmRegistry
{
    public static readonly IReadOnlyList<CharmDefinition> All = new List<CharmDefinition>
    {
        new() { Id = "moon",       Name = "Moon",           Shape = CharmShape.Moon,          FillColor = Color.FromRgb(0xFD, 0xF3, 0xD9), StrokeColor = Color.FromRgb(0xC9, 0xB8, 0x8A), Reaction = CharmReactionType.Pulse },
        new() { Id = "star",       Name = "Star",           Shape = CharmShape.Star,          FillColor = Color.FromRgb(0xFF, 0xE9, 0x9A), StrokeColor = Color.FromRgb(0xD8, 0xB8, 0x4A), Reaction = CharmReactionType.Sparkle },
        new() { Id = "heart",      Name = "Heart",          Shape = CharmShape.Heart,         FillColor = Color.FromRgb(0xF2, 0x9C, 0xAE), StrokeColor = Color.FromRgb(0xC9, 0x6A, 0x80), Reaction = CharmReactionType.Bounce },
        new() { Id = "sunflower",  Name = "Sunflower",      Shape = CharmShape.Sunflower,     FillColor = Color.FromRgb(0xF4, 0xC5, 0x4D), StrokeColor = Color.FromRgb(0xC9, 0x9A, 0x2A), Reaction = CharmReactionType.Wiggle },
        new() { Id = "evil-eye",   Name = "Evil Eye",       Shape = CharmShape.EvilEye,       FillColor = Color.FromRgb(0x2E, 0x5E, 0x9A), StrokeColor = Color.FromRgb(0x1B, 0x3A, 0x60), Reaction = CharmReactionType.Pulse },
        new() { Id = "cherries",   Name = "Cherries",       Shape = CharmShape.Cherries,      FillColor = Color.FromRgb(0xC5, 0x2B, 0x3A), StrokeColor = Color.FromRgb(0x8E, 0x1B, 0x28), Reaction = CharmReactionType.Wiggle },
        new() { Id = "cloud",      Name = "Cloud",          Shape = CharmShape.Cloud,         FillColor = Color.FromRgb(0xF3, 0xF6, 0xFA), StrokeColor = Color.FromRgb(0xC7, 0xD2, 0xE0), Reaction = CharmReactionType.Wiggle },
        new() { Id = "saturn",     Name = "Saturn",         Shape = CharmShape.Saturn,        FillColor = Color.FromRgb(0xE8, 0xC3, 0x8A), StrokeColor = Color.FromRgb(0xB8, 0x8F, 0x54), Reaction = CharmReactionType.Spin },
        new() { Id = "mushroom",   Name = "Mushroom",       Shape = CharmShape.Mushroom,      FillColor = Color.FromRgb(0xE0, 0x4F, 0x4F), StrokeColor = Color.FromRgb(0xA8, 0x2E, 0x2E), Reaction = CharmReactionType.Bounce },
        new() { Id = "cat",        Name = "Cat",            Shape = CharmShape.Cat,           FillColor = Color.FromRgb(0x4A, 0x4A, 0x4A), StrokeColor = Color.FromRgb(0x2A, 0x2A, 0x2A), Reaction = CharmReactionType.Wiggle },
        new() { Id = "rainbow",    Name = "Rainbow",        Shape = CharmShape.Rainbow,       FillColor = Colors.Transparent,               StrokeColor = Colors.Transparent,             Reaction = CharmReactionType.Pulse },
        new() { Id = "disco-ball", Name = "Disco Ball",     Shape = CharmShape.DiscoBall,     FillColor = Color.FromRgb(0xC9, 0xCE, 0xD6), StrokeColor = Color.FromRgb(0x8A, 0x90, 0x99), Reaction = CharmReactionType.Spin },
        new() { Id = "paw",        Name = "Paw",            Shape = CharmShape.Paw,           FillColor = Color.FromRgb(0xB0, 0x8D, 0x6A), StrokeColor = Color.FromRgb(0x80, 0x60, 0x44), Reaction = CharmReactionType.Bounce },
        new() { Id = "bow",        Name = "Bow",            Shape = CharmShape.Bow,           FillColor = Color.FromRgb(0xE8, 0x9A, 0xB4), StrokeColor = Color.FromRgb(0xB8, 0x5F, 0x80), Reaction = CharmReactionType.Wiggle },
        new() { Id = "teddy-bear", Name = "Teddy Bear",     Shape = CharmShape.TeddyBear,     FillColor = Color.FromRgb(0xC9, 0x9A, 0x6A), StrokeColor = Color.FromRgb(0x8F, 0x68, 0x44), Reaction = CharmReactionType.Bounce },
        new() { Id = "daisy",      Name = "Daisy",          Shape = CharmShape.Daisy,         FillColor = Color.FromRgb(0xFF, 0xFF, 0xF5), StrokeColor = Color.FromRgb(0xD8, 0xD8, 0xC8), Reaction = CharmReactionType.Wiggle },
        new() { Id = "coffee-cup", Name = "Coffee Cup",     Shape = CharmShape.CoffeeCup,     FillColor = Color.FromRgb(0xE8, 0xDC, 0xC8), StrokeColor = Color.FromRgb(0xA0, 0x7A, 0x50), Reaction = CharmReactionType.Pulse },
        new() { Id = "grad-cap",   Name = "Graduation Cap", Shape = CharmShape.GraduationCap, FillColor = Color.FromRgb(0x2A, 0x2A, 0x3A), StrokeColor = Color.FromRgb(0x10, 0x10, 0x18), Reaction = CharmReactionType.Spin },
        new() { Id = "initial",    Name = "Initial",        Shape = CharmShape.Initial,       FillColor = Color.FromRgb(0xD4, 0xAF, 0x37), StrokeColor = Color.FromRgb(0xA0, 0x80, 0x20), Reaction = CharmReactionType.Pulse },
    };

    public static CharmDefinition GetById(string id, double size)
    {
        CharmDefinition baseDefinition = All.FirstOrDefault(c => c.Id == id) ?? All[0];

        return new CharmDefinition
        {
            Id = baseDefinition.Id,
            Name = baseDefinition.Name,
            Shape = baseDefinition.Shape,
            Size = size,
            FillColor = baseDefinition.FillColor,
            StrokeColor = baseDefinition.StrokeColor,
            Reaction = baseDefinition.Reaction,
            InitialLetter = baseDefinition.InitialLetter
        };
    }
}