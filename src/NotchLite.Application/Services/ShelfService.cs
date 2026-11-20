using NotchLite.Core.Interfaces;
using NotchLite.Core.Models;

namespace NotchLite.Application.Services;

public sealed class ShelfService : IShelfService
{
    private readonly List<ShelfFileItem> _items = new();

    public IReadOnlyList<ShelfFileItem> Items => _items.AsReadOnly();

    public event EventHandler? ItemsChanged;

    public void AddFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path) && !_items.Any(i => i.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
            {
                var fi = new FileInfo(path);
                _items.Add(new ShelfFileItem(path, fi.Name, fi.Length, DateTime.Now));
            }
        }
        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveFile(string path)
    {
        _items.RemoveAll(i => i.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _items.Clear();
        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }
}
