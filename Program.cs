using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Web;

var dialogues = Directory.EnumerateDirectories(Read("Enter full path to 'messages' folder in exported data")).ToList();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // not sure what it is for, but I guess it's something important

VKExport vkmsgs = new();
vkmsgs.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // grab the timestamp of when this export was converted to JSON
dialogues.ForEach((z) =>
{
    string dirname = z.Split('\\').Last();

    Console.WriteLine($"Processing {dirname}...");

    Dialogue dlg = new Dialogue()
    {
        id = int.Parse(dirname),
        messages = new()
    };

    string title = null;

    var msgFragments = Directory.EnumerateFiles(z).ToList();
    msgFragments.ForEach((x) =>
    {
        string contents = ReadUTF8(x);

        HtmlDocument htmlSnippet = new HtmlDocument();
        htmlSnippet.LoadHtml(contents);

        if (title is null)
        {
            var l = htmlSnippet.DocumentNode.SelectSingleNode("//div[@class='ui_crumb']");
            title = l.InnerText;
        }

        var msgContainer = htmlSnippet.DocumentNode.SelectSingleNode("//div[@class='wrap_page_content']");
        msgContainer.ChildNodes.ToList().ForEach((c) =>
        {
            if (c.Name != "div" || !c.HasClass("item")) return;

            var msg = new Message();
            var header = c.SelectSingleNode(".//div[@class='message__header']");

            var senderNode = header.FirstChild;

            // figuring out SENDER
            var hrefVal = senderNode.GetAttributeValue<string>("href", "https://vk.com/id0");
            string needle = "https://vk.com/id";
            if (hrefVal.Contains("club")) needle = "https://vk.com/club";
            if (hrefVal.Contains("public")) needle = "https://vk.com/public";
            if (hrefVal.Contains("event")) needle = "https://vk.com/event";
            int indx = hrefVal.IndexOf(needle);
            int senderId = int.Parse(hrefVal.Substring(indx + needle.Length));
            if (needle.Contains("club") ||
                needle.Contains("public")) senderId = -senderId;
            if (needle.Contains("event")) senderId += 1000000000;
            msg.sender = senderId;


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
            if (txt is not null && txt.NextSibling is not null && txt.NextSibling.FirstChild is not null && txt.NextSibling.FirstChild.Name == "#text")
            {
                msg.body = txt.NextSibling.FirstChild.InnerText.Trim('\n');
            }

            // figuring out whether there are attachments
            var attachments = c.SelectNodes(".//div[@class='attachment']");
            msg.attachment = new();
            if (attachments is not null)
            {
                attachments.ToList().ForEach((c) =>
                {
                    var desc = c.SelectSingleNode(".//div[@class='attachment__description']");
                    var link = c.SelectSingleNode(".//a[@class='attachment__link']");

                    var dText = desc.InnerText;
                    if (string.IsNullOrEmpty(dText) || dText.Length < 4) dText = "Неизвестно";
                    var url = link is null ? "N/A" : link.GetAttributeValue("href", "N/A");

                    msg.attachment.Add(new Attachment()
                    {
                        name = dText,
                        url = url
                    });
                });
            }

            // figuring out invitations
            var invitations = c.SelectSingleNode(".//a[@class='im_srv_lnk ']");
            if (invitations is not null)
            {
                msg.attachment.Add(new Attachment()
                {
                    name = "Действие",
                    url = c.SelectSingleNode(".//div[@class='kludges']").InnerText
                });
            }

            dlg.messages.Add(msg);
        });
    });
    dlg.title = title;
    vkmsgs.dialogues.Add(dlg);
});
File.WriteAllText("export.json", JsonConvert.SerializeObject(vkmsgs));
Console.WriteLine("Export complete!");

string ReadUTF8(string file)
{
    Encoding win1251 = Encoding.GetEncoding("Windows-1251");

    return HttpUtility.HtmlDecode(win1251.GetString(File.ReadAllBytes(file)));
}

string Read(string c)
{
    Console.Write($"{c}:");
    return Console.ReadLine();
}

public class VKExport
{
    public List<Dialogue> dialogues = new();
    public long timestamp;
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