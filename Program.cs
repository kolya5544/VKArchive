using HtmlAgilityPack;
using System.Globalization;
using System.Text;
using System.Web;

var dialogues = Directory.EnumerateDirectories(@"C:\Users\kolya5544\Downloads\Archive\messages").ToList();
var myId = 127172472;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

List<Dialogue> vkmsgs = new();
dialogues.ForEach((z) =>
{
    string dirname = z.Split('\\').Last();

    Console.WriteLine($"Processing {dirname}...");

    Dialogue dlg = new Dialogue()
    {
        id = int.Parse(dirname),
        messages = new()
    };

    var msgFragments = Directory.EnumerateFiles(z).ToList();
    msgFragments.ForEach((x) =>
    {
        string contents = ReadUTF8(x);

        HtmlDocument htmlSnippet = new HtmlDocument();
        htmlSnippet.LoadHtml(contents);

        List<Message> messages = new();
        var msgContainer = htmlSnippet.DocumentNode.SelectSingleNode("//div[@class='wrap_page_content']");
        msgContainer.ChildNodes.ToList().ForEach((c) =>
        {
            if (c.Name != "div" || !c.HasClass("item")) return;

            var msg = new Message();
            var header = c.SelectSingleNode(".//div[@class='message__header']");

            var senderNode = header.FirstChild;

            // figuring out SENDER
            if (senderNode is null) { 
                msg.sender = myId; 
            } else {
                var hrefVal = senderNode.GetAttributeValue<string>("href", "https://vk.com/id0");
                string needle = "https://vk.com/id";
                if (hrefVal.Contains("club")) needle = "https://vk.com/club";
                if (hrefVal.Contains("public")) needle = "https://vk.com/public";
                if (hrefVal.Contains("event")) needle = "https://vk.com/event";
                int indx = hrefVal.IndexOf(needle);
                int senderId = int.Parse(hrefVal.Substring(indx+needle.Length));
                if (needle.Contains("club") ||
                    needle.Contains("public")) senderId = -senderId;
                if (needle.Contains("event")) senderId += 1000000000;
                msg.sender = senderId;
            }

            // figuring out messageId
            int msgId = int.Parse(c.SelectSingleNode(".//div[@class='message']").GetAttributeValue<string>("data-id", "0"));
            msg.messageId = msgId;

            // figuring out timestamp
            string tsPrint = header.InnerText.Split(',').Last().Trim(',').Trim().Replace(" в ", " ").Replace("Вы, ", "").Replace(" (ред.)", "")
            .Replace("янв", "января")
            .Replace("фев", "февраля")
            .Replace("мар", "марта")
            .Replace("апр", "апреля")
            .Replace("июн", "июня")
            .Replace("июл", "июля")
            .Replace("авг", "августа")
            .Replace("сен", "сентября")
            .Replace("окт", "октября")
            .Replace("ноя", "ноября")
            .Replace("дек", "декабря");
            var dto = DateTimeOffset.Parse(tsPrint).ToOffset(TimeSpan.Zero);
            msg.timestamp = dto.ToUnixTimeSeconds();

            // figuring out body
            var txt = header.NextSibling;
            if (txt is not null && txt.NextSibling is not null && txt.NextSibling.FirstChild.Name == "#text")
            {
                msg.body = txt.NextSibling.FirstChild.InnerText.Trim('\n');
            }
            
            // figuring out whether there are attachments
            // todo... just like everything else :D
        });
    });
});

string ReadUTF8(string file)
{
    Encoding win1251 = Encoding.GetEncoding("Windows-1251");

    return HttpUtility.HtmlDecode(win1251.GetString(File.ReadAllBytes(file)));
}

public class Dialogue
{
    public int id;
    public string title;
    public List<Message> messages;
}

public class Message
{
    public int sender;
    public int messageId;
    public long timestamp;
    public string body = "";
    public List<Attachment> attachment;
}

public class Attachment
{
    public string name;
    public string url;
}