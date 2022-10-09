using System.Collections.ObjectModel;

namespace BonfireCore.Models;

public class ProfileFiles
{
    public ObservableCollection<ProfileFile> Collection { get; set; } = new();

    public int Selected { get; set; } = 0;
    
    public void LoadFiles(string folderPath)
    {
        Collection.Clear();
        var collection = Directory.GetFiles(folderPath, "*.bin", SearchOption.TopDirectoryOnly)
            .Select(file => new ProfileFile(file)).ToList();
        Directory.CreateDirectory(folderPath);
        foreach (var item in collection) 
            Collection.Add(item);
    }
}