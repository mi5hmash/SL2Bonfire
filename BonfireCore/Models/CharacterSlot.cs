namespace BonfireCore.Models;

public class CharacterSlot
{
    public int ID { get; }

    public bool IsOccupied { get; }

    public int Offset { get; }

    public string CharacterName { get; set; } = "<empty>";

    public CharacterSlot(int id, bool isOccupied, int offset)
    {
        ID = id;
        IsOccupied = isOccupied;
        Offset = offset;
    }
}