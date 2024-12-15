using System.IO;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Dictionary;
using LinePutScript.Localization.WPF;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

#pragma warning disable CS8604 // 引用类型参数可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
/// <summary>
/// 模组加载器
/// </summary>
public class ModLoader
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; }

    /// <summary>
    /// 如果是上传至Steam,则为SteamUserID
    /// </summary>
    public long AuthorID { get; }

    /// <summary>
    /// 上传至Steam的ItemID
    /// </summary>
    public ulong ItemID { get; }

    /// <summary>
    /// 简介
    /// </summary>
    public string Intro { get; }

    /// <summary>
    /// 模组路径
    /// </summary>
    public DirectoryInfo ModPath { get; }

    /// <summary>
    /// 支持的游戏版本
    /// </summary>
    public int GameVer { get; }

    /// <summary>
    /// 版本
    /// </summary>
    public int Ver { get; }

    /// <summary>
    /// 标签
    /// </summary>
    public HashSet<string> Tag { get; } = [];

    /// <summary>
    /// 缓存数据
    /// </summary>
    public DateTime CacheDate { get; } = DateTime.MinValue;

    /// <summary>
    /// 宠物列表
    /// </summary>
    public List<PetLoader> Pets { get; } = [];

    /// <summary>
    /// 食物列表
    /// </summary>
    public List<Food> Foods { get; } = [];

    /// <summary>
    /// 低状态文本列表
    /// </summary>
    public List<LowText> LowTexts { get; } = [];

    /// <summary>
    /// 点击文本列表
    /// </summary>
    public List<ClickText> ClickTexts { get; } = [];

    /// <summary>
    /// 选择文本列表
    /// </summary>
    public List<SelectText> SelectTexts { get; } = [];

    /// <summary>
    /// I18n资源
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> I18nDatas { get; } = [];

    public ModLoader(DirectoryInfo path)
    {
        ModPath = path;
        var modlps = new LPS(File.ReadAllText(path.FullName + @"\info.lps"));

        Name = modlps.FindLine("vupmod").Info;
        Intro = modlps.FindLine("intro").Info;
        GameVer = modlps.FindSub("gamever").InfoToInt;
        Ver = modlps.FindSub("ver").InfoToInt;
        Author = modlps.FindSub("author").Info.Split('[').First();
        if (modlps.FindLine("authorid") != null)
            AuthorID = modlps.FindLine("authorid").InfoToInt64;
        else
            AuthorID = 0;
        if (modlps.FindLine("itemid") != null)
            ItemID = Convert.ToUInt64(modlps.FindLine("itemid").info);
        else
            ItemID = 0;
        CacheDate = modlps.GetDateTime("cachedate", DateTime.MinValue);

        //MOD未加载时支持翻译
        foreach (var line in modlps.FindAllLine("lang"))
        {
            if (I18nDatas.TryGetValue(line.Info, out var datas) is false)
                datas = I18nDatas[line.Info] = [];
            foreach (var sub in line)
            {
                if (sub.Name == Name)
                    datas[Name] = sub.Info;
                else if (sub.Name == Intro)
                    datas[Intro] = sub.Info;
            }
        }
        DirectoryInfo? langDirectory = null;
        foreach (DirectoryInfo di in path.EnumerateDirectories())
        {
            switch (di.Name.ToLower())
            {
                case "pet":
                    //宠物模型
                    Tag.Add("pet");
                    foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                    {
                        var lps = new LpsDocument(File.ReadAllText(fi.FullName));
                        if (lps.First().Name.Equals("pet", StringComparison.OrdinalIgnoreCase))
                        {
                            var name = lps.First().Info;
                            var pet = new PetLoader(lps, di);
                            if (pet.Name is null)
                                break;
                            Pets.Add(pet);
                        }
                    }
                    break;
                case "food":
                    Tag.Add("food");
                    foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                    {
                        var tmp = new LpsDocument(File.ReadAllText(fi.FullName));
                        foreach (ILine li in tmp)
                        {
                            var food = LPSConvert.DeserializeObject<Food>(li);
                            if (food.Name is null)
                                break;
                            var imagePath =
                                $"{path.FullName}\\image\\food\\{(string.IsNullOrWhiteSpace(food.Image) ? food.Name : food.Image)}.png";
                            if (File.Exists(imagePath))
                                food.Image = imagePath;
                            Foods.Add(food);
                            //string tmps = li.Find("name").info;
                            //mw.Foods.RemoveAll(x => x.Id == tmps);
                            //mw.Foods.Add(LPSConvert.DeserializeObject<Food>(li));
                        }
                    }
                    break;
                case "image":
                    Tag.Add("image");
                    break;
                case "text":
                    Tag.Add("text");
                    foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                    {
                        var tmp = new LpsDocument(File.ReadAllText(fi.FullName));
                        foreach (ILine li in tmp)
                        {
                            switch (li.Name.ToLower())
                            {
                                case "lowfoodtext":

                                    LowTexts.Add(LPSConvert.DeserializeObject<LowText>(li));
                                    break;
                                case "lowdrinktext":
                                    LowTexts.Add(LPSConvert.DeserializeObject<LowText>(li));
                                    break;
                                case "clicktext":
                                    ClickTexts.Add(LPSConvert.DeserializeObject<ClickText>(li));
                                    break;
                                case "selecttext":
                                    SelectTexts.Add(LPSConvert.DeserializeObject<SelectText>(li));
                                    break;
                            }
                        }
                    }
                    break;
                case "lang":
                    Tag.Add("lang");
                    langDirectory = di;
                    foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                    {
                        LocalizeCore.AddCulture(
                            Path.GetFileNameWithoutExtension(fi.Name),
                            new LPS_D(File.ReadAllText(fi.FullName))
                        );
                    }
                    foreach (DirectoryInfo dis in di.EnumerateDirectories())
                    {
                        foreach (FileInfo fi in dis.EnumerateFiles("*.lps"))
                        {
                            LocalizeCore.AddCulture(
                                dis.Name,
                                new LPS_D(File.ReadAllText(fi.FullName))
                            );
                        }
                    }
                    break;
            }
        }
        if (langDirectory is null)
            return;
        foreach (var dis in langDirectory.EnumerateDirectories())
        {
            I18nDatas.TryAdd(dis.Name, []);
            foreach (FileInfo fi in dis.EnumerateFiles("*.lps"))
            {
                var lps = new LPS(File.ReadAllText(fi.FullName));
                foreach (var item in lps)
                    I18nDatas[dis.Name].TryAdd(item.Name, item.Info);
            }
        }
    }

    //public void WriteFile()
    //{
    //    var lps = new LpsDocument(File.ReadAllText(ModPath.FullName + @"\info.lps"));
    //    lps.FindLine("vupmod").Info = Name;
    //    lps.FindLine("intro").Info = Intro;
    //    lps.FindSub("gamever").InfoToInt = GameVer;
    //    lps.FindSub("ver").InfoToInt = Ver;
    //    lps.FindSub("author").Info = Author;
    //    lps.FindorAddLine("authorid").InfoToInt64 = AuthorID;
    //    lps.FindorAddLine("itemid").info = ItemID.ToString();
    //    File.WriteAllText(ModPath.FullName + @"\info.lps", lps.ToString());
    //}
}
