public class Examples
{
    public static void DialogueDistribution(VKExport vke)
    {
        Dictionary<Dialogue, int> dist = new();
        vke.dialogues.ForEach((z) =>
        {
            dist.Add(z, z.messages.Count);
        });
        var totalMessages = vke.dialogues.Sum(z => z.messages.Count);
        var sorted = dist.ToList().OrderByDescending((z) => z.Value).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            var s = sorted[i];
            Console.WriteLine($"[#{i + 1}] {s.Key.title} - {s.Value} messages - {Math.Round((double)s.Value / totalMessages * 100, 2)}%");
        }
    }

    public static void MyMessagesDistribution(VKExport vke)
    {
        Dictionary<Dialogue, int> dist = new();
        vke.dialogues.ForEach((z) =>
        {
            dist.Add(z, z.messages.Count((x) => x.sender == 0));
        });
        var totalMessages = vke.dialogues.Sum(z => z.messages.Count);
        var sorted = dist.ToList().OrderByDescending((z) => z.Value).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            var s = sorted[i];
            Console.WriteLine($"[#{i + 1}] {s.Key.title} - {s.Value} messages sent by me - {Math.Round((double)s.Value / s.Key.messages.Count * 100, 2)}% (из {s.Key.messages.Count})");
        }
    }
}