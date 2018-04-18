using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Items<TKey>
{
    private int time = 0;

    public int Now => Interlocked.Increment(ref time);

    private class Item
    {
        public TKey Id { get; set; }

        public bool IsDeleted { get; set; }

        public int UpdatedAt { get; set; }
    }

    private readonly ICollection<Item> items = new List<Item>();

    public void Add(TKey id)
    {
        var item = items.SingleOrDefault(i => i.Id.Equals(id));
        if (item != null)
            throw new Exception($"Элемент с ключом '{id}' уже добавлен");

        items.Add(new Item
        {
            Id = id,
            UpdatedAt = Now
        });
    }

    public void Update(TKey id)
    {
        var item = items.SingleOrDefault(i => i.Id.Equals(id));
        if (item == null)
            throw new Exception($"Элемент с ключом '{id}' не найден");

        if (item.IsDeleted)
        {
            item.IsDeleted = false;
            item.UpdatedAt = Now;
        }
    }

    public void Delete(TKey id)
    {
        var item = items.SingleOrDefault(i => i.Id.Equals(id));
        if (item == null)
            throw new Exception($"Элемент с ключом '{id}' не найден");

        if (!item.IsDeleted)
        {
            item.IsDeleted = true;
            item.UpdatedAt = Now;
        }
    }

    public IEnumerable<TKey> GetActiveItems()
    {
        return items.Where(i => !i.IsDeleted).OrderByDescending(i => i.UpdatedAt).Select(i => i.Id);
    }

    public IEnumerable<TKey> GetDeletedItems()
    {
        return items.Where(i => i.IsDeleted).OrderBy(i => i.UpdatedAt).Select(i => i.Id);
    }

    public void SetActiveItems(IEnumerable<TKey> ids)
    {
        throw new NotImplementedException("Метод Items.SetActiveItems не реализован, его нужно реализовать");
    }
}

class ItemsTest
{
    static void Main(String[] args)
    {
        try
        {
            var items = new Items<int>();
            items.Add(0);
            items.Add(1);
            items.Add(2);
            items.Delete(2);
            items.Add(3);
            items.Delete(3);

            items.SetActiveItems(new[] {1, 2, 4});

            items.Add(5);

            var expected = new[] {5, 4, 2, 1};
            var actual = items.GetActiveItems();
            if (!expected.SequenceEqual(actual))
                throw new Exception(
                    $"Неверный ответ. Ожидалось '{string.Join(", ", expected)}'. Получено '{string.Join(", ", actual)}'");

            expected = new[] {3, 0};
            actual = items.GetDeletedItems();
            if (!expected.SequenceEqual(actual))
                throw new Exception(
                    $"Неверный ответ. Ожидалось '{string.Join(", ", expected)}'. Получено '{string.Join(", ", actual)}'");

            var items2 = new Items<int>();
            for (var i = 0; i < 10000; i++)
                items2.Add(i);

            if (Task.Run(() =>
                    items2.SetActiveItems(Enumerable.Range(5000, 15000).OrderBy(a => Guid.NewGuid())))
                .Wait(TimeSpan.FromSeconds(1)))
            {
                if (!Enumerable.Range(5000, 15000).SequenceEqual(items2.GetActiveItems().OrderBy(x => x)))
                    throw new Exception($"Неверный ответ");
                else
                    Console.WriteLine("Задача решена");
            }
            else
                throw new Exception("Задача не решена оптимально");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Задача не решена: {e.Message}!");
        }
    }
}
